using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Drafts.P_WhenThingsGoWrong.D_WhatHappensWhenAnExceptionIsThrown.B_CheckingTheException;

[DocFile]
public class CheckingTheException : CheckrTest<CheckingTheException>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Document(a => a.Run(412717959));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from result in Checkr.ActCarefully("Act", () => throw new Exception("BOOM"))
        from throws in Checkr.Expect("Throws", () => result.Threw)
        from throwsExact in Checkr.Expect("Throws InvalidOperationException", result.ThrewAs<InvalidOperationException>)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Throws InvalidOperationException", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Act", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Throws", article.PassedExpectation(1).Read().Label);
        Assert.Equal(1, article.PassedExpectation(1).Read().TimesPassed);
    }
}