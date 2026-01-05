using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.H_OnShrinking.D_InputReduction;

[DocFile]
[DocContent(
@"After InputShrinking has classified all inputs as either relevant or irrelevant, any registered *reducers*
are run on the remaining relevant inputs (example below).  

Note the use of the word *registered*. Staying true to it's shrinking philosophy,
QuickChecker does not try and guess how to reduce input values. It's up to the user to supply that information.  

Most of the time, even in the example here I think, reducers are not needed to figure out what went wrong from the report.  
But in some cases it is a must.  

So what is a reducer ?  

Well let me show you.  
Using the `Account` example from the guide:")]
public class InputReduction : CheckrTest<InputReduction>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explanation => false;

    [Fact]
    [DocModelHeader]
    [DocExample(typeof(Account))]
    [DocCheckrHeader]
    [DocContent(
@"We're running the same checkr, with the same seed.
The only difference is the addition of `Reducr.TowardsZero` in the `Checkr.Input` argument list.")]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    [DocContent(
@"And as you can see it does exactly what it says on the tin. 
It *reduces* the original value using a user supplied algorithm.  
This one just decrements the input until (and including) it reaches zero, and checks if the run still fails.
If the run passes, the value last tried is reported as the *reduced* input.  

In the example here, `0` was tried, but it made the test pass, so the previously tried candidate `1` is reported.  

QuickCheckr comes with a small number of built-in reducers, but it is relatively easy to implement your own.  
Depending on demand, the library of reducers might grow fast.

TODO: LINK TO REDUCE REUSE RECYCLE
")]
    public override void Example() => Run(1684147193, 10.ExecutionsPerRun());

    [CodeExample]
    public class Account
    {
        public int Balance { get; private set; }
        public void Deposit(int amount) => Balance += amount;
        public void WithDraw(int amount) => Balance -= amount;
    }

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from account in Trackr.Stashed(() => new Account())
        from amount in Checkr.Input("Amount", Fuzzr.Int(), Reduce.TowardsZero) // <=
        from operations in Checkr.OneOf(
            Checkr.Act("Deposit", () => account.Deposit(amount)),
            Checkr.Act("Withdraw", () => account.WithDraw(amount)))
        from noOverdraft in Checkr.Expect("No Overdraft", () => account.Balance >= 0)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("No Overdraft", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(2, article.ShrinkCount);
        Assert.Equal(2, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Withdraw", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Amount", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("87", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("1", article.Execution(1).Input(1).Read().Redux.Value);
        Assert.Equal("No Overdraft", article.PassedExpectation(1).Read().Label);
        Assert.Equal(1, article.PassedExpectation(1).Read().TimesPassed);
    }
}
