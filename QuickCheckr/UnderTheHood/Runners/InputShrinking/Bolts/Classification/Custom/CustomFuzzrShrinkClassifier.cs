using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;
using QuickFuzzr;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;

internal sealed class CustomFuzzrShrinkClassifier : CustomClassifier<IClassificationOptionFuzzr>
{
    protected override async ValueTask<InputClassification> GetClassificationAsync(IClassificationOptionFuzzr option, CustomShrinkingContext customContext)
        => await new CustomClassifierRunner(a => GetValues(customContext.Context, option)).ClassifyAsync(customContext.Info, customContext.Context).ConfigureAwait(false);

    private static IEnumerable<object> GetValues(ShrinkingContext context, IClassificationOptionFuzzr option)
        => option.GetFuzzr()
            .Unique(Guid.NewGuid())
            .Many(option.Times)(context.State.FuzzrState).Value.Cast<object>();
}
