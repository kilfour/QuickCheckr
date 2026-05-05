using QuickCheckr;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.M_HoneyIShrunkTheInputs.D_Challenges.G_BinHeap;

[DocFile]
[DocContent(
@"
This is based on an example from QuickCheck's test suite (via the SmartCheck paper). 
It generates binary heaps, and then uses a wrong implementation of a function 
that converts the binary heap to a sorted list and asserts that the result is sorted.

Interestingly most libraries seem to never find the smallest example here, 
which is the four valued heap (0, None, (0, (0, None, None), (1, None, None))). 
This is essentially because small examples are ""too sparse"", so it's very hard to find one by luck.
")]
public class BinHeap : CheckrTest<BinHeap>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocFuzzrHeader]
    [DocExample(typeof(BinHeap), nameof(TheFuzzr))]
    [DocConfigureHeader]
    [DocConfigure]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Run(195567586);

    [CodeSnippet]
    protected override CheckrConfig Configure(CheckrConfig config) =>
        config with
        {
            DeliberationPolicy = caseFile =>
               (caseFile.InputCount == 0 ? 100 : 0) +
               caseFile.InputsNamed<Heap>("Heap", a => Flatten(a).Count()),
            ReportMode = config.ReportMode & ~ReportMode.Warning
        };

    [CodeSnippet]
    private static readonly FuzzrOf<Heap> TheFuzzr =
        from depth in Configr<Heap>.Depth(3, 20)
        from inheritance in Configr<Heap>.AsOneOf(typeof(Empty), typeof(Node))
        from terminator in Configr<Heap>.EndOn<Empty>()
        from ctor in Configr<Node>.Construct(Fuzzr.Int(), Fuzzr.One<Heap>(), Fuzzr.One<Heap>())
        from value in Configr<Node>.Property(a => a.Value,
            from cnt in Fuzzr.Counter("heap") select 1000 - cnt)
        from heap in Fuzzr.One<Heap>()
        select heap;

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Heap", TheFuzzr)
        from correct in Checkr.Act("Correct", () => Flatten(input).OrderBy(x => x).ToList())
        from buggy in Checkr.Act("Buggy", () => Flatten(input).ToList())
        from expect in Checkr.Expect("Heap sort is correct", () => correct.SequenceEqual(buggy))
        from trace in Checkr.Trace("Original", () => input)
        select Case.Closed;

    protected override void Verify(Article article)
    {
    }

    public abstract record Heap;
    public record Empty : Heap;
    public record Node(int Value, Heap Left, Heap Right) : Heap;

    static IEnumerable<int> Flatten(Heap heap)
    {
        return heap switch
        {
            Empty => [],
            Node n => new[] { n.Value }
                .Concat(Flatten(n.Left))
                .Concat(Flatten(n.Right)),
            _ => throw new ArgumentOutOfRangeException(nameof(heap))
        };
    }
}
