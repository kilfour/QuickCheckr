using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Notes.X_Various.F_ParentChildInvariant;

[DocFile]
public class ParentChildInvariant : CheckrTest<ParentChildInvariant>
{
    protected override bool Asserts => false;
    protected override bool Report => false;


    [Fact]
    [DocModelHeader]
    [DocExample(typeof(Account))]
    [DocCheckr]
    [DocReport]
    public override void Example() =>
        Document(a => a.Run(483645165));

    [CodeExample]
    record Account(int Balance, int CreditLimit);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from account in Checkr.Input("pair", Fuzzr.Constant(new Account(-100, 50)))
        from expect in Checkr.Expect("Balance >= -CreditLimit (i.e. never exceeds credit)",
             () => account.Balance >= -account.CreditLimit)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Balance >= -CreditLimit (i.e. never exceeds credit)", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        // Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("pair", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ Balance: -100, CreditLimit: 50 }", article.Execution(1).Input(1).Read().Value);
    }
}