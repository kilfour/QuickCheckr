using QuickCheckr.Tests.Tools;
using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.A_ExpectationExceptions.Sub;

[DocFile]
public class B_CausedByDelayedResult : BaseTest
{
    [Fact]
    [DocExample(typeof(B_CausedByDelayedResult), nameof(GetCheckr))]
    [DocBoldHeader("Message:")]
    [DocExample(typeof(B_CausedByDelayedResult), nameof(Message))]
    public void RunTest()
    {
        var exception = Assert.Throws<ExpectationThrowsException>(() => GetCheckr().Run());
        Assert.Equal(Message(), exception.Message);
    }

    [CodeSnippet]
    private static string Message() =>
        "The expectation 'Expect Throws' threw a 'DelayedResultNullValueAccessedException'.";

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from input in Checkr.Input("Input", Fuzzr.Int())
            from act in Checkr.ActCarefully("Act", () => (int?)null)
            from expect in Checkr.Expect("Expect Throws", () => act.Value!.Value == 5)
            select Case.Closed;
    }
}
