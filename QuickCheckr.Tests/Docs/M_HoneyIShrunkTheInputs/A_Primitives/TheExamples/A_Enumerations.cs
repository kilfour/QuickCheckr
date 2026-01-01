using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.M_HoneyIShrunkTheInputs.A_Primitives.TheExamples;

[DocFile]
public class A_Enumerations : CheckrTest<A_Enumerations>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explanation => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Run(1017208542, 10.ExecutionsPerRun());

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from inputOne in Checkr.Input("One", Fuzzr.Enum<DayOfWeek>())
        from inputTwo in Checkr.Input("Two", Fuzzr.Enum<DayOfWeek>())
        from expect in Checkr.Expect("Two should not be Monday", () => inputTwo != DayOfWeek.Monday)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Two should not be Monday", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(10, article.ShrinkCount);
        Assert.Equal(10, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Two", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("Monday", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Two should not be Monday", article.PassedExpectation(1).Read().Label);
        Assert.Equal(9, article.PassedExpectation(1).Read().TimesPassed);
    }
}