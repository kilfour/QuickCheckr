using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Drafts.G_CustomShrinkers;

[DocFile]
public class CustomShrinkers : BaseTest
{
    [Fact]
    [DocExample(typeof(CustomShrinkers), nameof(GetCheckr))]
    [DocCodeFile("CustomShrinkers.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(2.ExecutionsPerRun()));
        WriteAsserts(article, false);
        WriteReport<CustomShrinkers>(article, false);
        Verify(article);
    }


    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from count in Checkr.Input("Counter", Fuzzr.Counter("count"))
            // One: Uses the default shrinker.
            from thingOne in Checkr.Input("Thing One", Fuzzr.Constant<Thing>(null!))
                // Two: Uses a Fuzzr for shrinking. 
                // The second argument is the number of values Fuzzr should produce.
                // The default number is 10.
            from shrinkThingTwo in Shrinkr.For(Fuzzr.One<Thing>(), 2)
            from thingTwo in Checkr.Input("Thing Two", Fuzzr.Constant<Thing>(null!))
                // Three: Puts back the default shrinker
            from shrinkThingThree in Shrinkr.For<Thing>(Shrink.Default<Thing>())
            from thingThree in Checkr.Input("Thing Three", Fuzzr.Constant<Thing>(null!))
                // Four: Uses hardcoded values for shrinking. 
            from shrinkThingFour in Shrinkr.For(new Thing(), new Thing())
            from thingFour in Checkr.Input("Thing Four", Fuzzr.Constant<Thing>(null!))
                // Five: Uses a factory function for shrinking. 
                // The lamda argument is the original Input value
            from shrinkThingFive in Shrinkr.For((Thing a) => [new Thing()])
            from thingFive in Checkr.Input("Thing Five", Fuzzr.Constant<Thing>(null!))
                // Six: Uses multiple shrinkers. 
                // The second one will remove this value from the report.
                // The first one will still emit a warning.
            from shrinkThingSix in Shrinkr.For(Shrink.Default<Thing>(), Shrink.Values(new Thing()))
            from thingSix in Checkr.Input("Thing Six", Fuzzr.Constant<Thing>(null!))
                // Seven: Multiples again. 
                // Now the first one will shortcircuit shrinking, so no warning from the second one.
            from shrinkThingSeven in Shrinkr.For(Shrink.Values(new Thing()), Shrink.Default<Thing>())
            from thingSeven in Checkr.Input("Thing Seven", Fuzzr.Constant<Thing>(null!))
            from expect in Checkr.Expect("count != 2", () => count != 2)
            select Case.Closed;
    }

    private static void Verify(Article article)
    {
        Assert.Equal("count != 2", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(3, article.Total().Inputs());
        Assert.Equal(3, article.Total().Warnings());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(6, article.ShrinkCount);
        Assert.Equal(2, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Counter", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("2", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Thing One", article.Execution(1).Input(2).Read().Label);
        Assert.Equal("null", article.Execution(1).Input(2).Read().Value);
        Assert.Equal("Thing Three", article.Execution(1).Input(3).Read().Label);
        Assert.Equal("null", article.Execution(1).Input(3).Read().Value);
        Assert.Equal("Null value for type 'Thing' at input 'Thing One'.", article.Execution(1).Warning(1).Read().Value);
        Assert.Equal("Null value for type 'Thing' at input 'Thing Three'.", article.Execution(1).Warning(2).Read().Value);
        Assert.Equal("Null value for type 'Thing' at input 'Thing Six'.", article.Execution(1).Warning(3).Read().Value);
        Assert.Equal("count != 2", article.PassedExpectation(1).Read().Label);
        Assert.Equal(1, article.PassedExpectation(1).Read().TimesPassed);
    }

    public class Thing;

    [CodeExample]
    public class TheUniverse
    {
        public class Life { public int Answer { get; set; } }
        public Life? Everything { get; set; }
        public Life? Irrelevant { get; set; }
        public int GetTheAnswer() => Everything!.Answer;
    }

    [Fact]
    [DocHeader("Nested", 1)]
    [DocExample(typeof(TheUniverse))]
    [DocContent(
@"The following example, if ran without the Shrinkr, would not be able to shrink the Null values and would also emit warnings.")]
    [DocExample(typeof(CustomShrinkers), nameof(GetCheckrNested))]
    [DocCodeFile("CustomShrinkersNested.txt", "text")]
    public void Nested()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckrNested().Run(1269557792));
        WriteAsserts(article, false);
        WriteReport(article, "CustomShrinkersNested.txt", false);
        VerifyNested(article);
    }
    private static readonly FuzzrOf<TheUniverse.Life?> TheLifeFuzzr =
        Fuzzr.One<TheUniverse.Life>().NullableRef();

    private static readonly FuzzrOf<TheUniverse> TheUniverseFuzzr =
        from everything in Configr<TheUniverse>.Property(a => a.Everything, TheLifeFuzzr)
        from irrelevant in Configr<TheUniverse>.Property(a => a.Irrelevant, TheLifeFuzzr)
        from one in Fuzzr.One<TheUniverse>()
        select one;

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckrNested()
    {
        return
            from shrink in Shrinkr.For(TheLifeFuzzr)
            from universe in Checkr.Input("The Universe", TheUniverseFuzzr)
            from trace in Checkr.Trace("The Universe", () => universe)
            from theAnswer in Checkr.Act("The Answer", () => universe.GetTheAnswer())
            select Case.Closed;
    }

    private static void VerifyNested(Article article)
    {
        Assert.Equal("NullReferenceException: Object reference not set to an instance of an object.", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("The Answer", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("The Universe", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ Everything: null }", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("The Universe", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("{ Everything: null, Irrelevant: null }", article.Execution(1).Trace(1).Read().Value);
    }
}