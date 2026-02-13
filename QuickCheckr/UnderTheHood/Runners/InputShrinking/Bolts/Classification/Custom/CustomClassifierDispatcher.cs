using QuickCheckr.UnderTheHood.Dispatching;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Defaults;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;

public class CustomClassifierDispatcher(DefaultClassifierDispatcher defaults)
        : FirstMatchCollectionDispatcher<IShrinkOption, CustomShrinkingContext, InputShrinkingClassification>(
            [ new CustomFuzzrShrinkClassifier()
            , new CustomFunctionShrinkClassifier()
            , new CustomDefaultShrinkClassifier(defaults)
            , new CustomKeepShrinkClassifier()
            , new CustomNoneShrinkClassifier()
            ], new CustomFallbackClassifier());
