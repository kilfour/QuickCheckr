using QuickCheckr.Tests.Tools;
using QuickPulse.Explains;
using QuickPulse.Instruments;

namespace QuickCheckr.Tests.Drafts.P_WhenThingsGoWrong.D_WhatHappensWhenAnExceptionIsThrown.H_PoolOperations;

[DocFile]
[DocContent("""
Whenever an exception occurs during the execution of the condition in `Poolr.OneOfWhen<T1, T2>` it is propagated.
""")]
public class G_DuringPoolrOneOfPairWhenCondition : CheckrThrowsTest<G_DuringPoolrOneOfPairWhenCondition>
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
        from operations in Poolr.OneOfWhen<int, string>("Select", (_, _) => ComputerSays.No<bool>("Boom"))
        select Case.Closed;

    [CodeExceptionType]
    protected override Type ExceptionType() => typeof(ComputerSaysNo);

    [CodeExceptionMessage]
    protected override string Message() => "Boom";
}
