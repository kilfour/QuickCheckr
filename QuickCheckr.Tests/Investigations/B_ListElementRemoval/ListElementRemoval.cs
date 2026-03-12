using QuickCheckr;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Experimental;


[DocFile]
public class ListElementRemoval : CheckrTest<ListElementRemoval>
{
    protected override bool Asserts => true;
    protected override bool Report => true;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Run(100.Runs());

    [CodeSnippet]
    private static readonly FuzzrOf<List<int>> InnerFuzzr =
         from list in Fuzzr.Int().Many(2)
         select list.ToList();

    private static readonly FuzzrOf<List<List<int>>> TheFuzzr =
         from list in InnerFuzzr.Many(2)
         select list.ToList();

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Nested List", TheFuzzr)
        from trace in Checkr.Trace("Original", () => input)
        from expect in Checkr.Expect(
            "The sum of lengths of the element lists is at most 10",
            () => input.Sum(a => a.Count) <= 2)
        select Case.Closed;

    protected override void Verify(Article article)
    {

    }
}