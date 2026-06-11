using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Notes.Z_ToBeLookedAt.Z_Issues.A_TheUnshrinkables.B_UnknownPrimitives;

[DocFile]
public class UnknownPrimitives : BaseTest
{
    [Fact]
    [DocExample(typeof(UnknownPrimitives), nameof(GetCheckr))]
    [DocContent("Run with `WarningLevel.Verbose`.")]
    [DocCodeFile("UnknownPrimitives.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Configure(a => a with { WarningLevel = WarningLevel.Verbose }).Run(746257211, 1.ExecutionsPerRun()));
        WriteAsserts(article, false);
        WriteReport<UnknownPrimitives>(article, false);
        Verify(article);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from unkown in Checkr.Input("Half", Fuzzr.Half())
            from fail in Checkr.Expect("Fails", () => false)
            select Case.Closed;
    }

    private static void Verify(Article article)
    {
        Assert.Equal("Fails", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Warnings());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Half", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("65.25", article.Execution(1).Input(1).Read().Value.ToString()!.Replace(",", "."));
        Assert.Equal("Can't shrink type 'Half' at input 'Half'.", article.Execution(1).Warning(1).Read().Value);
    }
}