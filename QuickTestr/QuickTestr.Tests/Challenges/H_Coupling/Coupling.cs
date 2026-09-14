using QuickCheckr;
using QuickTestr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Authoring.ThePress;

namespace QuickTestr.Tests.Challenges.H_Coupling;

[DocFile]
[DocContent(
@"
In this example the elements of a list of integers are coupled to their position in an unusual way.

The expected smallest falsified sample is [1, 0].
")]
public class Coupling : QuickTestrPropertyTest<Coupling>
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
    [CodeRemove("1934478623")]
    protected override void GetTestr(Journalist journalist) =>
        Testr
            .Named("No two different indexes point to each other.")
            .DisableValueReduction()
            .For(
                from length in Fuzzr.Int()
                from enumerable in Fuzzr.Int(0, length - 1).Many(length)
                select enumerable.ToList())
            .Deliberate(a => a.Count, 2)
            .Assert(list => list.All(
                element =>
                {
                    int index = list[element];
                    if (index != element && list[index] == element)
                        return false;
                    return true;
                }))
            .StoreCaseFiles(journalist)
            .Run(1934478623);

    protected override void Verify(Article article)
    {
        Assert.Equal("No two different indexes point to each other.", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(12, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ 1, 0 ]", article.Execution(1).Input(1).Read().Value);
    }
}
