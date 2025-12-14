using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Notes.B_BuildingBlocks.H_CheckrActCarefully;

[DocFile]
public class CheckrActCarefully : BaseTest
{
    [Fact]
    [DocExample(typeof(CheckrActCarefully), nameof(GetCheckr))]
    [DocCodeFile("CheckrActCarefully.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(412717959));
        WriteAsserts(article);
        WriteReport<CheckrActCarefully>(article);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("Throws InvalidOperationException", article.FailedExpectation());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().PassedSpecs());
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Act", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Throws", article.PassedExpectation(1).Read().Label);
        Assert.Equal(1, article.PassedExpectation(1).Read().TimesPassed);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from result in Checkr.ActCarefully("Act", () => throw new Exception("BOOM"))
            from throws in Checkr.Expect("Throws", () => result.Threw)
            from throwsExact in Checkr.Expect("Throws InvalidOperationException", result.ThrewAs<InvalidOperationException>)
            select Case.Closed;
    }
}