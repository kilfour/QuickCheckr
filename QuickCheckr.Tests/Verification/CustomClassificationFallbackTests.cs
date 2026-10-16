using QuickCheckr.Diagnostics;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Defaults;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;
using QuickFuzzr;

namespace QuickCheckr.Tests.Verification;

public class CustomClassificationFallbackTests
{
    [Fact]
    public async Task EmptyChainRemainsUnclassified()
    {
        var result = await new CustomClassifierDispatcher(new DefaultClassifierDispatcher())
            .DispatchAsync([], null!);
        Assert.IsType<Unclassified>(result);
    }

    [Fact]
    public async Task UnsupportedOptionRemainsUnclassified()
    {
        var result = await new CustomClassifierDispatcher(new DefaultClassifierDispatcher())
            .DispatchAsync([new UnsupportedOption()], null!);
        Assert.IsType<Unclassified>(result);
    }

    [Fact]
    public void SuccessfulIrrelevanceIsRetained() => Assert.Equal(1, Run(Try.Values((Thing)null!)).ShrinkCount);

    [Fact]
    public void LaterKeepOverridesIrrelevance()
    {
        var result = Run(Try.Values((Thing)null!), Try.Keep<Thing>());
        Assert.Equal(0, result.ShrinkCount);
        Assert.Equal(2, result.InputCount);
    }

    [Fact]
    public void LaterNoneOverridesIrrelevance() =>
        Assert.Equal(0, Run(Try.Values((Thing)null!), Try.None<Thing>()).ShrinkCount);

    private static FilingCabinet.CaseFile Run(params ClassificationOption<Thing>[] options)
    {
        var check =
            from answer in Checkr.Input("Answer", Fuzzr.Constant(42))
            from irrelevant in Checkr.Input("Irrelevant", Fuzzr.One<Thing>(),
                Shrink.OnType<Thing>(s => s.Classify(options)))
            from expectation in Checkr.Expect("Fails", () => answer != 42)
            select Case.Closed;
        return new CheckrRunner(check, new CheckrRunnerConfig(CheckrConfig.Default()),
            1105886268, AutopsyProbe.Empty).Run();
    }

    private sealed record UnsupportedOption : ClassificationOption<Thing>;
    public class Thing;
}
