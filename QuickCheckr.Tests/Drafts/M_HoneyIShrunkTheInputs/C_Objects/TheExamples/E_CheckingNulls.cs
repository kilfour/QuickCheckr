using QuickCheckr;
using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Drafts.M_HoneyIShrunkTheInputs.C_Objects.TheExamples;

[DocFile]
public class E_CheckingNulls : BaseTest
{
    private static CheckrConfig ConfigureNoStackTrace(CheckrConfig cfg)
        => cfg with { WarningLevel = WarningLevel.Verbose };

    [Fact]
    [DocContent("**The Model:**")]
    [DocExample(typeof(TheUniverse))]
    [DocContent("**The Checkr:**")]
    [DocExample(typeof(E_CheckingNulls), nameof(GetCheckr))]
    [DocContent("Run with `WarningLevel.Verbose`.")]
    [DocContent("**The Report:**")]
    [DocCodeFile("E_CheckingNulls.txt", "text")]
    [DocContent(
@"Note the shrinking of the `Irrelevant` property.  
In addition, QuickCheckr reports a shrinking warning:  
> Null value for type 'Life' at input 'The Universe.Everything'.  
This tells you that the shrinker reached a `null` value that it cannot safely reduce any further.  
")]
    public void RunTest()
    {
        var article = Journalist.Publish(GetCheckr(), a => a
            .Configure(a => ConfigureNoStackTrace(a) with { })
            .Run(358877731, 20.ExecutionsPerRun()));

        WriteAsserts(article, false);
        WriteReport<E_CheckingNulls>(article, false);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("NullReferenceException: Object reference not set to an instance of an object.", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(1, article.Total().Warnings());
        // Assert.Equal(2, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("The Answer", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("The Universe", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ Everything: null }", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("The Universe", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("{ Everything: null, Irrelevant: { Answer: 78 } }", article.Execution(1).Trace(1).Read().Value);
        Assert.Equal("Null value for type 'Life' at input 'The Universe.Everything'.", article.Execution(1).Warning(1).Read().Value);
    }

    private static readonly FuzzrOf<TheUniverse> TheUniverseFuzzr =
        from everything in Configr<TheUniverse>.Property(
                a => a.Everything, Fuzzr.One<TheUniverse.Life>().NullableRef())
        from irrelevant in Configr<TheUniverse>.Property(
                a => a.Irrelevant, Fuzzr.One<TheUniverse.Life>().NullableRef())
        from one in Fuzzr.One<TheUniverse>()
        select one;

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from universe in Checkr.Input("The Universe", TheUniverseFuzzr)
            from trace in Checkr.Trace("The Universe", () => universe)
            from theAnswer in Checkr.Act("The Answer", () => universe.GetTheAnswer())
            select Case.Closed;
    }

    [Fact]
    [DocContent("But when both `Everything` *and* `Irrelevant` are `null`, QuickCheckr cannot shrink either property, and both limitations are reported as warnings:")]
    [DocCodeFile("E_CheckingNulls_NotShrinkingNull.txt", "text")]
    public void RunTest_NotShrinkingNull()
    {
        var article = Journalist.Publish(GetCheckr(), a => a
            .Configure(a => ConfigureNoStackTrace(a) with { })
            .Run(1269557792));

        WriteAsserts(article, false);
        WriteReport(article, "E_CheckingNulls_NotShrinkingNull.txt", false);
        Verify_NotShrinkingNull(article);
    }

    private static void Verify_NotShrinkingNull(Article article)
    {
        Assert.Equal("NullReferenceException: Object reference not set to an instance of an object.", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(2, article.Total().Warnings());
        // Assert.Equal(0, article.ShrinkCount);
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
    [DocContent("We could help the shrinker by rewriting your checkr, in order to avoid the irrelevant input being reported as relevant.")]
    [DocExample(typeof(E_CheckingNulls), nameof(GetCheckr_Alternative))]
    [DocCodeFile("E_CheckingNulls_Alternative.txt", "text")]
    public void RunTest_Alternative()
    {
        var article = Journalist.Publish(GetCheckr_Alternative(), a => a
            .Configure(a => ConfigureNoStackTrace(a) with { })
            .Run(1269557792));

        WriteAsserts(article, false);
        WriteReport(article, "E_CheckingNulls_Alternative.txt", false);
        Verify_Alternative(article);
    }

    private static void Verify_Alternative(Article article)
    {
        Assert.Equal("NullReferenceException: Object reference not set to an instance of an object.", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        // Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("The Answer", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Everything", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("null", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("The Universe", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("{ Everything: null, Irrelevant: null }", article.Execution(1).Trace(1).Read().Value);
    }

    private static readonly FuzzrOf<TheUniverse.Life?> TheLifeFuzzr =
        Fuzzr.One<TheUniverse.Life>().NullableRef();

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr_Alternative()
    {
        return
            from everything in Checkr.Input("Everything", TheLifeFuzzr,
                Shrink.OnType<TheUniverse.Life?>(s => s.Classify(Try.Fuzzr(TheLifeFuzzr))))
            from irrelevant in Checkr.Input("Irrelevant", TheLifeFuzzr,
                Shrink.OnType<TheUniverse.Life?>(s => s.Classify(Try.Fuzzr(TheLifeFuzzr))))
            from universe in Checkr.Capture(() => new TheUniverse { Everything = everything, Irrelevant = irrelevant })
            from trace in Checkr.Trace("The Universe", () => universe)
            from theAnswer in Checkr.Act("The Answer", () => universe.GetTheAnswer())
            select Case.Closed;
    }

    [Fact(Skip = "REVIEW")]
    [DocContent("Or, we can solve the issue using a `DeliberationPolicy` that limits the number of warnings:")]
    [DocExample(typeof(E_CheckingNulls), nameof(Configure))]
    [DocCodeFile("E_CheckingNulls_Deliberated.txt", "text")]
    [DocContent(
@"As you can see the warning is still there for `Universe.Everything` but this is a relevant input,
so the warning can be ignored.  
Deliberation did succeed in removing the `Universe.Irrelevant` input from the report.")]
    public void RunTest_WithDeliberationPolicy()
    {
        var article = Journalist.Publish(GetCheckr(), a => a
                .Configure(cfg => ConfigureNoStackTrace(cfg with { }))
                .Run(75.ExecutionsPerRun()));

        WriteAsserts(article, false);
        WriteReport(article, "E_CheckingNulls_Deliberated.txt", false);
        Verify_Deliberated(article);
    }

    [CodeExample]
    private static CheckrConfig Configure(CheckrConfig config)
        => config with { Deliberation = Deliberation.MinimumWarnings() };

    private static void Verify_Deliberated(Article article)
    {
        Assert.Equal(1, article.Total().Warnings());
    }

    [CodeExample]
    public class TheUniverse
    {
        public class Life { public int Answer { get; set; } }
        public Life? Everything { get; set; }
        public Life? Irrelevant { get; set; }
        public int GetTheAnswer() => Everything!.Answer;
    }
}