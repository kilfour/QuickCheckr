using QuickTestr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Authoring.ThePress;

namespace QuickTestr.Tests.Challenges.J_Distinct;

[DocFile]
[DocContent(
@"
This tests the example provided for the property ""a list of integers containing at least three distinct elements"".

This is interesting because:

1. Most property-based testing libraries will not successfully normalize (i.e. always return the same answer) this property,
because it requires reordering examples to do so.
2. Hypothesis and test.check both provide a built in generator for ""a list of distinct elements"",
so the ""example of size at least N"" provides a sort of lower bound for how well they can shrink those built in generators.

The expected smallest falsified sample is `[0, 1, -1]` or `[0, 1, 2]`.
")]
public class Distinct : QuickTestrPropertyTest<Distinct>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocTestrHeader]
    [DocTestr]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Document();

    [CodeSnippet]
    [CodeRemoveJournalist]
    [CodeRemove("1943621438")]
    protected override void GetTestr(Journalist journalist) =>
        Testr
            .Named("Contains at least three distinct elements.")
            .For(Fuzzr.Int(1, 10).Many(3, 20))
            .Assert(a =>
            {
                if (a.Count() >= 3)
                    return a.ToHashSet().Count >= 3;
                return true;
            })
            .StoreCaseFiles(journalist)
            .Run(1943621438);

    protected override void Verify(Article article)
    {
        Assert.Equal("Contains at least three distinct elements.", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(3, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Run", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ 3, _, 3 ]", article.Execution(1).Input(1).Read().Value);
    }
}
