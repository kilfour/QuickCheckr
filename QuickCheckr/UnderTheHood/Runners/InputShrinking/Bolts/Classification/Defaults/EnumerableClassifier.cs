
using System.Collections;
using QuickCheckr.UnderTheHood.Dispatching;
using QuickCheckr.UnderTheHood.Docketing;
using QuickCheckr.UnderTheHood.Memories.Structure.Lensing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;
using QuickCheckr.UnderTheHood.Warnings;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Defaults;

public class EnumerableClassifier : Classifier
{
    public override HandlerResult<InputClassification> TryHandle(ShrinkingInfo info, ShrinkingContext context)
    {
        if (!CanHandle(info)) return Unhandled;
        return HandlerResult.Handled(Handle(info, context));
    }

    private ClassifierSelector selector = new();

    private static bool CanHandle(ShrinkingInfo info) => typeof(IEnumerable).IsAssignableFrom(info.Type);

    private InputClassification Handle(ShrinkingInfo info, ShrinkingContext context)
    {
        var verifier = new Verifier(context.State, context.Check);
        var theList = CloneList.AsOriginalType(info.Value!);
        var values = new List<InputClassification>();
        var elementCount = theList.Count;
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
            if (verifier.RunFailed(info.ExecutionId, info.Key, substitute!))
            {
                var clue = InputShrinkingClue.Remove<EnumerableClassifier>(info.ExecutionId, $"{info.Path}.{indexKey}", info.Value, theList);
                context.State.Docket.Record(new InputShrinkingClueFound(clue));
                values.Add(new ElementAssumedIrrelevant());
                continue;
            }
            theList.Insert(ix, before);
            substitute = CloneList.GetSubstituteForType(info.Type, theList);
            using (context.State.Memory.ScopedSwap(info.ExecutionId, info.Key, substitute))
            {
                var swapper = new ListLens(elementType, ix);
                using (context.State.Memory.NestedValue(info.ExecutionId, swapper))
                {
                    var childInfo = info with { Parent = info, Value = before, Path = $"{info.Path}.{index}", Type = elementType };
                    values.Add(selector.Classify(childInfo, context));
                }
            }
            index++;
        }
        context.State.Memory.Commit(info.ExecutionId, info.Key, CloneList.GetSubstituteForType(info.Type, theList));
        if (elementCount == 0 || values.Any(a => a is not ElementAssumedIrrelevant))
        {
            return new CollectionRelevant(values);
        }
        if (elementCount != 0)
        {
            if (!context.State.Memory.HasClassifier(info))
                context.State.Memory.StoreWarning(info.Path, () => new EmptyCollectionWarning(info.Key, info.Path));
        }
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