using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.T_Reducers.A_Simple;

[DocFile]
public class SimpleReducers : CheckrTest<SimpleReducers>
{
    protected override bool Asserts => false;
    protected override bool Report => true;
    protected override bool Explanation => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    [DocContent("")]
    [DocContent("")]
    public override void Example() => Run(1708781424);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Input", Fuzzr.Int(), Towards100)
        from trace in Checkr.Trace("Original", () => input)
        from expect in Checkr.Expect("> 10", () => input > 10)
        select Case.Closed;

    protected override void Verify(Article article)
    {

    }

    [CodeExample]
    private static IEnumerable<int> Towards100(int input)
    {
        var local = input;
        while (local != 100)
        {
            if (local > 100)
                yield return --local;
            else
                yield return ++local;
        }
    }
}
