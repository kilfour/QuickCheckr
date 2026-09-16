using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.FilingCabinet;
using QuickCheckr.FilingCabinet.Depositions;
using QuickCheckr.Tests.Tools;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickCheckr.UnderTheHood.Warnings;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Drafts.B_PuttingACaseOnTheBoard.C_Configuration.F_DeliberationPolicy;

//[DocFile]
[DocContent(
@"Sometimes the shrinker comes up with a *local* minimal failing case, not *the* minimal failing case.  
Declaring run-level deliberation inside the Checkr allows us to explore the input further in a directed fashion.

In short: Deliberation controls how agressive the shrinker keeps searching after it has found a *local* minimal failing case.

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
        var article = Journalist.Publish(GetCheckr(), a => a
            .Run(1920827676, 10.ExecutionsPerRun()));
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
    private static CheckrOf<Case> GetDeliberatedCheckr() =>
        from deliberate in Trackr.Deliberate(Deliberation.MinimumExecutions())
        from check in GetCheckr()
        select Case.Closed;


    [Fact]
    [DocContent(
@"As you can see, this is a *local* minimal failing case, but a smaller one exists.  

Let's deliberate over the completed runs, looking for a fail-case with less executions:")]
    [DocExample(typeof(DeliberationPolicyTests), nameof(GetDeliberatedCheckr))]
    [DocContent("Running this test now, with `100.Runs()` and `10.ExecutionsPerRun()` consistently produces a two execution failure.")]
    [DocCodeFile("DeliberationPolicyMinimal.txt", "text")]
    [DocContent("The following predefined policies are available")]
    public void RunTest_Deliberated()
    {
        var article = Journalist.Publish(GetDeliberatedCheckr(), a => a
            .Run(100.Runs(), 10.ExecutionsPerRun()));
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
    [DocContent($"- `{nameof(Deliberation)}.{nameof(Deliberation.MinimumExecutions)}`")]
    public void PolicyMinimumExecutions()
    {
        var caseFile1 = CaseFile.From(new FailureInfo(), new RunInfo(Seed: 1))
            .AddExecutionDeposition(new ExecutionDeposition(1));
        var caseFile2 = CaseFile.From(new FailureInfo(), new RunInfo(Seed: 2));
        var deliberator = new Deliberator(Deliberation.MinimumExecutions());
        deliberator.Check(caseFile1);
        deliberator.Check(caseFile2);
        Assert.Equal(2, deliberator.GetCaseFile().Seed);
    }

    [Fact]
    [DocContent($"- `{nameof(Deliberation)}.{nameof(Deliberation.MinimumOriginalExecutions)}`")]
    public void PolicyMinimumOriginalExecutions()
    {
        var caseFile1 = CaseFile.From(new FailureInfo(), new RunInfo(OriginalRunExecutionCount: 2, Seed: 1))
            .AddExecutionDeposition(new ExecutionDeposition(1));
        var caseFile2 = CaseFile.From(new FailureInfo(), new RunInfo(OriginalRunExecutionCount: 3, Seed: 2));
        var deliberator = new Deliberator(Deliberation.MinimumOriginalExecutions());
        deliberator.Check(caseFile1);
        deliberator.Check(caseFile2);
        Assert.Equal(1, deliberator.GetCaseFile().Seed);
    }

    [Fact]
    [DocContent($"- `{nameof(Deliberation)}.{nameof(Deliberation.MinimumShrinkCount)}`")]
    public void PolicyMinimumShrinkCount()
    {
        var caseFile1 = CaseFile.From(new FailureInfo(), new RunInfo(Seed: 1, ShrinkCount: 1));
        var caseFile2 = CaseFile.From(new FailureInfo(), new RunInfo(Seed: 2, ShrinkCount: 2));
        var deliberator = new Deliberator(Deliberation.MinimumShrinkCount());
        deliberator.Check(caseFile1);
        deliberator.Check(caseFile2);
        Assert.Equal(1, deliberator.GetCaseFile().Seed);
    }

    [Fact]
    [DocContent($"- `{nameof(Deliberation)}.{nameof(Deliberation.MaximumShrinkCount)}`")]
    public void PolicyMaximumShrinkCount()
    {
        var caseFile1 = CaseFile.From(new FailureInfo(), new RunInfo(Seed: 1, ShrinkCount: 1));
        var caseFile2 = CaseFile.From(new FailureInfo(), new RunInfo(Seed: 2, ShrinkCount: 2));
        var deliberator = new Deliberator(Deliberation.MaximumShrinkCount());
        deliberator.Check(caseFile1);
        deliberator.Check(caseFile2);
        Assert.Equal(2, deliberator.GetCaseFile().Seed);
    }

    [Fact]
    [DocContent($"- `{nameof(Deliberation)}.{nameof(Deliberation.MinimumWarnings)}`")]
    public void PolicyMinimumWarnings()
    {
        var caseFile1 = CaseFile.From(new FailureInfo(), new RunInfo(Seed: 1));
        var caseFile2 = CaseFile.From(new FailureInfo(), new RunInfo(Seed: 2))
            .AddExecutionDeposition(new ExecutionDeposition(1)
                .AddWarningDeposition(new WarningDeposition(new AllInputsIrrelevantWarning())));
        var deliberator = new Deliberator(Deliberation.MinimumWarnings());
        deliberator.Check(caseFile1);
        deliberator.Check(caseFile2);
        Assert.Equal(1, deliberator.GetCaseFile().Seed);
    }

    [Fact]
    [DocContent($"- `{nameof(Deliberation)}.{nameof(Deliberation.MaximumExecutionsShrunk)}`")]
    public void PolicyMaximumExecutionsShrunk()
    {
        var caseFile1 = CaseFile.From(new FailureInfo(), new RunInfo(OriginalRunExecutionCount: 3, Seed: 1))
            .AddExecutionDeposition(new ExecutionDeposition(1));
        var caseFile2 = CaseFile.From(new FailureInfo(), new RunInfo(OriginalRunExecutionCount: 2, Seed: 2))
            .AddExecutionDeposition(new ExecutionDeposition(1));
        var deliberator = new Deliberator(Deliberation.MaximumExecutionsShrunk());
        deliberator.Check(caseFile1);
        deliberator.Check(caseFile2);
        Assert.Equal(1, deliberator.GetCaseFile().Seed);
    }

    [Fact]
    [DocContent($"- `{nameof(Deliberation)}.{nameof(Deliberation.MaximumDistinctPassedExpectations)}`")]
    public void PolicyMaximumDistinctPassedExpectations()
    {
        var caseFile1 = CaseFile.From(new FailureInfo(), new RunInfo(OriginalRunExecutionCount: 3, Seed: 1))
            .AddPassedExpectationDeposition(new PassedExpectation("One", 1000));
        var caseFile2 = CaseFile.From(new FailureInfo(), new RunInfo(OriginalRunExecutionCount: 2, Seed: 2))
            .AddPassedExpectationDeposition(new PassedExpectation("One", 1))
            .AddPassedExpectationDeposition(new PassedExpectation("Two", 1));
        var deliberator = new Deliberator(Deliberation.MaximumDistinctPassedExpectations());
        deliberator.Check(caseFile1);
        deliberator.Check(caseFile2);
        Assert.Equal(2, deliberator.GetCaseFile().Seed);
    }
}
