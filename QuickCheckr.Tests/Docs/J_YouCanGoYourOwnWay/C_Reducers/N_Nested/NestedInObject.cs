using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.J_YouCanGoYourOwnWay.C_Reducers.N_Nested;

//[DocFile]
public class NestedInObject : CheckrTest<NestedInObject>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    public class TheUniverse
    {
        public int Answer { get; set; }
    }

    [Fact(Skip = "Wip")]
    [DocModelHeader]
    [DocExample(typeof(TheUniverse))]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    [DocContent("")]
    [DocContent("")]
    public override void Example() =>
        Run(1930190827);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from answer in Checkr.Input("Answer", Fuzzr.Int())
        from universe in Checkr.Input("Universe", Fuzzr.Constant(new TheUniverse { Answer = answer }))
        from notTooBig in Checkr.Expect("answer <= 42", () => universe.Answer <= 42)
        select Case.Closed;

    protected override void Verify(Article article)
    {

    }
}
