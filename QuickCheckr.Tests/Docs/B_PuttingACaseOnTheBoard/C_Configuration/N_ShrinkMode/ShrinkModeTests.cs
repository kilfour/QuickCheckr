using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.C_Configuration.N_ShrinkMode;

//[DocFile]
[DocFileHeader("ShrinkMode")]
[DocContent(
@"Controls which shrinking strategies are active.  

This is an option you will likely never need.  
It's main use is to investigate what happened during shrinking.  
As such, disabling shrinking entirely is probably the most useful step.")]
[DocExample(typeof(ShrinkModeTests), nameof(DisableShrinking))]
[DocContent("A full reference to the available options can be found [here][A_ShrinkModeReference].")]
public class ShrinkModeTests : BaseTest
{
    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
            from irrelevant in Checkr.Input("Irrelevant", Fuzzr.Int(1, 25))
            from expect in Checkr.Expect("Counter does not equal 3", () => counter != 3)
            select Case.Closed;
    }

    [CodeExample]
    private static CheckrConfig DisableShrinking(CheckrConfig config) =>
        config with { ShrinkMode = ShrinkMode.None };
}