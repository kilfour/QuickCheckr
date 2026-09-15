using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.A_Guide.A_CheckringThingsOutChapter;

[DocFileHeader("Time to Act!")]
public class E_TimeToAct : CheckrTest<E_TimeToAct>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocContent("Testing inputs is fun and all, but really, we want to test some kind of behaviour, right?")]
    [DocCheckr]
    [DocContent("Still a trivial example, but it shows how you can use `Checkr.Act` to start mutating things.")]
    [DocReport]
    [DocContent("As demonstrated, the name of the executed action is included in the report, as well as the relevant inputs.")]
    public override void Example() => Document(a => a.Run(1642083259));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from one in Checkr.Input("One", Fuzzr.Int())
        from two in Checkr.Input("Two", Fuzzr.Int())
        from three in Checkr.Input("Three", Fuzzr.Int())
        from sum in Checkr.Act("One plus Three", () => one + three)
        from expect in Checkr.Expect("Sum /= 42", () => sum != 42)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Sum /= 42", article.FailureDescription());
        Assert.Equal("", article.VerifyFailed());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(2, article.Total().Inputs());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("One plus Three", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("One", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("24", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Three", article.Execution(1).Input(2).Read().Label);
        Assert.Equal("18", article.Execution(1).Input(2).Read().Value);
    }
}