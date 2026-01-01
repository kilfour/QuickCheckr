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
        Run(1017208542, 10.ExecutionsPerRun());

    enum SoundCheck { One, Two } // [CodeExample] not valid ?

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from inputOne in Checkr.Input("One", Fuzzr.Enum<SoundCheck>())
        from inputTwo in Checkr.Input("Two", Fuzzr.Enum<SoundCheck>())
        from expect in Checkr.Expect("Two should not be Two", () => inputTwo != SoundCheck.Two)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Two should not be Two", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(2, article.ShrinkCount);
        Assert.Equal(2, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Two", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("Two", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Two should not be Two", article.PassedExpectation(1).Read().Label);
        Assert.Equal(1, article.PassedExpectation(1).Read().TimesPassed);
    }
}