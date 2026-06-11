using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Notes.T_Examples.B_BuggingTheHouse.Bugs;

[DocFile]
[DocExample(typeof(BugHouse))]
public class A_DelayedDetonation : BaseTest
{
    [CodeExample]
    public class BugHouse
    {
        private int count;
        public bool Run(int a)
        {
            return !(count++ == 2 && a == 1);
        }
    }

    [Fact]
    [DocContent(
@"Throws only after exactly three total runs when the input value is 1.  
Demonstrates a simple stateful failure triggered by run count.")]
    [DocExample(typeof(A_DelayedDetonation), nameof(GetCheckr))]
    [DocHeader("Minimal Fail Case:")]
    [DocContent(
@"- **Length:** 3 executions.
- **Example input:** `[42, 666, 1]`
```csharp
count = 0
Run( a = <any> ) // count = 1
Run( a = <any> ) // count = 2
Run( a = 1 ) // count == 2 && a == 1 → fail
```")]
    [DocHeader("Report:")]
    [DocCodeFile("A_DelayedDetonation.txt", "text")]
    public void RunTest()
    {
        var article = Journalist.Investigate(GetCheckr(), a => a

                .Run(1914853878, 10.ExecutionsPerRun()));

        WriteReport<A_DelayedDetonation>(article);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("Returns true", article.FailureDescription());
        Assert.Equal(2, article.Total().Executions());
        Assert.Equal(2, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal(2, article.Execution(1).Times);
        Assert.Equal("BugHouse.Run", article.Execution(1).Action(1).Read().Label);
        Assert.Equal(3, article.Execution(2).Read().ExecutionId);
        Assert.Equal("BugHouse.Run", article.Execution(2).Action(1).Read().Label);
        Assert.Equal("a", article.Execution(2).Input(1).Read().Label);
        Assert.Equal("1", article.Execution(2).Input(1).Read().Value);
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
}
