using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.A_Guide.B_TheStashRunsAndExecutionsChapter;


public class B_DelayedDetonationNotFound : CheckrTest<B_DelayedDetonationNotFound>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocContent("If we want to test it and write a Checkr like so:")]
    [DocCheckr]
    [DocContent(
@"The test will always pass, even if we increase the amount of runs to infinity,
because the test loses the state of the `BugHouse` between every run.")]
    public override void Example() => Document(a => a.Run());

    [CodeSnippet]
    [CodeRemove("A_TheStashRunsAndExecutions.")]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Int", Fuzzr.Int())
        from callBugHouse in Checkr.Act("BugHouse.DelayedDetonation", () => new A_TheStashRunsAndExecutions.BugHouse().DelayedDetonation(input))
        from checkResult in Checkr.Expect("Returns true", () => callBugHouse)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("", article.FailureDescription());
        Assert.Equal("", article.VerifyFailed());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal("Returns true", article.PassedExpectation(1).Read().Label);
        Assert.Equal(1, article.PassedExpectation(1).Read().TimesPassed);
    }
}