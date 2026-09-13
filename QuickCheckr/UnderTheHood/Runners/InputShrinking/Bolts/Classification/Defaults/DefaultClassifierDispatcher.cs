using QuickCheckr.UnderTheHood.Dispatching;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Defaults;

internal sealed class DefaultClassifierDispatcher()
        : FirstMatchDispatcher<ShrinkingInfo, ShrinkingContext, InputClassification>(
            [ new PrimitiveClassifier()
            , new EnumClassifier()
            , new EnumerableClassifier()
            , new ValueTupleClassifier()
            , new TupleClassifier()
            , new ObjectClassifier() ],
            new FallbackClassifier());
