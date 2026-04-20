using QuickCheckr.Tests.Tools;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.M_HoneyIShrunkTheInputs.A_Primitives.TheExamples;

[DocFile]
public class A_BuiltInEnumerations : CheckrTest<A_BuiltInEnumerations>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Run(2083214969);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from inputOne in Checkr.Input("One", Fuzzr.Enum<DayOfWeek>())
        from inputTwo in Checkr.Input("Two", Fuzzr.Enum<DayOfWeek>())
        from traceOne in Checkr.Trace("Original One", () => inputOne)
        from traceTwo in Checkr.Trace("Original Two", () => inputTwo)
        from expect in Checkr.Expect("Two should not be Monday", () => inputTwo != DayOfWeek.Monday)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Two should not be Monday", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(2, article.Total().Traces());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Two", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("Monday", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Original One", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("Wednesday", article.Execution(1).Trace(1).Read().Value);
        Assert.Equal("Original Two", article.Execution(1).Trace(2).Read().Label);
        Assert.Equal("Monday", article.Execution(1).Trace(2).Read().Value);
    }
}