using QuickCheckr.Tests.Docs.A_Guide.B_TheStashRunsAndExecutionsChapter;
using QuickCheckr.Tests.Tools;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.A_Guide.A_CheckringThingsOutChapter;

[DocFile]
[DocFileHeader("Checkr-ing Things Out")]
[DocNavigation(typeof(Guide), null, typeof(A_TheStashRunsAndExecutions))]
[DocContent(
@"A *Checkr* describes a sequence of steps that QuickCheckr will execute repeatedly.
Each run goes through the script from top to bottom: generating inputs, performing actions, checking expectations, and collecting diagnostics.
If any execution fails, QuickCheckr shrinks it to the smallest, easiest-to-understand failing case.")]
public class A_CheckringThingsOut
{
    [Fact]
    [DocHeader("First Expectation")]
    [DocContent("Let's start by having a look at the simplest possible Checkr:")]
    [DocExample(typeof(A_CheckringThingsOut), nameof(RunTest))]
    [DocContent(
@"Congratulations! You've just checked something out. The test passes and now we know that `true` is always `true`.
")]
    [DocInclude(typeof(B_MinimalFailingCheckr), true)]
    [DocInclude(typeof(C_GatheringInput))]
    [DocInclude(typeof(D_AddingAnotherInput), true)]
    [DocInclude(typeof(E_TimeToAct))]
    [CodeExample]
    public void RunTest()
        => Checkr.Expect("Always true", () => true).Run();
}
