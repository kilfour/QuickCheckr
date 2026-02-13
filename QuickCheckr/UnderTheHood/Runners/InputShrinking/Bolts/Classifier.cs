using QuickCheckr.UnderTheHood.Dispatching;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

public abstract class Classifier : FirstMatchHandler<ShrinkingInfo, ShrinkingContext, InputShrinkingClassification>;
