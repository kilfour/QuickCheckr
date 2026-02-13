using QuickCheckr.UnderTheHood.Dispatching;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Collections;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Fallback;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Objects;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Primitives;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

public class CustomClassifier()
        : FirstMatchDispatcher<ShrinkingInfo, ShrinkingContext, InputShrinkingClassification>(
            [ new PrimitiveShrinkClassifier()
            , new EnumShrinkClassifier()
            , new EnumerableShrinkClassifier()
            , new ObjectShrinkClassifier() ],
            new FallbackClassifier());
