using QuickCheckr.UnderTheHood.Dispatching;
using QuickCheckr.UnderTheHood.Docketing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;
using QuickCheckr.UnderTheHood.Warnings;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Defaults;

public class FallbackClassifier : FirstMatchFallback<ShrinkingInfo, ShrinkingContext, InputClassification>
{
    public override InputClassification Handle(ShrinkingInfo info, ShrinkingContext context)
    {
        var clue = InputShrinkingClue.Keep<NoShrinkerFound>(info.ExecutionId, info.Path, info.Value, context.State.InputPrinter.IntroduceThis(info.Value));
        context.State.Docket.Record(new InputShrinkingClueFound(clue));
        context.State.Memory.StoreWarning(info.Path, () => new CantShrinkWarning(info.Type, info.Path));
        return new InputRelevant(context.State.InputPrinter.IntroduceThis(info.Value));
    }
    public class NoShrinkerFound { }
}
