using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Authoring.ThePress;
using QuickPulse.Explains;
using QuickTestr.Tests.Tools;

namespace QuickTestr.Tests.Notes.T_Exploration.Sub;

[DocFile]
[DocContent("Let's remove the empty string option from `Evilr`")]
[DocTestr]
[DocContent("This results in:")]
[DocReport]
public class B_SlightlyLessEvil : QuickTestrPropertyTest<B_SlightlyLessEvil>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    public override void Example() => Document();

    [CodeSnippet]
    [CodeRemoveJournalist]
    [CodeRemove("1540338462")]
    protected override void GetTestr(Journalist journalist) =>
        Testr
            .Named("BookTitle Creation")
            .For(Evilr.StringExcept(Evilr.StringOption.Empty))
            .Assert(a => BookTitle.Create(a!).Value == a)
            .StoreCaseFiles(journalist)
            .Run(1540338462);

    protected override void Verify(Article article)
    {
        Assert.Equal("ComputerSaysNo: Title is required.", article.FailureDescription());
        Assert.Equal("", article.VerifyFailed());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(2, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Rethrow", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Run", article.Execution(1).Action(2).Read().Label);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("\"\\u00A0\"", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("\"\\u00A0\"", article.Execution(1).Input(1).Read().Original.Value);
        Assert.Equal("\"\"", article.Execution(1).Input(1).Read().Redux.Value);
    }
}
