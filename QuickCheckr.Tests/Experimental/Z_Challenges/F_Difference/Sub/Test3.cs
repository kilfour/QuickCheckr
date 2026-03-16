using QuickCheckr;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Experimental.Z_Challenges.F_Difference.Sub;

[DocFile]
[DocContent(
@"
Test 3 ('difference must not be one') only succeeds if 
- the first parameter is less than 10 
- _or_ the difference is not exactly 1.
The smallest falsified sample is `[10, 9]`.
")]
public class Test3 : CheckrTest<Test3>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocBoldHeader("The Reducer")]
    [DocExample(typeof(Test3), nameof(TowardsZero))]
    [DocReportHeader]
    [DocReport]
    [DocContent("This one sometimes produces 10, 11")]
    public override void Example() =>
        Run(1238621164);

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
            "Difference is not exactly 1.",
            () => input.A < 10 || Math.Abs(input.A - input.B) != 1)
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
        Assert.Equal("Difference is not exactly 1.", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ A: 49, B: 48 }", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("{ A: 10, B: 9 }", article.Execution(1).Input(1).Read().Redux.Value);
        Assert.Equal("Original", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("{ A: 49, B: 48 }", article.Execution(1).Trace(1).Read().Value);
    }
}