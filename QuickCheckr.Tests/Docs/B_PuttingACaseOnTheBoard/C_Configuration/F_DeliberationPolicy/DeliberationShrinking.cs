using QuickCheckr;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.C_Objects.TheExamples.Models;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.C_Configuration.F_DeliberationPolicy;

[DocFile]
[DocContent(
@"Sometimes the shrinker comes up with a *local* minimal fail-case, not *the* minimal fail-case.  
Configuring a *DeliberationPolicy* allows us to instruct the Shrinker to explore the input further in a directed fasion.  

Consider the following model:")]
[DocExample(typeof(Account))]
[DocContent("It's a simple bank account with a no-overdraft invariant and a fee applied on each transaction.")]
public class DeliberationPolicy : BaseTest
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
    [DocExample(typeof(DeliberationPolicy), nameof(GetCheckr))]
    [DocContent("Now if we run this we might get something like this:")]
    [DocCodeFile("DeliberationPolicy.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(1920827676, 10.ExecutionsPerRun()));
        WriteAsserts(article, false);
        WriteReport<DeliberationPolicy>(article, false);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("No Overdraft", article.FailedExpectation());
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
        => config with { DeliberationPolicy = Protocol.DeliberationPolicy.MinimumExecutions };


    [Fact]
    [DocContent(
@"As you can see, this is a *local* minimal fail-case, but a smaller one exists.  

Let's add a *DeliberationPolicy*, looking for a fail-case with less executions:")]
    [DocExample(typeof(DeliberationPolicy), nameof(Configure))]
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
        Assert.Equal("No Overdraft", article.FailedExpectation());
        Assert.Equal(2, article.Total().Executions());
        Assert.Equal(2, article.Total().Actions());
        Assert.Equal(2, article.Total().Inputs());
    }

    [Fact]
    [DocHeader("An Eager Shrinker")]
    [DocContent(
@"Under some rare circumstances, the opposite can also occur, and the Shrinker gets *greedy*.  
Currently the only identified cases where this happens is if more than one input triggers the same failure.")]
    [DocExample(typeof(DeliberationPolicy), nameof(GetCheckrGreedy))]
    [DocContent(
@"Now when the list contains multiple elements `1`, we might get:")]
    [DocCodeFile("DeliberationPolicyGreedy.txt", "text")]
    [DocContent(
@"It seems QuickCheckr thinks that the minimal fail-case is a list with atleast 3 elements (*correct*),
but the element values are considered irrelevant (*wrong*).")]
    public void RunTest_DeliberatedGreedy()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckrGreedy().Run(227109555, 1.ExecutionsPerRun()));
        WriteAsserts(article, false);
        WriteReport(article, "DeliberationPolicyGreedy.txt", false);
        Verify_DeliberatedGreedy(article);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckrGreedy()
    {
        return
            from input in Checkr.Input("Input", Fuzzr.Int(0, 5).Many(3))
            from trace in Checkr.Trace("List", () => input)
            from expect in Checkr.ExpectWhen("Does not contain 1",
                () => input.Count() > 2,
                () => !input.Contains(1))
            select Case.Closed;
    }

    private static void Verify_DeliberatedGreedy(Article article)
    {
        Assert.Equal("Does not contain 1", article.FailedExpectation());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(3, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ _, _, _ ]", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("List", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("[ 1, 2, 1 ]", article.Execution(1).Trace(1).Read().Value);
    }

    [Fact]
    [DocContent("For this rare case we can also add a DeliberationPolicy, allowing QuickFuzzr to surface a more explanatory fail-case:")]
    [DocExample(typeof(DeliberationPolicy), nameof(ConfigureShrinkCount))]
    [DocContent("**Report:**")]
    [DocCodeFile("DeliberationPolicyGreedyFixed.txt", "text")]
    public void RunTest_DeliberatedGreedyFixed()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckrGreedy().Run(20.Runs(), 1.ExecutionsPerRun(), ConfigureShrinkCount));
        WriteAsserts(article, false);
        WriteReport(article, "DeliberationPolicyGreedyFixed.txt", false);
        Verify_DeliberatedGreedyFixed(article);
    }

    [CodeExample]
    private static CheckrConfig ConfigureShrinkCount(CheckrConfig config)
        => config with { DeliberationPolicy = Protocol.DeliberationPolicy.MinimumShrinkCount };

    private static void Verify_DeliberatedGreedyFixed(Article article)
    {
        Assert.NotEqual("[ _, _, _ ]", article.Execution(1).Input(1).Read().Value);
    }

    [Fact]
    [DocContent("A similar thing can happen when shrinking objects, and the solution is exactly the same")]
    [DocExample(typeof(DeliberationPolicy), nameof(GetCheckrGreedyObjects))]
    [DocContent("**With DeliberationPolicy:**")]
    [DocCodeFile("DeliberationPolicyGreedyObjects.txt", "text")]
    public void RunTest_DeliberatedGreedyObjects()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckrGreedyObjects().Run(20.Runs(), 1.ExecutionsPerRun(), ConfigureShrinkCount));
        WriteAsserts(article, false);
        WriteReport(article, "DeliberationPolicyGreedyObjects.txt", false);
        Verify_DeliberatedGreedyObjects(article);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckrGreedyObjects()
    {
        return
            from obj in Checkr.Input("BaggyInters",
                from cfg in Configr.Primitive(Fuzzr.Int(1, 5))
                from result in Fuzzr.One<BaggyInters>()
                select result)
            from trace in Checkr.Trace("Original", () => obj)
            from expect in Checkr.Expect("obj.IntOne == 1 || obj.IntThree == 1",
                () => obj.IntOne != 1 && obj.IntThree != 1)
            select Case.Closed;
    }

    private static void Verify_DeliberatedGreedyObjects(Article article)
    {
        Assert.Equal("obj.IntOne == 1 || obj.IntThree == 1", article.FailedExpectation());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
    }
}