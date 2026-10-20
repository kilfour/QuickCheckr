using QuickCheckr.Tests.Tools;
using QuickCheckr.UnderTheHood;
using QuickPulse.Explains;
using QuickPulse.Instruments;


namespace QuickCheckr.Tests.Drafts.P_WhenThingsGoWrong.D_WhatHappensWhenAnExceptionIsThrown.H_PoolOperations;

[DocFile]
[DocContent($"""
Whenever an excecption occurs during the execution of the condition in `{nameof(Poolr)}.{nameof(Poolr.When)}` it is propagated.
""")]
public class B_DuringTrackrPoolWhen : CheckrThrowsTest<B_DuringTrackrPoolWhen>
{
    [Fact]
    [DocCheckr]
    [DocException]
    public override void Example() => Document(a => a.Run());

    protected override void Verify(string expected, string actual)
    {
        Assert.Equal(expected, actual);
    }

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from store in Poolr.Add("answer", Checkr.Capture(() => 42))
        from operations in Checkr.OneOfWhen(
            Poolr.When<int>(_ => ComputerSays.No<bool>("Boom"), state => CheckrResult.CaseOnly(state)))
        select Case.Closed;

    [CodeExceptionType]
    protected override Type ExceptionType() => typeof(ComputerSaysNo);

    [CodeExceptionMessage]
    protected override string Message() => "Boom";
}