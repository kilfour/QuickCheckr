using QuickCheckr;
using QuickCheckr.Tests.Docs.A_Guide.E_HorsesAgain;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickFuzzr;
using QuickPulse.Arteries;
using QuickPulse.Explains;
using QuickPulse.Explains.Text;


namespace QuickCheckr.Tests.Drafts.C_Configuration.H_Investigations;

[DocFile]
[DocContent("These tests show the initial version of QuickCheckrs logging configuration.")]
public class Investigations : BaseTest
{
    [Fact]
    public void CheckEventProcedure()
    {
        var collector = Collect.ValuesOf<string>();
        var article = TheJournalist.Exposes(() =>
            BoltingHorses.TheCheckr.Run(20.ExecutionsPerRun(),
                a => a with
                {
                    Seed = 1416432348,
                    Investigations = [new(() => collector, Procedure.Events)]
                }));
        var reader = LinesReader.FromStringList([.. collector.Values]);
        Assert.Equal("ExecutionRemoved", reader.NextLine());
        Assert.Equal("ExecutionRemoved", reader.NextLine());
        Assert.Equal("ExecutionRemoved", reader.NextLine());
        Assert.Equal("ExecutionRemoved", reader.NextLine());
        Assert.Equal("ExecutionRemoved", reader.NextLine());
        Assert.Equal("ExecutionRemoved", reader.NextLine());
        Assert.Equal("ExecutionRemoved", reader.NextLine());
        Assert.Equal("ExecutionRemoved", reader.NextLine());
        Assert.Equal("ExecutionRemoved", reader.NextLine());
        Assert.Equal("ExecutionRemoved", reader.NextLine());
        Assert.Equal("InputShrinkingClueFound", reader.NextLine());
        Assert.Equal("InputShrinkingClueFound", reader.NextLine());
        Assert.Equal("InputShrinkingClueFound", reader.NextLine());
        Assert.Equal("InputShrinkingClueFound", reader.NextLine());
        Assert.Equal("InputShrinkingClueFound", reader.NextLine());
        Assert.Equal("InputShrinkingClueFound", reader.NextLine());
        Assert.Equal("EvidenceFailure", reader.NextLine());
        Assert.Equal("InputShrinkingClueFound", reader.NextLine());
        Assert.Equal("InputShrinkingClueFound", reader.NextLine());
        Assert.Equal("InputShrinkingClueFound", reader.NextLine());
        Assert.True(reader.EndOfContent());
    }

    [Fact]
    public void CheckInputShrinkProcedure()
    {
        var check =
            from list in Checkr.Input("List", Fuzzr.Int().Many(1, 10))
            from trace in Checkr.Trace("Original List", () => list)
            from spec in Checkr.Spec("Length <= 2", () => list.Count() <= 2)
            select Case.Closed;
        var collector = Collect.ValuesOf<string>();
        var article = TheJournalist.Exposes(() => check.Run(
            a => a with
            {
                Seed = 1393576429,
                Investigations = [new(() => collector, Procedure.InputShrinking)]
            }));
        var reader = LinesReader.FromStringList([.. collector.Values.SelectMany(a => a.Split(Environment.NewLine))]);
        Assert.Equal("{", reader.NextLine());
        Assert.Equal("    ExecutionId: 1,", reader.NextLine());
        Assert.Equal("    Key: \"List.0\",", reader.NextLine());
        Assert.Equal("    Original: [ 12, 45, 56, 18, 31 ],", reader.NextLine());
        Assert.Equal("    Result: [ 45, 56, 18, 31 ],", reader.NextLine());
        Assert.Equal("    Report: null,", reader.NextLine());
        Assert.Equal("    Intent: Remove,", reader.NextLine());
        Assert.Equal("    Strategy: \"RemoveOneByOneStrategy\",", reader.NextLine());
        Assert.Equal("    Count: 1", reader.NextLine());
        Assert.Equal("}", reader.NextLine());
        Assert.Equal("-------------------------------------", reader.NextLine());
        Assert.Equal("{", reader.NextLine());
        Assert.Equal("    ExecutionId: 1,", reader.NextLine());
        Assert.Equal("    Key: \"List.1\",", reader.NextLine());
        Assert.Equal("    Original: [ 12, 45, 56, 18, 31 ],", reader.NextLine());
        Assert.Equal("    Result: [ 56, 18, 31 ],", reader.NextLine());
        Assert.Equal("    Report: null,", reader.NextLine());
        Assert.Equal("    Intent: Remove,", reader.NextLine());
        Assert.Equal("    Strategy: \"RemoveOneByOneStrategy\",", reader.NextLine());
        Assert.Equal("    Count: 1", reader.NextLine());
        Assert.Equal("}", reader.NextLine());
        Assert.Equal("-------------------------------------", reader.NextLine());
        Assert.Equal("{", reader.NextLine());
        Assert.Equal("    ExecutionId: 1,", reader.NextLine());
        Assert.Equal("    Key: \"List\",", reader.NextLine());
        Assert.Equal("    Original: [ 12, 45, 56, 18, 31 ],", reader.NextLine());
        Assert.Equal("    Result: [ 56, 18, 31 ],", reader.NextLine());
        Assert.Equal("    Report: \"[ 56, 18, 31 ]\",", reader.NextLine());
        Assert.Equal("    Intent: Keep,", reader.NextLine());
        Assert.Equal("    Strategy: \"RemoveOneByOneStrategy\",", reader.NextLine());
        Assert.Equal("    Count: 2", reader.NextLine());
        Assert.Equal("}", reader.NextLine());
        Assert.Equal("-------------------------------------", reader.NextLine());
        Assert.Equal("{", reader.NextLine());
        Assert.Equal("    ExecutionId: 1,", reader.NextLine());
        Assert.Equal("    Key: \"List.0\",", reader.NextLine());
        Assert.Equal("    Original: 56,", reader.NextLine());
        Assert.Equal("    Result: null,", reader.NextLine());
        Assert.Equal("    Report: \"\",", reader.NextLine());
        Assert.Equal("    Intent: Ignore,", reader.NextLine());
        Assert.Equal("    Strategy: \"PrimitiveShrinkStrategy\",", reader.NextLine());
        Assert.Equal("    Count: 1", reader.NextLine());
        Assert.Equal("}", reader.NextLine());
        Assert.Equal("-------------------------------------", reader.NextLine());
        Assert.Equal("{", reader.NextLine());
        Assert.Equal("    ExecutionId: 1,", reader.NextLine());
        Assert.Equal("    Key: \"List.1\",", reader.NextLine());
        Assert.Equal("    Original: 18,", reader.NextLine());
        Assert.Equal("    Result: null,", reader.NextLine());
        Assert.Equal("    Report: \"\",", reader.NextLine());
        Assert.Equal("    Intent: Ignore,", reader.NextLine());
        Assert.Equal("    Strategy: \"PrimitiveShrinkStrategy\",", reader.NextLine());
        Assert.Equal("    Count: 1", reader.NextLine());
        Assert.Equal("}", reader.NextLine());
        Assert.Equal("-------------------------------------", reader.NextLine());
        Assert.Equal("{", reader.NextLine());
        Assert.Equal("    ExecutionId: 1,", reader.NextLine());
        Assert.Equal("    Key: \"List.2\",", reader.NextLine());
        Assert.Equal("    Original: 31,", reader.NextLine());
        Assert.Equal("    Result: null,", reader.NextLine());
        Assert.Equal("    Report: \"\",", reader.NextLine());
        Assert.Equal("    Intent: Ignore,", reader.NextLine());
        Assert.Equal("    Strategy: \"PrimitiveShrinkStrategy\",", reader.NextLine());
        Assert.Equal("    Count: 1", reader.NextLine());
        Assert.Equal("}", reader.NextLine());
        Assert.Equal("-------------------------------------", reader.NextLine());
        Assert.Equal("{", reader.NextLine());
        Assert.Equal("    ExecutionId: 1,", reader.NextLine());
        Assert.Equal("    Key: \"List\",", reader.NextLine());
        Assert.Equal("    Original: [ 56, 18, 31 ],", reader.NextLine());
        Assert.Equal("    Result: [ 56, 18, 31 ],", reader.NextLine());
        Assert.Equal("    Report: \"[ _, _, _ ]\",", reader.NextLine());
        Assert.Equal("    Intent: Replace,", reader.NextLine());
        Assert.Equal("    Strategy: \"ShrinkEachElementStrategy\",", reader.NextLine());
        Assert.Equal("    Count: 3", reader.NextLine());
        Assert.Equal("}", reader.NextLine());
        Assert.Equal("-------------------------------------", reader.NextLine());
        Assert.True(reader.EndOfContent());
    }
}