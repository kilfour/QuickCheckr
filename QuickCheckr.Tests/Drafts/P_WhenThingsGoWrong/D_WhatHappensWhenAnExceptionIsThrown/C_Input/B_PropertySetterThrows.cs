using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Drafts.P_WhenThingsGoWrong.D_WhatHappensWhenAnExceptionIsThrown.C_Input;


[DocFile]
[DocContent("""
During Shrinking Property Setters do not throw.
""")]
public class B_PropertySetterThrows : CheckrTest<B_PropertySetterThrows>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Run(() => TheCheckr().Run(662847995, 1.ExecutionsPerRun()));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("above 0",
            from amount in Fuzzr.Int(1, 100)
            select new Item { Amount = amount })
        from expect in Checkr.Expect("amount < 10", () => input.Amount < 10)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("amount < 10", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("above 0", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ Amount: 77 }", article.Execution(1).Input(1).Read().Value);
    }

    public class Item
    {
        private int amount;
        public int Amount
        {
            get => amount;
            set
            {
                ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
                amount = value;
            }
        }
    }
}