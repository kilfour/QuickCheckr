using QuickCheckr.Code;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickCheckr.UnderTheHood.Protocol;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.PersonalNotes.T_Examples.D_ThereIsAlwaysAnAccount;

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
    [DocHeader("Original Run:")]
    [DocCodeFile("ThereIsAlwaysAnAccountFullRun.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(10.ExecutionsPerRun(), a => a with { Seed = 1863050139 }));
        WriteAsserts(article);
        WriteReport<ThereIsAlwaysAnAccount>(article);
        Verify(article);
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

    [Fact]
    public void FullRunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(10.ExecutionsPerRun(), a => a with { Seed = 1863050139, ShrinkMode = ShrinkMode.None }));
        WriteAsserts(article);
        WriteReport(article, "ThereIsAlwaysAnAccountFullRun.txt");
        VerifyFullRun(article);
    }

    private static void VerifyFullRun(Article article)
    {
        Assert.Equal("No Overdraft", article.FailedSpec());
        Assert.Equal(5, article.Total().Executions());
        Assert.Equal(5, article.Total().Actions());
        Assert.Equal(5, article.Total().Traces());
        Assert.Equal(1, article.Total().PassedSpecs());
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Deposit", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Amount", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("94", article.Execution(1).Trace(1).Read().Value);
        Assert.Equal(2, article.Execution(2).Read().ExecutionId);
        Assert.Equal("Deposit", article.Execution(2).Action(1).Read().Label);
        Assert.Equal("Amount", article.Execution(2).Trace(1).Read().Label);
        Assert.Equal("59", article.Execution(2).Trace(1).Read().Value);
        Assert.Equal(3, article.Execution(3).Read().ExecutionId);
        Assert.Equal("Withdraw", article.Execution(3).Action(1).Read().Label);
        Assert.Equal("Amount", article.Execution(3).Trace(1).Read().Label);
        Assert.Equal("43", article.Execution(3).Trace(1).Read().Value);
        Assert.Equal(4, article.Execution(4).Read().ExecutionId);
        Assert.Equal("Withdraw", article.Execution(4).Action(1).Read().Label);
        Assert.Equal("Amount", article.Execution(4).Trace(1).Read().Label);
        Assert.Equal("63", article.Execution(4).Trace(1).Read().Value);
        Assert.Equal(5, article.Execution(5).Read().ExecutionId);
        Assert.Equal("Withdraw", article.Execution(5).Action(1).Read().Label);
        Assert.Equal("Amount", article.Execution(5).Trace(1).Read().Label);
        Assert.Equal("88", article.Execution(5).Trace(1).Read().Value);
        Assert.Equal("No Overdraft", article.PassedSpec(1).Read().Label);
        Assert.Equal(4, article.PassedSpec(1).Read().TimesPassed);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from account in Trackr.Stashed(() => new Account())
            from amount in Checkr.Input("Amount", Fuzzr.Int(), a => a with { Reducer = TowardsZero })
            from trace in Checkr.Trace("Amount", () => amount)
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

    [Fact]
    public void CodeTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr_Code().Run(10.ExecutionsPerRun(), a => a with { Seed = 1863050139 }));
        WriteAsserts(article);
        WriteReport(article, "ThereIsAlwaysAnAccountCode.txt", true);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr_Code()
    {
        return
            from codeAccount in Codr.Arrange("Account", () => "var account = new Account();")
            from account in Trackr.Stashed(() => new Account())
            from amount in Checkr.Input("Amount", Fuzzr.Int(), a => a with { Reducer = TowardsZero })
            from operations in Checkr.OneOf(
                //--
                from codeDeposit in Codr.Act("Deposit", () => $"account.Deposit({amount});")
                from deposit in Checkr.Act("Deposit", () => account.Deposit(amount))
                select Case.Closed,
                //--
                from codeWithdraw in Codr.Act("Withdraw", () => $"account.Withdraw({amount});")
                from withdraw in Checkr.Act("Withdraw", () => account.WithDraw(amount))
                select Case.Closed
            )
            from codeWithdraw in Codr.Act("Withdraw", () => $"account.Withdraw({amount});")
            from noOverdraft in Checkr.Spec("No Overdraft", () => account.Balance >= 0)
            select Case.Closed;
    }
}