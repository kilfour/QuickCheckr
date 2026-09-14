using QuickCheckr;
using QuickTestr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Authoring.ThePress;

namespace QuickTestr.Tests.Challenges.G_BinHeap;

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
public class BinHeap : QuickTestrPropertyTest<BinHeap>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocTestrHeader]
    [DocTestr]
    [DocFuzzrHeader]
    [DocExample(typeof(BinHeap), nameof(TheFuzzr))]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Document();

    [CodeSnippet]
    [CodeRemoveJournalist]
    [CodeRemove("2136249593")]
    protected override void GetTestr(Journalist journalist) =>
        Testr.Named("Heap sort is correct.")
            .For(TheFuzzr)
            .Deliberate(a => Flatten(a).Count())
            .Assert(a =>
            {
                var correct = Flatten(a).OrderBy(x => x).ToList();
                var buggy = Flatten(a).ToList();
                return correct.SequenceEqual(buggy);
            })
            .StoreCaseFiles(journalist)
            .Run(2136249593);

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

    protected override void Verify(Article article)
    {
        Assert.Equal("Heap sort is correct.", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        // Assert.Equal(2, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Run", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ Left: { Value: 999, Left: { }, Right: { } }, Right: { Value: 998, Left: { }, Right: { } } }", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("{ Left: { Value: 999, Left: { }, Right: { } }, Right: { Value: 0, Left: { }, Right: { } } }", article.Execution(1).Input(1).Read().Redux.Value);
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
