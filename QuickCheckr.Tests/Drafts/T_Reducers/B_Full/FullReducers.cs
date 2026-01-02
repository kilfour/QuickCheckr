using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.T_Reducers.B_Full;

[DocFile]
public class FullReducers : CheckrTest<FullReducers>
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
    public override void Example() =>
        Run(2042459799);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from text in Checkr.Input("Text", Fuzzr.String())
        from trace in Checkr.Trace("Original", () => text)
        from expect in Checkr.Expect("no ab", () => !text.Contains("ab"))
        select Case.Closed;

    protected override void Verify(Article article)
    {

    }

    // private static string Reducer(bool passed, string current)
    // {
    //     return "";
    // }

    // private static Func<bool, object, object> Simplify<T>(Func<T, IEnumerable<T>> reducer)
    // {

    // }
}
