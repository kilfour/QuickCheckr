using QuickCheckr;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Notes.T_Examples.D_ThereIsAlwaysAnAccount;

[DocFile]
public class ThereIsAlwaysAnAccount : BaseTest
{
    [Fact]
    [DocHeader("The Model:")]
    [DocExample(typeof(Account))]
    [DocHeader("The Checkr:")]
    [DocExample(typeof(ThereIsAlwaysAnAccount), nameof(GetCheckr))]
    [DocHeader("The Report:")]
    [DocCodeFile("ThereIsAlwaysAnAccount.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(10.ExecutionsPerRun(), a => a with { Seed = 1863050139 }));
        WriteAsserts(article);
        WriteReport<ThereIsAlwaysAnAccount>(article);
        Verify(article);
        ExplainMe<ThereIsAlwaysAnAccount>();
    }

    private static void Verify(Article article)
    {
        Assert.Equal("No Overdraft", article.FailedSpec());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(5, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Withdraw", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Amount", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("1", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Amount", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("88", article.Execution(1).Trace(1).Read().Value);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from account in Trackr.Stashed(() => new Account())
            from amount in Checkr.Input("Amount", Fuzzr.Int(), a => a with { Reducer = TowardsZero })
            from trace in Checkr.Trace("Amount", amount)
            from operations in Checkr.OneOf(
                Checkr.Act("Deposit", () => account.Deposit(amount)),
                Checkr.Act("Withdraw", () => account.WithDraw(amount))
            )
            from noOverdraft in Checkr.Spec("No Overdraft", () => account.Balance >= 0)
            select Case.Closed;
    }

    private static IEnumerable<int> TowardsZero(int input)
    {
        var local = input;
        while (local != 0)
        {
            if (local > 0)
                yield return --local;
            else
                yield return ++local;
        }
    }

    [CodeExample]
    public class Account
    {
        public int Balance { get; private set; }
        public void Deposit(int amount) => Balance += amount;
        public void WithDraw(int amount) => Balance -= amount;
    }
}