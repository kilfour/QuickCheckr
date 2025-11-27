using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.C_Objects.TheExamples;

[DocFile]
public class B_NestedObjects : BaseTest
{
    [Fact]
    [DocExample(typeof(B_NestedObjects), nameof(GetCheckr))]
    [DocCodeFile("B_NestedObjects.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(a => a with { Seed = 803555194 }));
        WriteAsserts(article);
        WriteReport<B_NestedObjects>(article);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("It's not 42", article.FailedSpec());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Universe", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ Everything : { Answer : 42 } }", article.Execution(1).Input(1).Read().Value);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from input in Checkr.Input("Universe",
                from _ in Configr.Primitive(Fuzzr.Constant("Answer me"))
                from __ in Configr.Primitive(Fuzzr.Int())
                from thing in Fuzzr.One<TheUniverse>()
                select thing)
            from spec in Checkr.SpecIf("It's not 42",
                () => input.Everything != null,
                () => input.Everything!.Answer != 42)
            select Case.Closed;
    }

    public class TheUniverse
    {
        public Life? Everything { get; set; }
    }

    public class Life
    {
        public int Answer { get; set; }
        public string WrongAnswer { get; set; } = string.Empty;
    }
}