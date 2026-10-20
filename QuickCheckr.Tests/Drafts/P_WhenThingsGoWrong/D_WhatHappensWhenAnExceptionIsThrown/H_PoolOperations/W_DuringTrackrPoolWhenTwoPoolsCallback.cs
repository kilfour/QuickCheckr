using QuickCheckr.Tests.Tools;
using QuickPulse.Explains;
using QuickPulse.Instruments;

namespace QuickCheckr.Tests.Drafts.P_WhenThingsGoWrong.D_WhatHappensWhenAnExceptionIsThrown.H_PoolOperations;

[DocFile]
[DocContent("""
Whenever an exception occurs during the execution of the callback in `Trackr.PoolWhen<T1, T2>` it is propagated.
""")]
public class W_DuringTrackrPoolWhenTwoPoolsCallback : CheckrThrowsTest<W_DuringTrackrPoolWhenTwoPoolsCallback>
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
        from other in Poolr.Add("text", Checkr.Capture(() => "answer"))
        from operations in Checkr.OneOfWhen(Trackr.PoolWhen<int, string>("Select", (_, _) => true, (_, _) => ComputerSays.No<CheckrOf<Case>>("Boom")))
        select Case.Closed;

    [CodeExceptionType]
    protected override Type ExceptionType() => typeof(ComputerSaysNo);

    [CodeExceptionMessage]
    protected override string Message() => "Boom";
}
