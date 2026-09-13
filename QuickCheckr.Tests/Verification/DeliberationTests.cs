using QuickCheckr.Diagnostics;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood;
using QuickFuzzr;

namespace QuickCheckr.Tests.Verification;

public class DeliberationTests
{
    [Fact]
    public void MultipleClausesContributeToTheScore()
    {
        var check =
            from first in Checkr.Deliberate(() => 2)
            from second in Checkr.Deliberate(() => 3)
            from until in Checkr.DeliberateUntil(5)
            from expect in Checkr.Expect("always fails", () => false)
            select Case.Closed;

        var caseFile = new CheckrRunner(
            check,
            new CheckrRunnerConfig(CheckrConfig.Default()),
            1,
            AutopsyProbe.Empty).Run();

        Assert.Equal(5, caseFile.DeliberationScore);
        Assert.Equal(5, caseFile.DeliberationTarget);
    }

    [Fact]
    public void MultipleTargetsInOneExecutionAreRejected()
    {
        var check =
            from deliberate in Checkr.Deliberate(() => 5)
            from first in Checkr.DeliberateUntil(2)
            from second in Checkr.DeliberateUntil(3)
            from expect in Checkr.Expect("always fails", () => false)
            select Case.Closed;

        var exception = Assert.Throws<InvalidOperationException>(() =>
            new CheckrRunner(
                check,
                new CheckrRunnerConfig(CheckrConfig.Default()),
                1,
                AutopsyProbe.Empty).Run());

        Assert.Equal(
            "Checkr.DeliberateUntil may only be used once per execution.",
            exception.Message);
    }

    [Fact]
    public void ScoreIsOnlyEvaluatedDuringTheCaseSelectionRun()
    {
        var scoreCalls = 0;
        var check =
            from deliberate in Checkr.Deliberate(() => ++scoreCalls)
            from expect in Checkr.Expect("always fails", () => false)
            select Case.Closed;

        new CheckrRunner(
            check,
            new CheckrRunnerConfig(CheckrConfig.Default()),
            1,
            AutopsyProbe.Empty).Run();

        Assert.Equal(1, scoreCalls);
    }

    [Fact]
    public void PassingCasesDoNotEvaluateTheScore()
    {
        var scoreCalls = 0;
        var check =
            from deliberate in Checkr.Deliberate(() => ++scoreCalls)
            from expect in Checkr.Expect("always passes", () => true)
            select Case.Closed;

        new CheckrRunner(
            check,
            new CheckrRunnerConfig(CheckrConfig.Default()),
            1,
            AutopsyProbe.Empty).Run();

        Assert.Equal(0, scoreCalls);
    }

    [Fact]
    public void ScoreReflectsTheCommittedReducedInput()
    {
        var scoreCalls = 0;
        var check =
            from number in Checkr.Input("number", Fuzzr.Constant(999))
            from deliberate in Checkr.Deliberate(() =>
            {
                scoreCalls++;
                return number;
            })
            from expect in Checkr.Expect("below 10", () => number < 10)
            select Case.Closed;
        var config = CheckrConfig.Default() with { ShrinkMode = ShrinkMode.All };

        var caseFile = new CheckrRunner(
            check,
            new CheckrRunnerConfig(config),
            1,
            AutopsyProbe.Empty).Run();

        Assert.Equal(10, caseFile.DeliberationScore);
        Assert.Equal(1, scoreCalls);
    }

    [Fact]
    public void InlineDeliberationSelectsCasesAndStopsAtTarget()
    {
        var scores = new Queue<int>([4, 2, 1]);
        var currentScore = 0;
        CheckrOf<Case> check = state =>
        {
            if (!state.EvidenceRecorder.IsReplay)
                currentScore = scores.Dequeue();

            Checkr.Deliberate(() => currentScore)(state);
            Checkr.DeliberateUntil(2)(state);
            return Checkr.Expect("always fails", () => false)(state);
        };

        var findings = CaseOfficer.Investigate(
            check,
            10.Runs(),
            1.ExecutionsPerRun(),
            new CheckrRunnerConfig(CheckrConfig.Default()),
            AutopsyProbe.Empty);

        Assert.Equal(2, findings.NumberOfRuns);
        Assert.Equal(2, findings.FalsifyingCaseFile!.DeliberationScore);
    }

    [Fact]
    public void ConfiguredDeliberationOverridesInlineDeliberation()
    {
        var runs = 0;
        var scoreCalls = 0;
        CheckrOf<Case> check = state =>
        {
            if (!state.EvidenceRecorder.IsReplay)
                runs++;

            Checkr.Deliberate(() => ++scoreCalls)(state);
            Checkr.DeliberateUntil(0)(state);
            return Checkr.Expect("always fails", () => false)(state);
        };
        var config = CheckrConfig.Default() with
        {
            Deliberation = Deliberation.MinimumExecutions()
        };

        var findings = CaseOfficer.Investigate(
            check,
            3.Runs(),
            1.ExecutionsPerRun(),
            new CheckrRunnerConfig(config),
            AutopsyProbe.Empty);

        Assert.Equal(3, findings.NumberOfRuns);
        Assert.Equal(3, runs);
        Assert.Equal(0, scoreCalls);
    }
}
