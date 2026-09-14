using QuickCheckr.Tests.Tools;
using QuickPulse.Explains;
using QuickPulse.Instruments;

namespace QuickCheckr.Tests.Drafts.P_WhenThingsGoWrong.D_WhatHappensWhenAnExceptionIsThrown.H_PoolOperations;

[DocFile]
[DocContent("""
Whenever an exception occurs during the execution of the callback in `Trackr.OneOfPoolWhen` it is propagated.
""")]
public class F_DuringTrackrOneOfPoolWhenCallback : CheckrThrowsTest<F_DuringTrackrOneOfPoolWhenCallback>
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
        from store in Trackr.ToPool("answer", () => 42)
        from operations in Trackr.OneOfPoolWhen<int>("Select", _ => true, _ => ComputerSays.No<CheckrOf<Case>>("Boom"))
        select Case.Closed;

    [CodeExceptionType]
    protected override Type ExceptionType() => typeof(ComputerSaysNo);

    [CodeExceptionMessage]
    protected override string Message() => "Boom";
}
