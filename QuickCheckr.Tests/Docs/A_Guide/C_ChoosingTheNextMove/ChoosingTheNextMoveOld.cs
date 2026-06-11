using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Docs.A_Guide.B_TheStashRunsAndExecutions;
using QuickCheckr.Tests.Docs.A_Guide.D_InPoolsWeTrust;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Docs.A_Guide.C_ChoosingTheNextMove;

//[DocFile]
[DocLink(typeof(Guide))]
[DocLink(typeof(TheStashRunsAndExecutions))]
[DocLink(typeof(InPoolsWeTrust))]
[DocFileHeader("Choosing the Next Move")]
[DocContent("[Guide][Guide] | [Previous][TheStashRunsAndExecutions] | [Next][InPoolsWeTrust]  \n")]
[DocContent(
@$"In the previous chapter we mentioned *Pools* and promised to explain how they work.  
We'll get there soon, but first we need to talk about `{nameof(Checkr)}.{nameof(Checkr.OneOf)}`, the tool that lets QuickCheckr explore branching behaviour.

`{nameof(Checkr.OneOf)}` is QuickCheckr's way of saying:

> **""At this step, choose *exactly one* of these possible behaviours and execute it.""**

It is the foundational building block for introducing randomness into *what happens next*.
Instead of describing one action, you describe a *set* of alternative next moves,
and QuickCheckr picks one at random during each execution.

Let's keep the model simple:")]
[DocExample(typeof(Account))]
public class ChoosingTheNextMoveOld : BaseTest
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
        var article = Journalist.Investigate(GetCheckr(), a => a
                .Configure(a => a with { ShrinkMode = ShrinkMode.Inputs })
                .Run(1684147193, 10.ExecutionsPerRun()));
        WriteAsserts(article, false);
        WriteReport<ChoosingTheNextMove>(article, false);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("No Overdraft", article.FailureDescription());
        Assert.Equal(2, article.Total().Executions());
        Assert.Equal(2, article.Total().Actions());
        Assert.Equal(2, article.Total().Inputs());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Deposit", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Amount", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("17", article.Execution(1).Input(1).Read().Value);
        Assert.Equal(2, article.Execution(2).Read().ExecutionId);
        Assert.Equal("Withdraw", article.Execution(2).Action(1).Read().Label);
        Assert.Equal("Amount", article.Execution(2).Input(1).Read().Label);
        Assert.Equal("87", article.Execution(2).Input(1).Read().Value);
        Assert.Equal("No Overdraft", article.PassedExpectation(1).Read().Label);
        Assert.Equal(1, article.PassedExpectation(1).Read().TimesPassed);
    }

    [Fact]
    [DocHeader("Shrinking Branching Behaviour")]
    [DocContent("And if we let the QuickCheckr shrinker do its work:")]
    [DocCodeFile("ChoosingTheNextMoveShrunk.txt", "text")]
    [DocContent(
@"The shrunk case shows:

* Only one execution was *relevant*.
* The relevant action was a `Withdraw`.
")]
    public void RunTest_Shrunk()
    {
        var article = Journalist.Investigate(GetCheckr(), a => a.Run(1684147193, 10.ExecutionsPerRun()));
        WriteAsserts(article, false);
        WriteReport(article, "ChoosingTheNextMoveShrunk.txt", false);
        Verify_Shrunk(article);
    }

    private static void Verify_Shrunk(Article article)
    {
        Assert.Equal("No Overdraft", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(2, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Withdraw", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Amount", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("87", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("No Overdraft", article.PassedExpectation(1).Read().Label);
        Assert.Equal(1, article.PassedExpectation(1).Read().TimesPassed);
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

    [Fact]
    [DocHeader("On One Condition")]
    [DocContent(
@$"In many cases you would like to control the possible branches taken, based on some condition.  
For this you can use `{nameof(Checkr)}.{nameof(Checkr.OneOfWhen)}`, which takes an enumerable of `{nameof(Checkr)}.{nameof(Checkr.Option)}` elements.  

**Example:**")]
    [DocExample(typeof(ChoosingTheNextMove), nameof(GetConditionalCheckr))]
    [DocContent("**Results in:**")]
    [DocCodeFile("ChoosingTheNextConditional.txt", "text")]
    [DocHeader("Recap")]
    [DocContent(
@$"`{nameof(Checkr)}.{nameof(Checkr.OneOf)}` and `{nameof(Checkr)}.{nameof(Checkr.OneOfWhen)}` introduces controlled nondeterminism:

* Each execution picks **exactly one** branch.
* All branches operate on the same stashed state.
* Reports show exactly which branch was chosen.")]
    [DocContent("\n[Guide][Guide] | [Previous][TheStashRunsAndExecutions] | [Next][InPoolsWeTrust]")]
    public void RunTest_Conditional()
    {
        var article = Journalist.Investigate(GetConditionalCheckr(), a => a
            .Run(356083796, 2.ExecutionsPerRun()));
        WriteAsserts(article, false);
        WriteReport(article, "ChoosingTheNextConditional.txt", false);
        Verify_Conditional(article);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetConditionalCheckr()
    {
        return
            from everything in Trackr.Stashed(() => new Everything())
            from operations in Checkr.OneOfWhen(
                Checkr.Option(() => everything.TheUniverse == null,
                    from genesis in Checkr.Act("Create the universe",
                        () => everything.TheUniverse = new Universe())
                    select Case.Closed),
                Checkr.Option(() => everything.TheUniverse != null,
                    from falsify in Checkr.Act("Falsify the answer",
                        () => everything.TheUniverse!.Answer++)
                    select Case.Closed)
            )
            from checkTheAnswer in Checkr.ExpectWhen("is 42",
                () => everything.TheUniverse != null,
                () => everything.TheUniverse!.Answer == 42)
            select Case.Closed;
    }

    private static void Verify_Conditional(Article article)
    {
        Assert.Equal("is 42", article.FailureDescription());
        Assert.Equal(2, article.Total().Executions());
        Assert.Equal(2, article.Total().Actions());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Create the universe", article.Execution(1).Action(1).Read().Label);
        Assert.Equal(2, article.Execution(2).Read().ExecutionId);
        Assert.Equal("Falsify the answer", article.Execution(2).Action(1).Read().Label);
        Assert.Equal("is 42", article.PassedExpectation(1).Read().Label);
        Assert.Equal(1, article.PassedExpectation(1).Read().TimesPassed);
    }

    public class Everything { public Universe? TheUniverse; }
    public class Universe { public int Answer = 42; }
}