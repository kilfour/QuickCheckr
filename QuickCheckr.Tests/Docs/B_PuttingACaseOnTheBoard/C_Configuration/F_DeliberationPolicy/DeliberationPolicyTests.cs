using QuickCheckr.Protocol;
using QuickCheckr.Tests.Docs.H_OnShrinking.F_DeliberationShrinking;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.C_Configuration.F_DeliberationPolicy;

//[DocFile]
[DocContent(
@"Sometimes the shrinker comes up with a *local* minimal fail-case, not *the* minimal fail-case.  
Configuring a *DeliberationPolicy* allows us to instruct the Shrinker to explore the input further in a directed fasion.  

In short: Deliberation controls how agressive the shrinker keeps searching after it has found a *local* minimal fail-case.

Consider the following model:")]
[DocExample(typeof(Account))]
[DocContent("It's a simple bank account with a no-overdraft invariant and a fee applied on each transaction.")]
public class DeliberationPolicyTests : BaseTest
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

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from account in Trackr.Stashed(() => new Account())
            from amount in Checkr.Input("Amount", Fuzzr.Int(1, 10))
            from operations in Checkr.OneOf(
                Checkr.Act("Deposit", () => account.Deposit(amount)),
                Checkr.Act("Withdraw", () => account.WithDraw(amount))
            )
            from noOverdraft in Checkr.Expect("No Overdraft", () => account.Balance >= 0)
            select Case.Closed;
    }

    [Fact]
    [DocContent("We can verify this with the following Checkr:")]
    [DocExample(typeof(DeliberationPolicyTests), nameof(GetCheckr))]
    [DocContent("Now if we run this we might get something like this:")]
    [DocCodeFile("DeliberationPolicy.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(1920827676, 10.ExecutionsPerRun()));
        WriteAsserts(article, false);
        WriteReport<DeliberationPolicyTests>(article, false);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("No Overdraft", article.FailureDescription());
        Assert.Equal(3, article.Total().Executions());
        Assert.Equal(3, article.Total().Actions());
        Assert.Equal(3, article.Total().Inputs());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Deposit", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Amount", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("8", article.Execution(1).Input(1).Read().Value);
        Assert.Equal(2, article.Execution(2).Read().ExecutionId);
        Assert.Equal("Withdraw", article.Execution(2).Action(1).Read().Label);
        Assert.Equal("Amount", article.Execution(2).Input(1).Read().Label);
        Assert.Equal("1", article.Execution(2).Input(1).Read().Value);
        Assert.Equal(3, article.Execution(3).Read().ExecutionId);
        Assert.Equal("Withdraw", article.Execution(3).Action(1).Read().Label);
        Assert.Equal("Amount", article.Execution(3).Input(1).Read().Label);
        Assert.Equal("5", article.Execution(3).Input(1).Read().Value);
        Assert.Equal("No Overdraft", article.PassedExpectation(1).Read().Label);
        Assert.Equal(2, article.PassedExpectation(1).Read().TimesPassed);
    }

    [CodeExample]
    private static CheckrConfig Configure(CheckrConfig config)
        => config with { DeliberationPolicy = DeliberationPolicy.MinimumExecutions };


    [Fact]
    [DocContent(
@"As you can see, this is a *local* minimal fail-case, but a smaller one exists.  

Let's add a *DeliberationPolicy*, looking for a fail-case with less executions:")]
    [DocExample(typeof(DeliberationPolicyTests), nameof(Configure))]
    [DocContent("Running this test now, with `100.Runs()` and `10.ExecutionsPerRun()` consistently produces a two execution failure.")]
    [DocCodeFile("DeliberationPolicyMinimal.txt", "text")]
    public void RunTest_Deliberated()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(100.Runs(), 10.ExecutionsPerRun(), Configure));
        WriteAsserts(article, false);
        WriteReport(article, "DeliberationPolicyMinimal.txt", false);
        Verify_Deliberated(article);
    }

    private static void Verify_Deliberated(Article article)
    {
        Assert.Equal("No Overdraft", article.FailureDescription());
        Assert.Equal(2, article.Total().Executions());
        Assert.Equal(2, article.Total().Actions());
        Assert.Equal(2, article.Total().Inputs());
    }

    [Fact]
    [DocContent("The following predefined policies are available")]
    [DocExample(typeof(DeliberationPolicyTests), nameof(PolicyList))]
    [DocContent(
@"
See the documentation for [*Deliberation Shrinking*][DeliberationShrinking] for more examples and usage of these policies.

It is also possible to define a custom policy, but this is an ...")]
    [DocExperimental]
    public void AvailablePolicies()
    {
        Assert.Equal(6, PolicyList().Count()); // need a better way to test this
    }

    [CodeSnippet]
    [CodeReplace("[ ", "- ")]
    [CodeReplace(", ", "- ")]
    [CodeRemove("];")]
    private static IEnumerable<Func<CaseFile, int>> PolicyList() =>
        [ DeliberationPolicy.MinimumExecutions
        , DeliberationPolicy.MinimumShrinkCount
        , DeliberationPolicy.MaximumShrinkCount
        , DeliberationPolicy.MinimumWarnings
        , DeliberationPolicy.MaximumExecutionsShrunk
        , DeliberationPolicy.MaximumDistinctPassedExpectations
        ];
}