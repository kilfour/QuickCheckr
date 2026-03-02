using QuickCheckr.UnderTheHood.Dispatching;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.StructuralCollapse;

public class CollapserDispatcher()
        : FirstMatchDispatcher<ShrinkingInfo, ShrinkingContext, int>(
            [ new EnumerableCollapser()
            //, new ObjectCollapser() 
            ],
            new FallbackCollapser());
