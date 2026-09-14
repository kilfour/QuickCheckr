using QuickCheckr;
using QuickTestr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Authoring.ThePress;

namespace QuickTestr.Tests.Docs.A_PropertyBased.Sub;

[DocFile]
[DocContent(
"""
Now for something less trivial, one of the `jqwik` challenges.  

The property we are testing states:  
> No two different elements point to each other when used as indexes into the list.
""")]
public class B_MovingOn : QuickTestrPropertyTest<B_MovingOn>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocTestrHeader]
    [DocTestr]
    [DocContent("""
**Some Notes:**
- `DisableValueReduction()`: By default QuickTestr tries to move ints towards zero, but here that isn't necessary.
- `Deliberate(a => a.Count, 2)`: This is a *Deliberation Policy*. It helps escape local minima during shrinking. In this case, we're looking for smaller lists.
- The compositional abilities of QuickFuzzr allow us to generate only valid indexes, keeping inputs structurally sound.
""")]
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
