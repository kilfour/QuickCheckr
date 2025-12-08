using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickCheckr.UnderTheHood.Protocol;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Docs.H_OnShrinking.F_DeliberationShrinking;

[DocFile]
public class DeliberationShrinking : BaseTest
{
    [CodeExample]
    public class Account
    {
        public int Balance { get; private set; } = 0;
        public void Deposit(int amount)
        {
            Balance += amount;
            ApplyFee();
        }

        public void WithDraw(int amount)
        {
            if (amount > Balance) return;
            Balance -= amount;
            ApplyFee();
        }

        private void ApplyFee() => Balance -= 1;
    }

    [Fact]
    [DocHeader("The Model:")]
    [DocExample(typeof(Account))]
    [DocHeader("The Checkr:")]
    [DocExample(typeof(DeliberationShrinking), nameof(GetCheckr))]
    [DocHeader("The Configuration:")]
    [DocExample(typeof(DeliberationShrinking), nameof(Configure))]
    [DocHeader("The Report:")]
    [DocCodeFile("DeliberationShrinking.txt", "text")]
    [DocContent("Running this test with `100.Runs()` and `10.ExecutionsPerRun()` consistently produces a two execution failure.")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(100.Runs(), 10.ExecutionsPerRun(), Configure));
        WriteAsserts(article, false);
        WriteReport<DeliberationShrinking>(article, false);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("No Overdraft", article.FailedSpec());
        Assert.Equal(2, article.Total().Executions());
        Assert.Equal(2, article.Total().Actions());
        Assert.Equal(2, article.Total().Inputs());
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from account in Trackr.Stashed(() => new Account())
            from amount in Checkr.Input("Amount", Fuzzr.Int(1, 5))
            from operations in Checkr.OneOf(
                Checkr.Act("Deposit", () => account.Deposit(amount)),
                Checkr.Act("Withdraw", () => account.WithDraw(amount))
            )
            from noOverdraft in Checkr.Spec("No Overdraft", () => account.Balance >= 0)
            select Case.Closed;
    }

    [CodeExample]
    private static CheckrConfig Configure(CheckrConfig config)
        => config with { DeliberationPolicy = (a, b) => a.Verdict!.ExecutionCount < b.Verdict!.ExecutionCount ? a : b };
}