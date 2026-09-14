using QuickCheckr.Tests.Tools;
using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickPulse.Explains;
using QuickPulse.Instruments;

namespace QuickCheckr.Tests.Drafts.P_WhenThingsGoWrong.D_WhatHappensWhenAnExceptionIsThrown.H_PoolOperations;

[DocFile]
[DocContent("""
Whenever an exception occurs during the execution of the expectation in `Trackr.PoolExpectEach` it is wrapped in an `ExpectationThrowsException`.
""")]
public class M_DuringTrackrPoolExpectEach : CheckrThrowsTest<M_DuringTrackrPoolExpectEach>
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
        from operations in Trackr.PoolExpectEach<int>("Expectation", _ => ComputerSays.No<bool>("Boom"))
        select Case.Closed;

    [CodeExceptionType]
    protected override Type ExceptionType() => typeof(ExpectationThrowsException);

    [CodeExceptionMessage]
    protected override string Message() => "The expectation 'Expectation' threw a 'ComputerSaysNo'.";
}
