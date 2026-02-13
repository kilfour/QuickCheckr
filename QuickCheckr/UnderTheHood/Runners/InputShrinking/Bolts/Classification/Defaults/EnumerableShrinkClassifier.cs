
using System.Collections;
using QuickCheckr.UnderTheHood.Dispatching;
using QuickCheckr.UnderTheHood.Docketing;
using QuickCheckr.UnderTheHood.Memories.Structure.Lensing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;
using QuickCheckr.UnderTheHood.Warnings;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Defaults;

public class EnumerableShrinkClassifier : Classifier
{
    public override HandlerResult<InputShrinkingClassification> TryHandle(ShrinkingInfo info, ShrinkingContext context)
    {
        if (!CanHandle(info)) return Unhandled;
        return HandlerResult.Handled(Handle(info, context));
    }

    private ShrinkClassifierSelector selector = new();
    public Dictionary<int, int>? Mapper { get; set; }

    private int GetMappedIndex(int index) => Mapper == null ? index : Mapper[index];

    private static bool CanHandle(ShrinkingInfo info) => typeof(IEnumerable).IsAssignableFrom(info.Type);

    private InputShrinkingClassification Handle(ShrinkingInfo info, ShrinkingContext context)
    {
        var verifier = new Verifier(context.State, context.Check);
        var theList = CloneList.AsOriginalType(info.Value!);
        var values = new List<InputShrinkingClassification>();
        var elementCount = theList.Count;
        int index = 0;
        int indexKey = -1;
        while (index < theList.Count)
        {
            indexKey++;
            var ix = index;
            var before = theList[ix];
            var valueType = before!.GetType();
            var elementType = valueType;
            theList.RemoveAt(ix);
            var substitute = CloneList.GetSubstituteForType(info.Type, theList);
            if (verifier.RunFailed(info.ExecutionId, info.Key, substitute!))
            {
                var clue = InputShrinkingClue.Remove<int>(info.ExecutionId, $"{info.Path}.{indexKey}", info.Value, theList);
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
                    var childInfo = info with { Value = before, Path = $"{info.Path}.{GetMappedIndex(index)}", Type = elementType };
                    values.Add(selector.Classify(childInfo, context));
                }
            }
            index++;
        }
        if (elementCount == 0 || values.Any(a => a is not ElementAssumedIrrelevant))
        {
            return new CollectionRelevant(values);
        }
        if (elementCount != 0)
        {
            if (!context.State.Memory.HasShrinker(info.ExecutionId, info.Key, info.Type))
                context.State.Memory.StoreWarning(info.Path, () => new EmptyCollectionWarning(info.Key, info.Path));
        }
        return new InputAssumedIrrelevant();
    }
}