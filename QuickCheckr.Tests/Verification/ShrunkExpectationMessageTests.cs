using QuickCheckr.Diagnostics;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood;
using QuickFuzzr;

namespace QuickCheckr.Tests.Verification;

public class ShrunkExpectationMessageTests
{
    [Theory]
    [InlineData("check", ReportMode.None)]
    [InlineData("check", ReportMode.Default)]
    [InlineData("check", ReportMode.FinalTrace)]
    [InlineData("pool", ReportMode.None)]
    [InlineData("each", ReportMode.None)]
    [InlineData("when", ReportMode.None)]
    [InlineData("related", ReportMode.None)]
    public async Task Reports_final_shrunk_value(string kind, ReportMode reportMode)
    {
        CheckrOf<Case> Expect(int value) => kind switch
        {
            "check" => Checkr.Expect("failure", context => value > 0 ? context.Equal(0, value) : context.Pass()),
            "pool" => Trackr.PoolExpect<int>("failure", async (pool, context) =>
            {
                await Task.Yield();
                return pool.Single() > 0 ? context.Equal(0, pool.Single()) : context.Pass();
            }),
            "each" => Trackr.PoolExpectEach<int>("failure", (item, context) => item > 0 ? context.Equal(0, item) : context.Pass()),
            "when" => Trackr.PoolExpectEachWhen<int>("failure", item => item > 0, async (item, context, token) =>
            {
                await Task.Yield();
                return context.Equal(0, item);
            }),
            _ => Trackr.PoolExpectEach<int, string>("failure", (_, _) => true,
                (item, children, context) => item > 0 ? context.Equal(0, item) : context.Pass())
        };
        var check =
            from input in Checkr.Input("value", Fuzzr.Constant(42))
            from pooled in Trackr.ToPool("number", () => input)
            from failure in Expect(input)
            select Case.Closed;
        var runner = new CheckrRunner(check,
            new CheckrRunnerConfig(CheckrConfig.Default() with { ReportMode = reportMode, ShrinkMode = ShrinkMode.All }), 1, AutopsyProbe.Empty);
        var file = await runner.RunAsync();
        Assert.True(file.HasEvidence);
        var messages = runner.State.EvidenceRecorder.FailingExpectation.Message;
        Assert.Contains("Actual: 1", messages);
        Assert.DoesNotContain("Actual: 42", messages);
    }

    [Fact]
    public void Uncommitted_replays_do_not_replace_reported_messages()
    {
        var recorder = new EvidenceRecorder(new CheckrState(1, 100, 123, AutopsyProbe.Empty).Pathologist);
        recorder.ExpectationFailed(new ExpectationFailure("failure", ["original"]), 1);
        recorder.StartReplay();
        recorder.ExpectationFailed(new ExpectationFailure("failure", ["candidate"]), 1);
        Assert.Equal(new[] { "original" }, recorder.FailingExpectation.Message);
        recorder.StartReplay();
        recorder.RefreshFailureMessages();
        Assert.Equal(new[] { "original" }, recorder.FailingExpectation.Message);
        recorder.ExpectationFailed(new ExpectationFailure("failure", ["final"]), 1);
        recorder.RefreshFailureMessages();
        Assert.Equal(new[] { "final" }, recorder.FailingExpectation.Message);
    }
}
