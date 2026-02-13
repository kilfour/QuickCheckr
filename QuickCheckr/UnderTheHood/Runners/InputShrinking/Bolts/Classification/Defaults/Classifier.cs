using QuickCheckr.UnderTheHood.Dispatching;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Defaults;

public abstract class Classifier : FirstMatchHandler<ShrinkingInfo, ShrinkingContext, InputShrinkingClassification>;
