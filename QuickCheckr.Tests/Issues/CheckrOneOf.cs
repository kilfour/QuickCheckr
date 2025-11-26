using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Arteries;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Reference;

[DocFile]
[DocFileHeader("Multiple OneOf")]
[DocContent(
@"They end up using the same `key` for `StoreVariable` and thus the second one reads the result of the first one,
instead of writing to short term memory itself. ")]
public class CheckrOneOf : BaseTest
{
    [Fact]
    [DocExample(typeof(CheckrOneOf), nameof(GetCheckr))]
    [DocCodeFile("CheckrOneOf.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(10.ExecutionsPerRun(), a => a with { Seed = 1794561593 }));
        WriteAsserts(article);
        WriteReport<CheckrOneOf>(article);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("Counter fails on 10", article.FailedSpec());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(2, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(2, article.Total().Traces());
        Assert.Equal(1, article.Total().PassedSpecs());
        Assert.Equal(10, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Ops 2, Add 1", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Ops1, Add 1", article.Execution(1).Action(2).Read().Label);
        Assert.Equal("counter", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("10", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("1", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("{ Values: [ 2, 2, 1, 1, 2, 2, 2, 2, 2, 1 ] }", article.Execution(1).Trace(1).Read().Value);
        Assert.Equal("2", article.Execution(1).Trace(2).Read().Label);
        Assert.Equal("{ Values: [ 2, 2, 1, 1, 2, 2, 2, 2, 2, 1 ] }", article.Execution(1).Trace(2).Read().Value);
        Assert.Equal("Counter fails on 10", article.PassedSpec(1).Read().Label);
        Assert.Equal(9, article.PassedSpec(1).Read().TimesPassed);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
        from collector1 in Trackr.Stashed("1", () => Collect.ValuesOf<int>())
        from collector2 in Trackr.Stashed("2", () => Collect.ValuesOf<int>())
        from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
        from ops1 in Checkr.OneOf(
            Checkr.Act("Ops1, Add 1", () => collector1.Absorb(1)),
            Checkr.Act("Ops1, Add 2", () => collector1.Absorb(2))
        )
        from ops2 in Checkr.OneOf(
            Checkr.Act("Ops 2, Add 1", () => collector2.Absorb(1)),
            Checkr.Act("Ops 2, Add 2", () => collector2.Absorb(2))
        )
        from trace1 in Checkr.Trace("1", () => collector1)
        from trace2 in Checkr.Trace("2", () => collector2)
        from spec in Checkr.Spec("Counter fails on 10", () => counter != 10)
        select Case.Closed;
    }
}