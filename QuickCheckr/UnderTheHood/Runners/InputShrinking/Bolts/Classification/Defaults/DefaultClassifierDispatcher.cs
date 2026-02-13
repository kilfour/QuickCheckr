using QuickCheckr.UnderTheHood.Dispatching;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Defaults;

public class DefaultClassifierDispatcher()
        : FirstMatchDispatcher<ShrinkingInfo, ShrinkingContext, InputShrinkingClassification>(
            [ new PrimitiveShrinkClassifier()
            , new EnumShrinkClassifier()
            , new EnumerableShrinkClassifier()
            , new ObjectShrinkClassifier() ],
            new FallbackClassifier());
