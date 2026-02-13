using QuickFuzzr;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;

public class CustomFuzzrShrinkClassifier : CustomClassifier<IShrinkOptionFuzzr>
{
    protected override InputShrinkingClassification GetClassification(IShrinkOptionFuzzr option, CustomShrinkingContext customContext)
        => new CustomShrinkClassifier(a => GetValues(customContext.Context, option)).Classify(customContext.Info, customContext.Context);

    private static IEnumerable<object> GetValues(ShrinkingContext context, IShrinkOptionFuzzr option)
        => option.GetFuzzr()
            .Unique(Guid.NewGuid())
            .Many(option.Times)(context.State.FuzzrState).Value.Cast<object>();
}
