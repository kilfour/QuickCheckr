using QuickCheckr.Diagnostics;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Warnings;
using QuickFuzzr;

namespace QuickCheckr.Tests.Verification;

public class EmptyCollectionClassificationTests
{
    [Fact]
    public void OriginallyEmptyCollectionsProduceAnActionableWarning()
    {
        var caseFile = Run();

        Assert.Equal(0, caseFile.InputCount);
        Assert.Equal(1, caseFile.WarningsOf<EmptyCollectionWarning>(_ => 1));
    }

    [Fact]
    public void ExplicitNonEmptyWitnessesKeepEmptyCollectionsRelevant()
    {
        var caseFile = Run(Shrink.Values(new List<int> { 1 }));

        Assert.Equal(1, caseFile.InputCount);
        Assert.Equal(0, caseFile.WarningsOf<EmptyCollectionWarning>(_ => 1));
    }

    private static FilingCabinet.CaseFile Run(params Shrinker[] shrinkers)
    {
        var check =
            from list in Checkr.Input("List", Fuzzr.Constant(new List<int>()), shrinkers)
            from expectation in Checkr.Expect("list is not empty", () => list.Count != 0)
            select Case.Closed;

        return new CheckrRunner(
            check,
            new CheckrRunnerConfig(CheckrConfig.Default()),
            1,
            AutopsyProbe.Empty).Run();
    }
}
