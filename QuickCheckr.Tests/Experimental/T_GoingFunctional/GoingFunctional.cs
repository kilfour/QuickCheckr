using QuickCheckr.Invariants;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Experimental.T_GoingFunctional;

[DocFile]
[DocHeader("Spike")]
public class GoingFunctional : InvariantsTest<GoingFunctional>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocModelHeader]
    [DocExample(typeof(TheReverser))]
    [DocInvariantHeader]
    [DocInvariant]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Run();

    [CodeSnippet]
    protected override void TheInvariant() =>
        Check.ForAll(
            from len in Fuzzr.Int(0, 10)
            from elements in Fuzzr.Int(0, 10).Many(len)
            select elements.ToList(),
            list => list.SequenceEqual(TheReverser.Reverses(TheReverser.Reverses(list))));

    [CodeExample]
    public class TheReverser
    {
        public static List<int> Reverses(List<int> list)
            => [.. FudgeIt(list.AsEnumerable()).Reverse()];

        private static IEnumerable<int> FudgeIt(IEnumerable<int> list)
            => list.Where(a => a != 6).Concat(list.Where(a => a == 6));
    }

    protected override void Verify(Article article)
    {
        Assert.Equal("Property", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Contains(" 6 ", article.Execution(1).Input(1).Read().Value.ToString());
    }
}