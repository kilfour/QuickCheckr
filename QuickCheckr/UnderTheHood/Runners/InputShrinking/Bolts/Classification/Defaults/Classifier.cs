using QuickCheckr.UnderTheHood.Dispatching;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Defaults;

public abstract class Classifier : FirstMatchHandler<ShrinkingInfo, ShrinkingContext, InputShrinkingClassification>;
