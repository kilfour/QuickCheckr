using QuickCheckr.UnderTheHood.Dispatching;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.StructuralCollapse;

public class FallbackCollapser : FirstMatchFallback<ShrinkingInfo, ShrinkingContext, int>
{
    public override int Handle(ShrinkingInfo info, ShrinkingContext context) => 0;
}
