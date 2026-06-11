using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickPulse.Show;

namespace QuickCheckr.Tests.Drafts.M_HoneyIShrunkTheInputs.B_Collections.TheExamples;

[DocFile]
public class K_ACollectionOfObjects : CheckrTest<K_ACollectionOfObjects>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocModelHeader]
    [DocExample(typeof(Thing))]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport] public override void Example() => Document(a => a.Run(849984174));

    [CodeExample]
    public class Thing
    {
        public int One { get; set; }
        public int Two { get; set; }
    }

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from list in Checkr.Input("List", Fuzzr.One<Thing>().Many(2, 5))
        from show in Showr.ForTrace(ListShowr)
        from trace in Checkr.Trace("Original", () => list)
        from expect in Checkr.ExpectWhen("Only answers for One",
            () => list.Any(), () => list.All(a => a.One != 42))
        select Case.Closed;

    private static QuickPulse.Show.Bolts.ManOfWealthAndTaste ListShowr()
        => Please.AllowMe()
            .ToPrettyPrint()
            .ToInline<Thing>();

    protected override void Verify(Article article)
    {
        Assert.Equal("Only answers for One", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        // Assert.Equal(5, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("List", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ { One: 42 } ]", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Original", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal(@"[
    { One: 13, Two: 49 },
    { One: 61, Two: 79 },
    { One: 93, Two: 91 },
    { One: 18, Two: 39 },
    { One: 42, Two: 54 }
]", article.Execution(1).Trace(1).Read().Value);
    }
}
