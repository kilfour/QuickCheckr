using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Docs.A_Guide.C_ChoosingTheNextMove;

[DocFile]
[DocContent(
@"In the previous chapter we mentioned *Pools* and promised to explain how they work.  
We'll get there soon, but first we need to talk about `Checkr.OneOf`, the tool that lets QuickCheckr explore branching behaviour.

`OneOf` is QuickCheckr's way of saying:

> **""At this step, choose *exactly one* of these possible behaviours and execute it.""**

It is the foundational building block for introducing randomness into *what happens next*.
Instead of describing one action, you describe a *set* of alternative next moves,
and QuickCheckr picks one at random during each execution.

Let's keep the model simple:")]
[DocExample(typeof(Account))]
public class ChoosingTheNextMove : BaseTest
{
    [CodeExample]
    public class Account
    {
        public int Balance { get; private set; }
        public void Deposit(int amount) => Balance += amount;
        public void WithDraw(int amount) => Balance -= amount;
    }

    [Fact]
    [DocContent(
@"Suppose our system has a (not implemented here) 'no overdraft' invariant.
The bug is trivial, but that's fine: we're here to demonstrate *behavioural branching*, not banking security.")]
    [DocHeader("A Checkr With a Choice")]
    [DocExample(typeof(ChoosingTheNextMove), nameof(GetCheckr))]
    [DocContent(
@"Every execution through this Checkr does the same things:

1. Generate a random amount.
2. Pick exactly **one** action: `Deposit` *or* `Withdraw`.
3. Check the invariant.

A key detail:

> **Each branch runs inside the same execution scope.**
> This means both branches mutate the same stashed `account` instance.

So across executions, QuickCheckr builds a sequence of deposits and withdrawals, any of which may lead to a failure.
")]
    [DocHeader("Seeing Which Branches Were Chosen")]
    [DocContent("Running the Checkr without shrinking yields a report similar to this:")]
    [DocCodeFile("ChoosingTheNextMove.txt", "text")]
    [DocContent(
@"This shows us that **QuickCheckr records the branch actually taken for each execution**.    
There is no guesswork.")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(1169776765, 10.ExecutionsPerRun(), a => a with { ShrinkMode = ShrinkMode.Inputs }));
        WriteAsserts(article);
        WriteReport<ChoosingTheNextMove>(article);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("No Overdraft", article.FailedSpec());
        Assert.Equal(8, article.Total().Executions());
        Assert.Equal(8, article.Total().Actions());
        Assert.Equal(8, article.Total().Inputs());
        Assert.Equal(1, article.Total().PassedSpecs());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Deposit", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Amount", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("71", article.Execution(1).Input(1).Read().Value);
        Assert.Equal(2, article.Execution(2).Read().ExecutionId);
        Assert.Equal("Deposit", article.Execution(2).Action(1).Read().Label);
        Assert.Equal("Amount", article.Execution(2).Input(1).Read().Label);
        Assert.Equal("5", article.Execution(2).Input(1).Read().Value);
        Assert.Equal(3, article.Execution(3).Read().ExecutionId);
        Assert.Equal("Withdraw", article.Execution(3).Action(1).Read().Label);
        Assert.Equal("Amount", article.Execution(3).Input(1).Read().Label);
        Assert.Equal("31", article.Execution(3).Input(1).Read().Value);
        Assert.Equal(4, article.Execution(4).Read().ExecutionId);
        Assert.Equal("Deposit", article.Execution(4).Action(1).Read().Label);
        Assert.Equal("Amount", article.Execution(4).Input(1).Read().Label);
        Assert.Equal("15", article.Execution(4).Input(1).Read().Value);
        Assert.Equal(5, article.Execution(5).Read().ExecutionId);
        Assert.Equal("Deposit", article.Execution(5).Action(1).Read().Label);
        Assert.Equal("Amount", article.Execution(5).Input(1).Read().Label);
        Assert.Equal("56", article.Execution(5).Input(1).Read().Value);
        Assert.Equal(6, article.Execution(6).Read().ExecutionId);
        Assert.Equal("Withdraw", article.Execution(6).Action(1).Read().Label);
        Assert.Equal("Amount", article.Execution(6).Input(1).Read().Label);
        Assert.Equal("94", article.Execution(6).Input(1).Read().Value);
        Assert.Equal(7, article.Execution(7).Read().ExecutionId);
        Assert.Equal("Deposit", article.Execution(7).Action(1).Read().Label);
        Assert.Equal("Amount", article.Execution(7).Input(1).Read().Label);
        Assert.Equal("36", article.Execution(7).Input(1).Read().Value);
        Assert.Equal(8, article.Execution(8).Read().ExecutionId);
        Assert.Equal("Withdraw", article.Execution(8).Action(1).Read().Label);
        Assert.Equal("Amount", article.Execution(8).Input(1).Read().Label);
        Assert.Equal("66", article.Execution(8).Input(1).Read().Value);
        Assert.Equal("No Overdraft", article.PassedSpec(1).Read().Label);
        Assert.Equal(7, article.PassedSpec(1).Read().TimesPassed);
    }

    [Fact]
    [DocHeader("Shrinking Branching Behaviour")]
    [DocContent("And if we let the QuickCheckr shrinker do its work:")]
    [DocCodeFile("ChoosingTheNextMoveShrunk.txt", "text")]
    [DocContent(
@"The shrunk case shows:

* Only one execution was *relevant*.
* The relevant action was a `Withdraw`.
* The amount was positive, so withdrawing it immediately violates the invariant.

This highlights an important point:

> **QuickCheckr does not explore alternative branches during shrinking.**  
> It only shrinks *what actually happened*.

Shrinking removes irrelevant **executions**, trims **inputs**, and discards everything that had no influence on the failure.
The branch chosen during the original failing run is *the* branch examined during shrinking. ")]
    [DocHeader("Recap")]
    [DocContent(
@"`Checkr.OneOf` introduces controlled nondeterminism:

* Each execution picks **exactly one** branch.
* All branches operate on the same stashed state.
* Reports show exactly which branch was chosen.
* Shrinking works naturally: QuickCheckr does **not** try unchosen branches. 
  It only minimizes the actual failing scenario.")]
    public void RunTest_Shrunk()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(1169776765, 10.ExecutionsPerRun()));
        WriteAsserts(article);
        WriteReport(article, "ChoosingTheNextMoveShrunk.txt", false);
        Verify_Shrunk(article);
    }

    private static void Verify_Shrunk(Article article)
    {
        Assert.Equal("No Overdraft", article.FailedSpec());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().PassedSpecs());
        Assert.Equal(7, article.ShrinkCount);
        Assert.Equal(8, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Withdraw", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Amount", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("66", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("No Overdraft", article.PassedSpec(1).Read().Label);
        Assert.Equal(7, article.PassedSpec(1).Read().TimesPassed);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from account in Trackr.Stashed(() => new Account())
            from amount in Checkr.Input("Amount", Fuzzr.Int())
            from operations in Checkr.OneOf(
                Checkr.Act("Deposit", () => account.Deposit(amount)),
                Checkr.Act("Withdraw", () => account.WithDraw(amount))
            )
            from noOverdraft in Checkr.Expect("No Overdraft", () => account.Balance >= 0)
            select Case.Closed;
    }
}