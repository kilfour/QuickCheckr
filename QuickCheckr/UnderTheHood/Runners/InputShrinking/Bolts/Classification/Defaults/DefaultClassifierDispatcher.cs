using QuickCheckr.UnderTheHood.Dispatching;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Defaults;

public class DefaultClassifierDispatcher()
        : FirstMatchDispatcher<ShrinkingInfo, ShrinkingContext, InputClassification>(
            [ new PrimitiveClassifier()
            , new EnumClassifier()
            , new EnumerableClassifier()
            , new ObjectClassifier() ],
            new FallbackClassifier());
