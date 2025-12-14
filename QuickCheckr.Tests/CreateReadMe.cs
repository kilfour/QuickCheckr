using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests;

[DocFile]
[DocFileHeader("<img src='icon.png' width='40' align='top'/> QuickCheckr")]
[DocRawFile("CreateReadme-header.md")]
public class CreateReadMe : BaseTest
{
    [Fact]
    [DocHeader("Example")]
    [DocContent("**Buggy Code:**")]
    [DocExample(typeof(BugHouse))]
    [DocContent("**The Checkr:**")]
    [DocExample(typeof(CreateReadMe), nameof(GetCheckr))]
    [DocContent("**Reports:**")]
    [DocCodeFile("CreateReadMe.txt", "text")]
    [DocRawFile("CreateReadme-footer.md")]
    public void Example()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(1141724745, 100.ExecutionsPerRun()));
        WriteAsserts(article);
        WriteReport<CreateReadMe>(article);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal(2, article.Total().Executions());
        Assert.Equal(2, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().PassedSpecs());
        Assert.Equal(4, article.Execution(1).Read().ExecutionId);
        Assert.Equal(3, article.Execution(1).Read().Times);
        Assert.Equal("BugHouse.Run", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("number", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("6", article.Execution(1).Input(1).Read().Value);
        Assert.Equal(84, article.Execution(2).Read().ExecutionId);
        Assert.Equal(2, article.Execution(2).Read().Times);
        Assert.Equal("BugHouse.Run", article.Execution(2).Action(1).Read().Label);
        Assert.Equal("Returns true", article.PassedExpectation(1).Read().Label);
        Assert.Equal(84, article.PassedExpectation(1).Read().TimesPassed);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
        from bughouse in Trackr.Stashed(() => new BugHouse())
        from number in Checkr.Input("number", Fuzzr.Int())
        from output in Checkr.Act("BugHouse.Run", () => bughouse.Run(number))
        from spec in Checkr.Expect("Returns true", () => output)
        select Case.Closed;
    }

    [CodeExample]
    public class BugHouse
    {
        private int count;

        public bool Run(int number)
        {
            if (count == 5) throw new Exception("Here's Johnny");
            if (number == 6 && count != 3) count++;
            if (count >= 3) count++;
            return true;
        }
    }
}