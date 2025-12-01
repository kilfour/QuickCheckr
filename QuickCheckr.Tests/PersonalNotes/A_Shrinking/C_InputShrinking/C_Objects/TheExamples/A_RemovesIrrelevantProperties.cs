using QuickCheckr.Tests.PersonalNotes.A_Shrinking.C_InputShrinking.C_Objects.TheExamples.Models;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.PersonalNotes.A_Shrinking.C_InputShrinking.C_Objects.TheExamples;

[DocFile]
public class A_RemovesIrrelevantProperties : BaseTest
{
    [Fact]
    [DocExample(typeof(BaggyInters))]
    [DocExample(typeof(A_RemovesIrrelevantProperties), nameof(GetCheckr))]
    [DocCodeFile("A_RemovesIrrelevantProperties.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(a => a with { Seed = 488581569 }));
        WriteAsserts(article);
        WriteReport<A_RemovesIrrelevantProperties>(article);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("IntOne /= 42", article.FailedSpec());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Object", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ IntOne: 42 }", article.Execution(1).Input(1).Read().Value);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from obj in Checkr.Input("Object", Fuzzr.One<BaggyInters>())
            from spec in Checkr.Spec("IntOne /= 42", () => obj.IntOne != 42)
            select Case.Closed;
    }


}
