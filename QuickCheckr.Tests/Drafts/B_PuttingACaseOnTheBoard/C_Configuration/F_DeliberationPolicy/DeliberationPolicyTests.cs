using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickCheckr.UnderTheHood.Proceedings.Depositions;
using QuickCheckr.UnderTheHood.Warnings;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.C_Configuration.F_DeliberationPolicy;

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
    [DocContent("The following predefined policies are available")]
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
    [DocContent($"- `{nameof(DeliberationPolicy)}.{nameof(DeliberationPolicy.MinimumExecutions)}`")]
    public void PolicyMinimumExecutions()
    {
        var caseFile1 = CaseFile.From(new FailureInfo(), new RunInfo(Seed: 1))
            .AddExecutionDeposition(new ExecutionDeposition(1));
        var caseFile2 = CaseFile.From(new FailureInfo(), new RunInfo(Seed: 2));
        var deliberator = new Deliberator(DeliberationPolicy.MinimumExecutions, null);
        deliberator.Check(caseFile1);
        deliberator.Check(caseFile2);
        Assert.Equal(2, deliberator.GetCaseFile().Seed);
    }

    [Fact]
    [DocContent($"- `{nameof(DeliberationPolicy)}.{nameof(DeliberationPolicy.MinimumOriginalExecutions)}`")]
    public void PolicyMinimumOriginalExecutions()
    {
        var caseFile1 = CaseFile.From(new FailureInfo(), new RunInfo(OriginalRunExecutionCount: 2, Seed: 1))
            .AddExecutionDeposition(new ExecutionDeposition(1));
        var caseFile2 = CaseFile.From(new FailureInfo(), new RunInfo(OriginalRunExecutionCount: 3, Seed: 2));
        var deliberator = new Deliberator(DeliberationPolicy.MinimumOriginalExecutions, null);
        deliberator.Check(caseFile1);
        deliberator.Check(caseFile2);
        Assert.Equal(1, deliberator.GetCaseFile().Seed);
    }

    [Fact]
    [DocContent($"- `{nameof(DeliberationPolicy)}.{nameof(DeliberationPolicy.MinimumShrinkCount)}`")]
    public void PolicyMinimumShrinkCount()
    {
        var caseFile1 = CaseFile.From(new FailureInfo(), new RunInfo(Seed: 1, ShrinkCount: 1));
        var caseFile2 = CaseFile.From(new FailureInfo(), new RunInfo(Seed: 2, ShrinkCount: 2));
        var deliberator = new Deliberator(DeliberationPolicy.MinimumShrinkCount, null);
        deliberator.Check(caseFile1);
        deliberator.Check(caseFile2);
        Assert.Equal(1, deliberator.GetCaseFile().Seed);
    }

    [Fact]
    [DocContent($"- `{nameof(DeliberationPolicy)}.{nameof(DeliberationPolicy.MaximumShrinkCount)}`")]
    public void PolicyMaximumShrinkCount()
    {
        var caseFile1 = CaseFile.From(new FailureInfo(), new RunInfo(Seed: 1, ShrinkCount: 1));
        var caseFile2 = CaseFile.From(new FailureInfo(), new RunInfo(Seed: 2, ShrinkCount: 2));
        var deliberator = new Deliberator(DeliberationPolicy.MaximumShrinkCount, null);
        deliberator.Check(caseFile1);
        deliberator.Check(caseFile2);
        Assert.Equal(2, deliberator.GetCaseFile().Seed);
    }

    [Fact]
    [DocContent($"- `{nameof(DeliberationPolicy)}.{nameof(DeliberationPolicy.MinimumWarnings)}`")]
    public void PolicyMinimumWarnings()
    {
        var caseFile1 = CaseFile.From(new FailureInfo(), new RunInfo(Seed: 1));
        var caseFile2 = CaseFile.From(new FailureInfo(), new RunInfo(Seed: 2))
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddWarningDeposition(new WarningDeposition(new AllInputsIrrelevantWarning())));
        var deliberator = new Deliberator(DeliberationPolicy.MinimumWarnings, null);
        deliberator.Check(caseFile1);
        deliberator.Check(caseFile2);
        Assert.Equal(1, deliberator.GetCaseFile().Seed);
    }

    [Fact]
    [DocContent($"- `{nameof(DeliberationPolicy)}.{nameof(DeliberationPolicy.MaximumExecutionsShrunk)}`")]
    public void PolicyMaximumExecutionsShrunk()
    {
        var caseFile1 = CaseFile.From(new FailureInfo(), new RunInfo(OriginalRunExecutionCount: 3, Seed: 1))
            .AddExecutionDeposition(new ExecutionDeposition(1));
        var caseFile2 = CaseFile.From(new FailureInfo(), new RunInfo(OriginalRunExecutionCount: 2, Seed: 2))
            .AddExecutionDeposition(new ExecutionDeposition(1));
        var deliberator = new Deliberator(DeliberationPolicy.MaximumExecutionsShrunk, null);
        deliberator.Check(caseFile1);
        deliberator.Check(caseFile2);
        Assert.Equal(1, deliberator.GetCaseFile().Seed);
    }

    [Fact]
    [DocContent($"- `{nameof(DeliberationPolicy)}.{nameof(DeliberationPolicy.MaximumDistinctPassedExpectations)}`")]
    public void PolicyMaximumDistinctPassedExpectations()
    {
        var caseFile1 = CaseFile.From(new FailureInfo(), new RunInfo(OriginalRunExecutionCount: 3, Seed: 1))
            .AddPassedExpectationDeposition(new PassedExpectationDeposition("One", 1000));
        var caseFile2 = CaseFile.From(new FailureInfo(), new RunInfo(OriginalRunExecutionCount: 2, Seed: 2))
            .AddPassedExpectationDeposition(new PassedExpectationDeposition("One", 1))
            .AddPassedExpectationDeposition(new PassedExpectationDeposition("Two", 1));
        var deliberator = new Deliberator(DeliberationPolicy.MaximumDistinctPassedExpectations, null);
        deliberator.Check(caseFile1);
        deliberator.Check(caseFile2);
        Assert.Equal(2, deliberator.GetCaseFile().Seed);
    }
}