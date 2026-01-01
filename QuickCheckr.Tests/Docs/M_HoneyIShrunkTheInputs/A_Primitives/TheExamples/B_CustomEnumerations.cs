using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.M_HoneyIShrunkTheInputs.A_Primitives.TheExamples;

[DocFile]
public class B_CustomEnumerations : CheckrTest<B_CustomEnumerations>
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
        Run(1735186993);

    enum SoundCheck { One, Two } // [CodeExample] not valid ?

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from inputOne in Checkr.Input("One", Fuzzr.Enum<SoundCheck>())
        from inputTwo in Checkr.Input("Two", Fuzzr.Enum<SoundCheck>())
        from traceOne in Checkr.Trace("Original One", () => inputOne)
        from traceTwo in Checkr.Trace("Original Two", () => inputTwo)
        from expect in Checkr.Expect("Two should not be Two", () => inputTwo != SoundCheck.Two)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Two should not be Two", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(2, article.Total().Traces());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Two", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("Two", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Original One", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("Two", article.Execution(1).Trace(1).Read().Value);
        Assert.Equal("Original Two", article.Execution(1).Trace(2).Read().Label);
        Assert.Equal("Two", article.Execution(1).Trace(2).Read().Value);
    }
}