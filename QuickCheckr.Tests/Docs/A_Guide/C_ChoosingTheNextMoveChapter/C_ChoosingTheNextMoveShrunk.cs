using QuickCheckr;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickPulse.Instruments;


namespace QuickCheckr.Tests.Docs.A_Guide.C_ChoosingTheNextMoveChapter;

[DocFileHeader("Shrinking Branching Behaviour")]
public class C_ChoosingTheNextMoveShrunk : CheckrTest<C_ChoosingTheNextMoveShrunk>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocContent("And if we let the QuickCheckr shrinker do its work:")]
    [DocReport]
    [DocContent(
@"The shrunk case shows:

* Only one execution was *relevant*.
* The relevant action was a `Withdraw`.
")]
    public override void Example() => Document(a => a.Run(971854269, 10.ExecutionsPerRun()));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from account in Trackr.Stashed(() => new Account())
        from amount in Checkr.Input("Amount", Fuzzr.Int())
        from operations in Checkr.OneOf(
            Checkr.Act("Deposit", () => account.Deposit(amount)),
            Checkr.Act("Withdraw", () => account.WithDraw(amount))
        )
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("ComputerSaysNo: No answers for you.", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(4, article.ShrinkCount);
        Assert.Equal(5, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Withdraw", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Amount", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("42", article.Execution(1).Input(1).Read().Value);
    }

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
}


