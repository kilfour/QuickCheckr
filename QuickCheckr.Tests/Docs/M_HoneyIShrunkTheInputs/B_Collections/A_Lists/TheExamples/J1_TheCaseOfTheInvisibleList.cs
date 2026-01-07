using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.M_HoneyIShrunkTheInputs.B_Collections.A_Lists.TheExamples;

[DocFileHeader("The Issue")]
[DocContent(
@"Another case of '*Greedy Shrinker Disease*'.  

This one is caused by the fact that the shrinker only sees the inputs.
It is unaware of any *state derived* meaningfull values. ")]
public class J1_TheCaseOfTheInvisibleList : CheckrTest<J1_TheCaseOfTheInvisibleList>
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
    [DocContent("Shrinker marks the list as irrelevant, but it is **not**.")]
    public override void Example() => Run(1954145022);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from thing in Trackr.Stashed(() => new Thing())
        from ints in Checkr.Input("ints", Fuzzr.Int().Many(2))
        from act in Checkr.Act("change", () => thing.Ints = [.. ints])
        from check in Checkr.Expect("has 1", () => thing.Ints.Contains(1))
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("has 1", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(2, article.Total().Warnings());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("change", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Empty collection deemed irrelevant at input 'ints'.", article.Execution(1).Warning(1).Read().Value);
        Assert.Equal("All inputs were considered irrelevant.", article.Execution(1).Warning(2).Read().Value);
    }

    [CodeExample]
    public class Thing { public List<int> Ints = [1]; }
}
