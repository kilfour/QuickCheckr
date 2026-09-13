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
            from until in Trackr.DeliberateUntil(5)
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
            from first in Trackr.DeliberateUntil(2)
            from second in Trackr.DeliberateUntil(3)
            from expect in Checkr.Expect("always fails", () => false)
            select Case.Closed;

        var exception = Assert.Throws<InvalidOperationException>(() =>
            new CheckrRunner(
                check,
                new CheckrRunnerConfig(CheckrConfig.Default()),
                1,
                AutopsyProbe.Empty).Run());

        Assert.Equal(
            "Trackr.DeliberateUntil may only be used once per run.",
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
        var executionScoreCalls = 0;
        var runScoreCalls = 0;
        var check =
            from executionScore in Checkr.Deliberate(() => ++executionScoreCalls)
            from runScore in Trackr.Deliberate(_ => ++runScoreCalls)
            from expect in Checkr.Expect("always passes", () => true)
            select Case.Closed;

        new CheckrRunner(
            check,
            new CheckrRunnerConfig(CheckrConfig.Default()),
            1,
            AutopsyProbe.Empty).Run();

        Assert.Equal(0, executionScoreCalls);
        Assert.Equal(0, runScoreCalls);
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
            Trackr.DeliberateUntil(2)(state);
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
    public void ExecutionAndRunScoresAreAddedTogether()
    {
        var runScoreCalls = 0;
        var check =
            from executionScore in Checkr.Deliberate(() => 2)
            from runScore in Trackr.Deliberate(caseFile =>
            {
                runScoreCalls++;
                return caseFile.ExecutionCount + 2;
            })
            from until in Trackr.DeliberateUntil(5)
            from expect in Checkr.Expect("always fails", () => false)
            select Case.Closed;

        var caseFile = new CheckrRunner(
            check,
            new CheckrRunnerConfig(CheckrConfig.Default()),
            1,
            AutopsyProbe.Empty).Run();

        Assert.Equal(5, caseFile.DeliberationScore);
        Assert.Equal(5, caseFile.DeliberationTarget);
        Assert.Equal(1, runScoreCalls);
    }

    [Fact]
    public void ExecutionScoresAreAddedPerExecutionAndRunScoresOncePerRun()
    {
        var runScoreCalls = 0;
        var check =
            from executionScore in Checkr.Deliberate(() => 2)
            from runScore in Trackr.Deliberate(caseFile =>
            {
                runScoreCalls++;
                return caseFile.ExecutionCount;
            })
            from until in Trackr.DeliberateUntil(9)
            from verify in Trackr.Verify("always fails", () => false)
            select Case.Closed;
        var config = CheckrConfig.Default() with { ShrinkMode = ShrinkMode.None };

        var caseFile = new CheckrRunner(
            check,
            new CheckrRunnerConfig(config),
            3,
            AutopsyProbe.Empty).Run();

        Assert.Equal(9, caseFile.DeliberationScore);
        Assert.Equal(9, caseFile.DeliberationTarget);
        Assert.Equal(1, runScoreCalls);
    }
}
