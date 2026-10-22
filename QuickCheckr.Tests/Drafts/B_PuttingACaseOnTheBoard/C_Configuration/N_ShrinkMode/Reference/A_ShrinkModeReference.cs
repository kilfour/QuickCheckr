using QuickCheckr.Protocol;
using QuickCheckr.Shrinking;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.C_Configuration.N_ShrinkMode.Reference;

[DocFile]
[DocFileHeader("ShrinkMode Reference")]
[DocContent("The following Checkr will be used to generate the reports throughout this chapter:")]
[DocExample(typeof(A_ShrinkModeReference), nameof(GetCheckr))]
[DocInclude(typeof(ShrinkModeNone))]
[DocInclude(typeof(ShrinkModeExecutions))]
[DocInclude(typeof(ShrinkModeInputs))]
[DocInclude(typeof(ShrinkModeDefault))]
[DocContent("**Hint:** You can use bit-wise operations to manipulate `ShrinkMode`:")]
[DocExample(typeof(A_ShrinkModeReference), nameof(BitWiseOperationExample))]
public class A_ShrinkModeReference : BaseTest
{
    [CodeSnippet]
    [CodeRemove("return")]
    public static CheckrOf<Case> GetCheckr()
    {
        return
            from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
            from relevant in Checkr.Input("Relevant", Fuzzr.Int(), Shrinkr.For<int>().Reduce(Reduce.TowardsZero))
            from irrelevant in Checkr.Input("Irrelevant", Fuzzr.Int(1, 25))
            from expect in Checkr.Expect("Third Time Answer", () => !(counter == 3 && relevant >= 42))
            select Case.Closed;
    }

    [CodeSnippet]
    private static CheckrConfig BitWiseOperationExample(CheckrConfig config) =>
        config with { ShrinkMode = config.ShrinkMode & ~ShrinkMode.Inputs };
}