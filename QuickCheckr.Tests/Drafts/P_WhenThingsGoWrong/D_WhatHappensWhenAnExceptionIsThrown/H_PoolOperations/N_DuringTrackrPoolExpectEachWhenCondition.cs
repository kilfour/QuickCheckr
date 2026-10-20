using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickCheckr.Tests.Tools;
using QuickPulse.Explains;
using QuickPulse.Instruments;

namespace QuickCheckr.Tests.Drafts.P_WhenThingsGoWrong.D_WhatHappensWhenAnExceptionIsThrown.H_PoolOperations;

[DocFile]
[DocContent("""
Whenever an exception occurs during the execution of the condition in `Trackr.PoolExpectEachWhen` it is wrapped in an `ExpectationThrowsException`.
""")]
public class N_DuringTrackrPoolExpectEachWhenCondition : CheckrThrowsTest<N_DuringTrackrPoolExpectEachWhenCondition>
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
        from operations in Trackr.PoolExpectEachWhen<int>("Expectation", _ => ComputerSays.No<bool>("Boom"), _ => true)
        select Case.Closed;

    [CodeExceptionType]
    protected override Type ExceptionType() => typeof(ExpectationThrowsException);

    [CodeExceptionMessage]
    protected override string Message() => "The expectation 'Expectation' threw a 'ComputerSaysNo'.";
}
