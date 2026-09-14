using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Authoring.ThePress;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickTestr.Tests.Tools;

namespace QuickTestr.Tests.Notes.T_Exploration;

[DocFile]
[DocFileHeader("Exploring a Method")]
[DocContent("Consider the following simple *Value Object*")]
[DocExample(typeof(BookTitle))]
[DocContent("`QuickFuzzr` by default generates safe-ish values, so the following `Testr` will pass:")]
[DocTestr]
public class A_ExploringAMethod : QuickTestrPropertyTest<A_ExploringAMethod>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    public override void Example() => Document();

    [CodeSnippet]
    [CodeRemoveJournalist]
    protected override void GetTestr(Journalist journalist) =>
        Testr
            .Named("BookTitle Creation")
            .For(Fuzzr.String())
            .Assert(a => BookTitle.Create(a).Value == a)
            .StoreCaseFiles(journalist)
            .Run();

    protected override void Verify(Article article)
    {
        Assert.Equal("", article.FailureDescription());
        Assert.Equal("", article.VerifyFailed());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal("BookTitle Creation", article.PassedExpectation(1).Read().Label);
        Assert.Equal(100, article.PassedExpectation(1).Read().TimesPassed);
    }
}
