using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickCheckr.UnderTheHood;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.A_Guide.A_CheckringThingsOutChapter;

[CodeExample]
public class B_MinimalFailingCheckr : CheckrTest<B_MinimalFailingCheckr>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocContent("So what happens if we try to do the opposite?")]
    [DocCheckr]
    [DocContent("The test fails and QuickCheckr produces the following:")]
    [DocReport]
    [DocContent(
@"
The report shows a run summary containing:
* Test: Name of the test executing the Checkr.
* Location: Source code location of the test executing the Checkr.
* Original failing run: Number of executions executed during the run.
* The minimal failing case: In this simplest of examples it *only* specifies that an execution happened.
* Failure details: Why the run failed in the first place.
* Seed: The seed used to generate the failing run.  
  You can use this to reproduce the failing run reliably, by passing the seed to Run.")]
    [DocExample(typeof(B_MinimalFailingCheckr), nameof(RunSeeded))]
    public override void Example() => Document(RunSeeded);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        Checkr.Expect("Always true", () => false);

    protected override void Verify(Article article)
    {
        Assert.Equal("Always true", article.FailureDescription());
        Assert.Equal("", article.VerifyFailed());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
    }

    [CodeSnippet]
    private static void RunSeeded(ConfiguredCheckr checkr) =>
        checkr.Run(1642083259);
}