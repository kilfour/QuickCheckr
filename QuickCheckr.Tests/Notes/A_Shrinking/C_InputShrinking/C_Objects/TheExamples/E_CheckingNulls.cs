using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickCheckr.UnderTheHood.Protocol;
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
    [DocContent(
@"Note the shrinking of the `Irrelevant` property.  
In addition, QuickCheckr reports a shrinking warning:  
> Null value for type 'Life' at input 'The Universe.Everything'.  
This tells you that the shrinker reached a `null` value that it cannot safely reduce any further.  
")]
    [DocCodeFile("E_CheckingNulls.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(358877731));
        WriteAsserts(article, false);
        WriteReport<E_CheckingNulls>(article, false);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(1, article.Total().Warnings());
        Assert.Equal(2, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("The Answer", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("The Universe", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ Everything: null }", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("The Universe", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("{ Everything: null, Irrelevant: { Answer: 78 } }", article.Execution(1).Trace(1).Read().Value);
        Assert.Equal("Null value for type 'Life' at input 'The Universe.Everything'.", article.Execution(1).Warning(1).Read().Value);

    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from universe in Checkr.Input("The Universe",
                from everything in Configr<TheUniverse>.Property(a => a.Everything, Fuzzr.One<Life>().NullableRef())
                from irrelevant in Configr<TheUniverse>.Property(a => a.Irrelevant, Fuzzr.One<Life>().NullableRef())
                from one in Fuzzr.One<TheUniverse>()
                select one)
            from trace in Checkr.Trace("The Universe", () => universe)
            from theAnswer in Checkr.Act("The Answer", () => universe.GetTheAnswer())
                // No Expect == does not throw, implicitely
            select Case.Closed;
    }

    [Fact]
    [DocContent("But when both `Everything` *and* `Irrelevant` are `null`, QuickCheckr cannot shrink either property, and both limitations are reported as warnings:")]
    [DocCodeFile("E_CheckingNulls_NotShrinkingNull.txt", "text")]
    public void RunTest_NotShrinkingNull()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(1269557792));
        WriteAsserts(article, false);
        WriteReport(article, "E_CheckingNulls_NotShrinkingNull.txt", false);
        Verify_NotShrinkingNull(article);
    }

    private static void Verify_NotShrinkingNull(Article article)
    {
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(2, article.Total().Warnings());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("The Answer", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("The Universe", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ Everything: null, Irrelevant: null }", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("The Universe", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("{ Everything: null, Irrelevant: null }", article.Execution(1).Trace(1).Read().Value);
        Assert.Equal("Null value for type 'Life' at input 'The Universe.Everything'.", article.Execution(1).Warning(1).Read().Value);
        Assert.Equal("Null value for type 'Life' at input 'The Universe.Irrelevant'.", article.Execution(1).Warning(2).Read().Value);
    }

    [Fact]
    [DocContent("The current workaround to avoid the irrelevant input being reported as relevant is to help the shrinker by rewriting your checkr.")]
    [DocExample(typeof(E_CheckingNulls), nameof(GetCheckr_Alternative))]
    [DocCodeFile("E_CheckingNulls_Alternative.txt", "text")]
    public void RunTest_Alternative()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr_Alternative().Run(1269557792));
        WriteAsserts(article);
        WriteReport(article, "E_CheckingNulls_Alternative.txt", false);
        Verify_Alternative(article);
    }

    private static void Verify_Alternative(Article article)
    {
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("The Answer", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Everything", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("null", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("The Universe", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("{ Everything: null, Irrelevant: null }", article.Execution(1).Trace(1).Read().Value);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr_Alternative()
    {
        return
            from everything in Checkr.Input("Everything", Fuzzr.One<Life>().NullableRef(), a => a with { FuzzrShrinker = 10 })
            from irrelevant in Checkr.Input("Irrelevant", Fuzzr.One<Life>().NullableRef(), a => a with { FuzzrShrinker = 10 })
            from universe in Checkr.Capture(() => new TheUniverse { Everything = everything, Irrelevant = irrelevant })
            from trace in Checkr.Trace("The Universe", () => universe)
            from theAnswer in Checkr.Act("The Answer", () => universe.GetTheAnswer())
            select Case.Closed;
    }

    [Fact]
    [DocContent("Or, you can solve the issue using a `DeliberationPolicy` that limits the number of warnings:")]
    [DocExample(typeof(E_CheckingNulls), nameof(Configure))]
    [DocCodeFile("E_CheckingNulls_Deliberated.txt", "text")]
    [DocContent(
@"As you can see the warning is still there for `Universe.Everything` but this is a relevant input,
so the warning can be ignored.  
Deliberation did succeed in removing the `Universe.Irrelevant` input from the report.")]
    public void RunTest_WithDeliberationPolicy()
    {
        var article = TheJournalist.Exposes(() => GetCheckr().Run(50.Runs(), Configure));
        WriteAsserts(article, false);
        WriteReport(article, "E_CheckingNulls_Deliberated.txt", false);
        Verify_Deliberated(article);
    }

    [CodeExample]
    private static CheckrConfig Configure(CheckrConfig config)
        => config with { DeliberationPolicy = DeliberationPolicy.MinimumWarnings };

    private static void Verify_Deliberated(Article article)
    {
        Assert.Equal(1, article.Total().Warnings());
    }

    public class TheUniverse
    {
        public Life? Everything { get; set; }
        public Life? Irrelevant { get; set; }
        public int GetTheAnswer() => Everything!.Answer;
    }

    public class Life { public int Answer { get; set; } }
}