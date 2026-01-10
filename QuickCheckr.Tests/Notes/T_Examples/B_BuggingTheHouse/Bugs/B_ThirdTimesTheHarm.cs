using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Notes.T_Examples.B_BuggingTheHouse.Bugs;

[DocFile]
[DocFileHeader("Third Times the Harm")]
[DocExample(typeof(BugHouse))]
public class B_ThirdTimesTheHarm : BaseTest
{
    [Fact]
    [DocContent(
@"Throws as soon as the value 6 has been seen three times.  
A straightforward counter-based failure condition.")]
    [DocExample(typeof(B_ThirdTimesTheHarm), nameof(GetCheckr))]
    [DocHeader("Minimal Fail Case:")]
    [DocContent(
@"- **Length:** 3 executions.
- **Example input:** `[6, 6, 6]`
```csharp
count = 0
Run( a = 6 ) // count = 1
Run( a = 6 ) // count = 2
Run( a = 6 ) // count == 3 → fail
```")]
    [DocHeader("Report:")]
    [DocCodeFile("B_ThirdTimesTheHarm.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(766917706, 50.ExecutionsPerRun()));
        WriteAsserts(article);
        WriteReport<B_ThirdTimesTheHarm>(article);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("Returns true", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(9, article.Execution(1).Read().ExecutionId);
        Assert.Equal(3, article.Execution(1).Times);
        Assert.Equal("BugHouse.Run", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("a", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("6", article.Execution(1).Input(1).Read().Value);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from a in Checkr.Input("a", Fuzzr.Int())
            from bughouse in Trackr.Stashed(() => new BugHouse())
            from output in Checkr.Act("BugHouse.Run", () => bughouse.Run(a))
            from expect in Checkr.Expect("Returns true", () => output)
            select Case.Closed;
    }

    [CodeExample]
    public class BugHouse
    {
        private int count;
        public bool Run(int a)
        {
            if (a == 6) count++;
            return !(count == 3);
        }
    }
}