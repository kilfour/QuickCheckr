using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Docs.H_OnShrinking.C_InputShrinking.A_Primitives;

[DocFile]
public class Enums : BaseTest
{
    [Fact]
    [DocExample(typeof(Enums), nameof(GetCheckr))]
    [DocCodeFile("Enums.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(a => a with { Seed = 1732064051 }));
        WriteAsserts(article);
        WriteReport(article, "Enums.txt");
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("Should not be Monday", article.FailedSpec());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal("Input Two", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("Monday", article.Execution(1).Input(1).Read().Value);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from inputOne in Checkr.Input("Input One", Fuzzr.Enum<DayOfWeek>())
            from inputTwo in Checkr.Input("Input Two", Fuzzr.Enum<DayOfWeek>())
            from spec in Checkr.Spec("Should not be Monday", () => inputTwo != DayOfWeek.Monday)
            select Case.Closed;
    }

    enum SoundCheck { One, Two }

    [Fact]
    [DocExample(typeof(Enums), nameof(GetCheckr_Custom))]
    [DocCodeFile("EnumsCustom.txt", "text")]
    public void RunTest_Custom()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr_Custom().Run(a => a with { Seed = 1205305161 }));
        WriteAsserts(article);
        WriteReport(article, "EnumsCustom.txt");
        Verify_Custom(article);
    }

    private static void Verify_Custom(Article article)
    {
        Assert.Equal("Should not be Two", article.FailedSpec());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal("Input Two", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("Two", article.Execution(1).Input(1).Read().Value);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr_Custom()
    {
        return
            from inputOne in Checkr.Input("Input One", Fuzzr.Enum<SoundCheck>())
            from inputTwo in Checkr.Input("Input Two", Fuzzr.Enum<SoundCheck>())
            from spec in Checkr.Spec("Should not be Two", () => inputTwo != SoundCheck.Two)
            select Case.Closed;
    }
}