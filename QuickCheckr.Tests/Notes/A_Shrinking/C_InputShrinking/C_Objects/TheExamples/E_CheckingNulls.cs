using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.C_Objects.TheExamples;

[DocFile]
public class E_CheckingNulls : BaseTest
{
    [Fact]
    [DocHeader("Catching a Null Reference Exception")]
    [DocContent("Any exception thrown by the Checkr will be caught and considered a failure.")]
    [DocExample(typeof(E_CheckingNulls), nameof(GetCheckr))]
    [DocContent("Note the shrinking of the `Irrelevant` property.")]
    [DocCodeFile("E_CheckingNulls.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(a => a with { Seed = 358877731 }));
        WriteAsserts(article);
        WriteReport<E_CheckingNulls>(article);
        Verify(article);
        ExplainMe<E_CheckingNulls>(true);
    }

    private static void Verify(Article article)
    {
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("The Answer", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("The Universe", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ Everything : null }", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("The Universe", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("{ Everything: null, Irrelevant: { Answer: 78 } }", article.Execution(1).Trace(1).Read().Value);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            // TODO make NullableRef the default for object? props in QuickFuzzr
            from universe in Checkr.Input("The Universe",
                from everything in Configr<TheUniverse>.Property(a => a.Everything, Fuzzr.One<Life>().NullableRef())
                from irrelevant in Configr<TheUniverse>.Property(a => a.Irrelevant, Fuzzr.One<Life>().NullableRef())
                from one in Fuzzr.One<TheUniverse>()
                select one)
            from trace in Checkr.Trace("The Universe", () => universe)
            from theAnswer in Checkr.Act("The Answer", () => universe.GetTheAnswer())
                // No Spec == does not throw, implicitely
            select Case.Closed;
    }

    [Fact]
    [DocContent("But, when `Irrelevant` is `null`, QuickCheckr can not shrink.")]
    [DocCodeFile("E_CheckingNulls_NotShrinkingNull.txt", "text")]
    public void RunTest_NotShrinkingNull()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(a => a with { Seed = 1269557792 }));
        WriteAsserts(article);
        WriteReport(article, "E_CheckingNulls_NotShrinkingNull.txt", true);
        Verify_NotShrinkingNull(article);
    }

    private static void Verify_NotShrinkingNull(Article article)
    {
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("The Answer", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("The Universe", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ Everything : null, Irrelevant : null }", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("The Universe", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("{ Everything: null, Irrelevant: null }", article.Execution(1).Trace(1).Read().Value);
    }

    public class TheUniverse
    {
        public Life? Everything { get; set; }
        public Life? Irrelevant { get; set; }
        public int GetTheAnswer() => Everything!.Answer;
    }

    public class Life { public int Answer { get; set; } }
}