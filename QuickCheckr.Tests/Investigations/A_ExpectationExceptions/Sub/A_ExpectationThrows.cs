using QuickCheckr.Tests.Tools;
using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Investigations.A_ExpectationExceptions.Sub;

[DocFile]
[DocContent(
@"In case an expectation throws an exception during the initial Checkr run, 
It is wrapped in an ExpectationThrowsException through InnerException and the expectation key is reported.")]
public class A_ExpectationThrows : BaseTest
{
    [Fact]
    [DocCheckrHeader]
    [DocExample(typeof(A_ExpectationThrows), nameof(GetCheckr))]
    [DocBoldHeader("Message:")]
    [DocExample(typeof(A_ExpectationThrows), nameof(message))]
    public void RunTest()
    {
        var exception = Assert.Throws<ExpectationThrowsException>(() => GetCheckr().Run());
        Assert.Equal(message, exception.Message);
    }

    [CodeSnippet]
    private static readonly string message =
        "The expectation 'Expect Throws' threw a 'InvalidOperationException'.";

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from input in Checkr.Input("Input", Fuzzr.Int())
            from act in Checkr.Act("Act", () => (int?)null)
            from expect in Checkr.Expect("Expect Throws", () => act.Value == 5)
            select Case.Closed;
    }
}