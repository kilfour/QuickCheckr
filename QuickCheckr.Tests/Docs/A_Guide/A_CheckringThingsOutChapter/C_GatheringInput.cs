using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickCheckr.UnderTheHood;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.A_Guide.A_CheckringThingsOutChapter;

public class C_GatheringInput : CheckrTest<C_GatheringInput>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocContent("Let's add an input to the Checkr, and change our expectation.")]
    [DocCheckr]
    [DocContent(
@"
*Note:* `Case.Closed` signals the end of the Checkr script.
A Checkr is evaluated step by step, and `Case.Closed` is the final marker to indicate the scenario is complete.

QuickCheckr relies on the [`QuickFuzzr`](https://github.com/kilfour/QuickFuzzr/blob/main/README.md) library to generate inputs.

`Fuzzr.Int()` generates a random integer between 1 and 100, so this test will fail, *on average*, once every hundred times.  
Not ideal, so let's up the run number, like so:")]
    [DocExample(typeof(C_GatheringInput), nameof(RunTwoHundredTimes))]
    [DocContent("With more runs, the test is more likely to find a failing input.")]
    [DocReport]
    [DocContent(
@"As you can see the input is now included in the report *minimal fail-case*,
this is because QuickCheckr judged the input to be relevant to the failure.")]
    public override void Example() => Document(a => a.Run(127648209));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Input", Fuzzr.Int())
        from expect in Checkr.Expect("Input /= 42", () => input != 42)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Input /= 42", article.FailureDescription());
        Assert.Equal("", article.VerifyFailed());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("42", article.Execution(1).Input(1).Read().Value);
    }

    [CodeSnippet]
    private static void RunTwoHundredTimes(ConfiguredCheckr checkr) =>
        checkr.Run(200.Runs());
}