using QuickCheckr;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickCheckr.UnderTheHood;
using QuickPulse.Explains;
using QuickPulse.Instruments;


namespace QuickCheckr.Tests.Drafts.P_WhenThingsGoWrong.D_WhatHappensWhenAnExceptionIsThrown.H_PoolOperations;

[DocFile]
[DocContent("""
Whenever an excecption occurs during the execution of the condition in `Trackr.PoolWhen` it is propagated.
""")]
public class B_DuringTrackrPoolWhen : CheckrThrowsTest<B_DuringTrackrPoolWhen>
{
    protected override bool Explain => true;

    [CodeSnippet]
    protected override Type ExceptionType => typeof(ComputerSaysNo);

    [CodeSnippet]
    protected override string Message => "Boom";

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocExceptionType]
    [DocMessage]
    public override void Example() => Document(a => a.Run());

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from store in Trackr.ToPool("answer", () => 42)
        from operations in Checkr.OneOfWhen(
            Trackr.PoolWhen<int>(
                _ => ComputerSays.No<bool>("Boom"),
                CheckrResult.CaseOnly))
        select Case.Closed;

    protected override void Verify(string message)
    {
        Assert.Equal(Message, message);
    }
}