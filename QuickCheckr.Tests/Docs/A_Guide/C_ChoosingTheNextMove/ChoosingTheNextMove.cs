using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Docs.A_Guide.B_TheStashRunsAndExecutions;
using QuickCheckr.Tests.Docs.A_Guide.D_InPoolsWeTrust;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickPulse.Instruments;


namespace QuickCheckr.Tests.Docs.A_Guide.C_ChoosingTheNextMove;

[DocFile]
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

`OneOf` turns your Checkr into a **state machine explorer**.

Let's keep the model simple:")]
[DocExample(typeof(Account))]
public class ChoosingTheNextMove : CheckrTest<ChoosingTheNextMove>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [CodeExample]
    public class Account
    {
        public int Balance { get; private set; }
        public void Deposit(int amount) => Balance += amount;
        public void WithDraw(int amount)
        {
            if (amount == 42)
                ComputerSays.No("No answers for you.");
            Balance -= amount;
        }
    }

    [Fact]
    [DocHeader("A Checkr With a Choice")]
    [DocExample(typeof(ChoosingTheNextMove), nameof(GetCheckr))]
    [DocContent(
@$"Every execution through this Checkr does the same things:

1. Generate a random amount.
2. Pick exactly **one** action: `Deposit` *or* `Withdraw`.

A key detail:

> **Each branch runs inside the same *Run Scope*.**
> This means both branches mutate the same stashed `account` instance.

So across executions, QuickCheckr builds a sequence of deposits and withdrawals, any of which may lead to a failure.

**Note:**  
> Any exception thrown during execution of a `{nameof(Checkr)}.{nameof(Checkr.Act)}` operation will be caught and considered a failure.
")]
    [DocHeader("Seeing Which Branches Were Chosen")]
    [DocContent("Running the Checkr without shrinking yields a report similar to this:")]
    [DocReport]
    [DocContent(
@"This shows us that **QuickCheckr records the branch actually taken for each execution**.    
There is no guesswork.")]
    [DocInclude(typeof(ChoosingTheNextMoveShrunk))]
    [DocInclude(typeof(ChoosingTheNextMoveConditional))]
    public override void Example() => Run(() => TheCheckr().Run(971854269, 10.ExecutionsPerRun(), Configure));

    private CheckrConfig Configure(CheckrConfig config)
        => config with
        {
            ShrinkMode = ShrinkMode.None,
            ReportMode = config.ReportMode & ~ReportMode.StackTrace
        };


    [CodeSnippet]
    public static CheckrOf<Case> GetCheckr() =>
        from account in Trackr.Stashed(() => new Account())
        from amount in Checkr.Input("Amount", Fuzzr.Int())
        from operations in Checkr.OneOf(
            Checkr.Act("Deposit", () => account.Deposit(amount)),
            Checkr.Act("Withdraw", () => account.WithDraw(amount))
        )
        select Case.Closed;

    protected override CheckrOf<Case> TheCheckr() => GetCheckr();

    protected override void Verify(Article article)
    {
        Assert.Equal("ComputerSaysNo: No answers for you.", article.FailureDescription());
        Assert.Equal(5, article.Total().Executions());
        Assert.Equal(5, article.Total().Actions());
        Assert.Equal(5, article.Total().Inputs());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Withdraw", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Amount", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("55", article.Execution(1).Input(1).Read().Value);
        Assert.Equal(2, article.Execution(2).Read().ExecutionId);
        Assert.Equal("Withdraw", article.Execution(2).Action(1).Read().Label);
        Assert.Equal("Amount", article.Execution(2).Input(1).Read().Label);
        Assert.Equal("30", article.Execution(2).Input(1).Read().Value);
        Assert.Equal(3, article.Execution(3).Read().ExecutionId);
        Assert.Equal("Withdraw", article.Execution(3).Action(1).Read().Label);
        Assert.Equal("Amount", article.Execution(3).Input(1).Read().Label);
        Assert.Equal("79", article.Execution(3).Input(1).Read().Value);
        Assert.Equal(4, article.Execution(4).Read().ExecutionId);
        Assert.Equal("Deposit", article.Execution(4).Action(1).Read().Label);
        Assert.Equal("Amount", article.Execution(4).Input(1).Read().Label);
        Assert.Equal("21", article.Execution(4).Input(1).Read().Value);
        Assert.Equal(5, article.Execution(5).Read().ExecutionId);
        Assert.Equal("Withdraw", article.Execution(5).Action(1).Read().Label);
        Assert.Equal("Amount", article.Execution(5).Input(1).Read().Label);
        Assert.Equal("42", article.Execution(5).Input(1).Read().Value);
    }
}