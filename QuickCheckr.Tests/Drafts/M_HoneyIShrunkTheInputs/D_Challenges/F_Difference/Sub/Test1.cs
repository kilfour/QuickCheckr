using QuickCheckr;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.M_HoneyIShrunkTheInputs.D_Challenges.F_Difference.Sub;

[DocFile]
[DocContent(
@"
Test 1 ('difference must not be zero') only succeeds if 
- the first parameter is less than 10 
- _or_ the difference is not zero.
The smallest falsified sample is `[10, 10]`
")]
public class Test1 : CheckrTest<Test1>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocBoldHeader("The Reducer")]
    [DocExample(typeof(Test1), nameof(TowardsZero))]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Run(1040200141);

    public record Pair(int A, int B);

    private static readonly FuzzrOf<Pair> TheFuzzr =
        from _ in Configr<Pair>.Construct(Fuzzr.Int(), Fuzzr.Int())
        from pair in Fuzzr.One<Pair>()
        select pair;

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from input in Checkr.Input("Input", TheFuzzr,
            Shrink.OnType<Pair>(s => s.Simplify(Reduce.Function<Pair>(a => TowardsZero(a)))))
        from trace in Checkr.Trace("Original", () => input)
        from expect in Checkr.Expect(
            "Difference must not be zero.",
            () => input.A < 10 || input.A != input.B)
        select Case.Closed;

    [CodeExample]
    public static IEnumerable<Pair> TowardsZero(Pair input)
    {
        var local = input;
        while (local.A >= 0)
        {
            local = local with { A = local.A - 1, B = local.B - 1 };
            yield return local;
        }
    }

    protected override void Verify(Article article)
    {
        Assert.Equal("Difference must not be zero.", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ A: 21, B: 21 }", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("{ A: 10, B: 10 }", article.Execution(1).Input(1).Read().Redux.Value);
        Assert.Equal("Original", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("{ A: 21, B: 21 }", article.Execution(1).Trace(1).Read().Value);
    }
}