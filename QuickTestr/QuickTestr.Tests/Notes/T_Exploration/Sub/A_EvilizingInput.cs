using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Authoring.ThePress;
using QuickPulse.Explains;
using QuickTestr.Tests.Tools;

namespace QuickTestr.Tests.Notes.T_Exploration.Sub;

[DocFile]
[DocContent("Let's look a little bit closer though, by *evilizing* the input:")]
[DocTestr]
[DocContent("This results in:")]
[DocReport]
public class A_EvilizingInput : QuickTestrPropertyTest<A_EvilizingInput>
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
            .For(Evilr.String())
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
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Rethrow", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Run", article.Execution(1).Action(2).Read().Label);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("\"\"", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("\"\"", article.Execution(1).Input(1).Read().Original.Value);
    }
}
