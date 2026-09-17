using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickCheckr.Tests.Tools;
using QuickPulse.Explains;
using QuickPulse.Instruments;

namespace QuickCheckr.Tests.Drafts.P_WhenThingsGoWrong.D_WhatHappensWhenAnExceptionIsThrown.H_PoolOperations;

[DocFile]
[DocContent("""
Whenever an exception occurs during the execution of the belongs-to condition in `Trackr.PoolExpectEach<T1, T2>` it is wrapped in an `ExpectationThrowsException`.
""")]
public class P_DuringTrackrPoolExpectEachBelongsTo : CheckrThrowsTest<P_DuringTrackrPoolExpectEachBelongsTo>
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
        from other in Trackr.ToPool("text", () => "answer")
        from operations in Trackr.PoolExpectEach<int, string>("Expectation", (_, _) => ComputerSays.No<bool>("Boom"), (_, _) => true)
        select Case.Closed;

    [CodeExceptionType]
    protected override Type ExceptionType() => typeof(ExpectationThrowsException);

    [CodeExceptionMessage]
    protected override string Message() => "The expectation 'Expectation: Int32-1' threw a 'ComputerSaysNo'.";
}
