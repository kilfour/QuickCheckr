using QuickCheckr.Tests.Tools;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.M_HoneyIShrunkTheInputs.B_Collections.TheExamples;

[DocFileHeader("The Better Solution")]
[DocContent(
@"Use the built-in `Shrink.CollectionFromState` shrinker, 
which is a convenience function that hides the plumbing required to define this type of shrinker.")]
public class J3_TheCaseOfTheInvisibleCollection : CheckrTest<J3_TheCaseOfTheInvisibleCollection>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    public override void Example() => Run(169301180, 2.ExecutionsPerRun());

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from thing in Trackr.Stashed(() => new Thing())
        from _ in Checkr.OneOfWhen(
            Checkr.Option(() => !thing.Ints.Contains(1),
                from anInt in Checkr.Input("Int", Fuzzr.Int(1, 10))
                from act in Checkr.Act("Add", () => thing.Ints.Add(anInt))
                select Case.Closed),
            Checkr.Option(() => thing.Ints.Contains(1),
                from ints in Checkr.Input("Ints",
                Fuzzr.Int().Many(2),
                Shrink.CollectionFromState(thing.Ints, [])) // <= Built-in Utility Shrinker
                from act in Checkr.Act("Update", () => thing.Ints = [.. ints])
                from check in Checkr.Expect("Has 1", () => thing.Ints.Contains(1))
                select Case.Closed))
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Has 1", article.FailureDescription());
        Assert.Equal(2, article.Total().Executions());
        Assert.Equal(2, article.Total().Actions());
        Assert.Equal(2, article.Total().Inputs());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Add", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Int", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("1", article.Execution(1).Input(1).Read().Value);
        Assert.Equal(2, article.Execution(2).Read().ExecutionId);
        Assert.Equal("Update", article.Execution(2).Action(1).Read().Label);
        Assert.Equal("Ints", article.Execution(2).Input(1).Read().Label);
        Assert.Equal("[ 57, 70 ]", article.Execution(2).Input(1).Read().Value);
        Assert.Equal("[ ]", article.Execution(2).Input(1).Read().Redux.Value);
    }

    [CodeExample]
    public class Thing { public List<int> Ints = []; }
}