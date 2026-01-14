using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.M_HoneyIShrunkTheInputs.B_Collections.TheExamples;

[DocFileHeader("The Issue")]
[DocContent(
@"Another case of '*Greedy Shrinker Disease*'.  

This one is caused by the fact that the shrinker only sees the inputs.
It is unaware of any *state derived* meaningfull values. ")]
public class J1_TheCaseOfTheInvisibleCollection : CheckrTest<J1_TheCaseOfTheInvisibleCollection>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocModelHeader]
    [DocExample(typeof(Thing))]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
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
                from ints in Checkr.Input("Ints", Fuzzr.Int().Many(2))
                from act in Checkr.Act("Update", () => thing.Ints = [.. ints])
                from check in Checkr.Expect("Has 1", () => thing.Ints.Contains(1))
                select Case.Closed))
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Has 1", article.FailureDescription());
        Assert.Equal(2, article.Total().Executions());
        Assert.Equal(2, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(2, article.Total().Warnings());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Add", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Int", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("1", article.Execution(1).Input(1).Read().Value);
        Assert.Equal(2, article.Execution(2).Read().ExecutionId);
        Assert.Equal("Update", article.Execution(2).Action(1).Read().Label);
        Assert.Equal("Empty collection deemed irrelevant at input 'Ints'.", article.Execution(2).Warning(1).Read().Value);
        Assert.Equal("All inputs were considered irrelevant.", article.Execution(2).Warning(2).Read().Value);
    }

    [CodeExample]
    public class Thing
    {
        public List<int> Ints = [];
    }
}
