
using System.Collections;
using QuickCheckr.UnderTheHood.Dispatching;
using QuickCheckr.UnderTheHood.Memories.Structure.Lensing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;
using QuickCheckr.UnderTheHood.Warnings;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Defaults;

internal sealed class EnumerableClassifier : Classifier
{
    public override async ValueTask<HandlerResult<InputClassification>> TryHandleAsync(ShrinkingInfo info, ShrinkingContext context)
    {
        if (!CanHandle(info)) return Unhandled;
        return HandlerResult.Handled(await HandleAsync(info, context).ConfigureAwait(false));
    }

    private ClassifierSelector selector = new();

    private static bool CanHandle(ShrinkingInfo info) => typeof(IEnumerable).IsAssignableFrom(info.Type);

    private async ValueTask<InputClassification> HandleAsync(ShrinkingInfo info, ShrinkingContext context)
    {
        if (info.Value == null)
        {
            context.Warn(info.Path, () => new NullValueWarning(info.Type, info.Path));
            return InputRelevant.Create(context.PrintInput(info.Value));
        }
        var verifier = new Verifier(context.State, context.Check);
        var theList = CloneList.AsOriginalType(info.Value!);
        var values = new List<InputClassification>();
        var elementType = GetElementType(info.Type);
        int index = 0;
        int indexKey = -1;
        while (index < theList.Count)
        {
            indexKey++;
            var ix = index;
            var before = theList[ix];
            theList.RemoveAt(ix);
            var substitute = CloneList.GetSubstituteForType(info.Type, theList);
            if (await verifier.RunFailedAsync(info.ExecutionId, info.Key, substitute!).ConfigureAwait(false))
            {
                values.Add(new ElementAssumedIrrelevant());
                continue;
            }
            theList.Insert(ix, before);
            substitute = CloneList.GetSubstituteForType(info.Type, theList);
            IDisposable disposable;
            try
            {
                disposable = context.State.Memory.ScopedSwap(info.ExecutionId, info.Key, substitute);
            }
            catch (CandidateRejectedException exception)
            {
                context.State.Pathologist.Inform(
                    "CandidateRejected",
                    info.Key,
                    exception.Rejection.GetType().Name);
                values.Add(InputRelevant.Create(context.PrintInput(before)));
                index++;
                continue;
            }

            using (disposable)
            {
                var swapper = new ListLens(elementType, ix);
                using (context.State.Memory.NestedValue(info.ExecutionId, swapper))
                {
                    var childInfo = info with { Parent = info, Value = before, Path = $"{info.Path}.{index}", Type = elementType };
                    var classification = await selector.ClassifyAsync(childInfo, context).ConfigureAwait(false);
                    values.Add(classification);
                    if (classification is InputRelevant { Redux.HasValue: true } relevant)
                        theList[ix] = relevant.Redux.Value!.Value;
                }
            }
            index++;
        }
        context.State.Memory.Commit(info.ExecutionId, info.Key, CloneList.GetSubstituteForType(info.Type, theList));
        if (values.Any(a => a is not ElementAssumedIrrelevant))
        {
            return new CollectionRelevant(values);
        }
        if (!context.State.Memory.HasClassifier(info))
            context.Warn(info.Path, () => new EmptyCollectionWarning(info.Key, info.Path));
        return new InputAssumedIrrelevant();
    }

    private static Type GetElementType(Type collectionType)
    {
        if (collectionType.IsArray)
            return collectionType.GetElementType()!;

        if (collectionType.IsGenericType)
            return collectionType.GetGenericArguments()[0];

        var ienum = collectionType
            .GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

        return ienum?.GetGenericArguments()[0] ?? typeof(object);
    }
}
