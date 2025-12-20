using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Experimental.B_OverFlowing;

[DocFile]
[DocContent("This looks like a redundant example. I think it started as a case for Trackr.Analyze. It isn't anymore, and does not look like a good one for that anyway.")]
public class OverFlowing : BaseTest
{
    [Fact]
    [DocHeader("The SUT:")]
    [DocExample(typeof(Ledger))]
    [DocHeader("The Checkr:")]
    [DocExample(typeof(OverFlowing), nameof(GetCheckr))]
    [DocHeader("The Report:")]
    [DocCodeFile("OverFlowing.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(1990927690, 10.ExecutionsPerRun()));
        WriteAsserts(article, false);
        WriteReport<OverFlowing>(article, false);
        Verify(article);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from ledger in Trackr.Stashed(() => new Ledger())
            from entry in Checkr.Input("Entry", Fuzzr.OneOf(
                    (80, Fuzzr.Int(int.MaxValue / 2, int.MaxValue)),
                    (20, Fuzzr.Int(1, 10))
                ))
            from add in Checkr.Act("Add", () => ledger.Add(entry))
            from expect in Trackr.Expect(
                    "Checked total greater or equal than all entries", () =>
                        ledger.Entries.All(a => ledger.TotalChecked() >= a))
            select Case.Closed;
    }

    private static void Verify(Article article)
    {
        Assert.Equal("Checked total greater or equal than all entries", article.FailedExpectation());
        Assert.Equal(2, article.Total().Executions());
        Assert.Equal(2, article.Total().Actions());
        Assert.Equal(2, article.Total().Inputs());
        Assert.Equal(8, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Add", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Entry", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("1959373951", article.Execution(1).Input(1).Read().Value);
        Assert.Equal(10, article.Execution(2).Read().ExecutionId);
        Assert.Equal("Add", article.Execution(2).Action(1).Read().Label);
        Assert.Equal("Entry", article.Execution(2).Input(1).Read().Label);
        Assert.Equal("1930233829", article.Execution(2).Input(1).Read().Value);
    }

    [CodeExample]
    public sealed class Ledger
    {
        public readonly List<int> Entries = new();
        public void Add(int x) => Entries.Add(x);

        public int TotalChecked()
        {
            var sum = 0;
            foreach (var x in Entries) sum += x;
            return sum;
        }
    }
}