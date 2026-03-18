using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.D_CustomShrinkers.B_Reduction.C_ReducersOld.N_Nested;

// [DocFile]
public class NestedReducerInObjectOnPath : CheckrTest<NestedReducerInObjectOnPath>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [CodeExample]
    public class TheUniverse
    {
        public int AnswerOne { get; set; }
        public int AnswerTwo { get; set; }
        public int AnswerThree { get; set; }
    }

    [Fact(Skip = "TODO")]
    [DocModelHeader]
    [DocExample(typeof(TheUniverse))]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Run(1118829238);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from universe in Checkr.Input("Universe", Fuzzr.One<TheUniverse>())//,
            //[("Universe.AnswerOne", Reduce.TowardsZero), ("Universe.AnswerTwo", Reduce.Towards(100))])
        from trace in Checkr.Trace("Universe", () => universe)
        from justRight in Checkr.Expect("just right", () => universe.AnswerOne <= 42 || universe.AnswerTwo >= 42)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("just right", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(3, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Universe", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ AnswerOne: 91, AnswerTwo: 15 }", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("{ AnswerOne: 43, AnswerTwo: 41 }", article.Execution(1).Input(1).Read().Redux.Value);
        Assert.Equal("Universe", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("{ AnswerOne: 91, AnswerTwo: 15, AnswerThree: 52 }", article.Execution(1).Trace(1).Read().Value);
    }
}