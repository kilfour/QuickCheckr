using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;
using QuickFuzzr;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;

public class CustomFuzzrShrinkClassifier : CustomClassifier<IClassificationOptionFuzzr>
{
    protected override InputShrinkingClassification GetClassification(IClassificationOptionFuzzr option, CustomShrinkingContext customContext)
        => new CustomClassifierRunner(a => GetValues(customContext.Context, option)).Classify(customContext.Info, customContext.Context);

    private static IEnumerable<object> GetValues(ShrinkingContext context, IClassificationOptionFuzzr option)
        => option.GetFuzzr()
            .Unique(Guid.NewGuid())
            .Many(option.Times)(context.State.FuzzrState).Value.Cast<object>();
}
