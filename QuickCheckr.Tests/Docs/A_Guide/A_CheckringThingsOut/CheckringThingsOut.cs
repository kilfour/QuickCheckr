using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.UnderTheHood;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.A_Guide.A_CheckringThingsOut;

[DocFile]
[DocFileHeader("Checkr-ing Things Out")]
[DocContent(
@"A *Checkr* describes a sequence of steps that QuickCheckr will execute repeatedly.
Each run goes through the script from top to bottom: generating inputs, performing actions, checking expectations, and collecting diagnostics.
If any execution fails, QuickCheckr shrinks it to the smallest, easiest-to-understand failing case.")]
public class CheckringThingsOut : BaseTest
{
    [Fact]
    [DocHeader("First Expectation")]
    [DocContent("Let's start by having a look at the simplest possible Checkr:")]
    [DocExample(typeof(MinimalCheckr))]
    [DocContent(
@"Congratulations! You've just checked something out. The test passes and now we know that `true` is always `true`.

So what happens if try to do the opposite?")]
    [DocExample(typeof(CheckringThingsOut), nameof(ExpectAlwaysFalse_Checkr))]
    [DocContent("The test fails and QuickCheckr produces the following report:")]
    [DocCodeFile("ExpectAlwaysFalse.txt", "text")]
    [DocContent(
@"
The report shows a run summary containing :
* Test: Name of the test executing the Checkr.
* Location: Source code Location of the test executing the Checkr.
* Original failing run: Number of executions executed during the run.
* The minimal fail-case: In this toyest of examples it *only* specifies that an execution happened.
* Failure details: Why the run failed in the first place.
* Seed: The seed used to generate the failing run.  
  You can use this to reproduce the failing run reliably, by calling run with the seed passed in.")]
    [DocExample(typeof(CheckringThingsOut), nameof(Run_ExpectAlwaysFalse))]
    public void ExpectAlwaysFalse()
    {
        var article = TheJournalist.Exposes(() => Run_ExpectAlwaysFalse(ExpectAlwaysFalse_Checkr()));
        Assert.Equal("Always true", article.FailedExpectation());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        WriteAsserts(article);
        WriteReport(article, "ExpectAlwaysFalse.txt");
    }

    [CodeSnippet]
    [CodeRemove("return ")]
    private static CheckrOf<Case> ExpectAlwaysFalse_Checkr()
    {
        return Checkr.Expect("Always true", () => false);
    }

    [CodeSnippet]
    [CodeRemove("return ")]
    private static void Run_ExpectAlwaysFalse(CheckrOf<Case> checkr)
    {
        checkr.Run(1642083259);
    }

    [Fact]
    [DocHeader("Gathering Input")]
    [DocContent("Let's add an input to the Checkr, and change our expectation:")]
    [DocExample(typeof(CheckringThingsOut), nameof(AddingAnInput_Checkr))]
    [DocContent(
@"
*Note:* `Case.Closed` marks the end of the Checkr script.
A Checkr is just a LINQ expression that evaluates step by step,
and the final select clause must return a value representing the completed case.
`Case.Closed` simply signals ""this case finished successfully, there are no more steps"".

QuickCheckr relies on the [`QuickFuzzr`](https://github.com/kilfour/QuickFuzzr/blob/main/README.md) library to generate inputs.

`Fuzzr.Int()` generates a random integer between 1 and 100, so this test will fail, on average, once every hundred times.  
Not ideal, so let's up the run number, like so:")]
    [DocExample(typeof(CheckringThingsOut), nameof(FailureThrows))]
    [DocContent("Now the test fails consistently:")]
    [DocCodeFile("AddingAnInput.txt", "text")]
    [DocContent(
@"As you can see the input is now included in the report *minimal fail-case*,
this is because QuickCheckr judged the input to be relevant to the failure.")]
    public void AddingAnInput()
    {
        var article = TheJournalist.Exposes(() => AddingAnInput_Checkr().Run(127648209));
        WriteAsserts(article);
        WriteReport(article, "AddingAnInput.txt");
        Assert.Equal("Input /= 42", article.FailedExpectation());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(0, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("42", article.Execution(1).Input(1).Read().Value);
    }

    [Fact]
    [CodeRemove("Assert.Throws<FalsifiableException>(() => ")]
    [CodeReplace("ExpectAlwaysFalse_Checkr()", "check")]
    [CodeRemove(");")]
    [CodeSnippet]
    public void FailureThrows()
    {
        Assert.Throws<FalsifiableException>(() => ExpectAlwaysFalse_Checkr().Run(200.Runs()));
    }

    [CodeSnippet]
    [CodeRemove("return check;")]
    private static CheckrOf<Case> AddingAnInput_Checkr()
    {
        var check =
            from input in Checkr.Input("Input", Fuzzr.Int())
            from expect in Checkr.Expect("Input /= 42", () => input != 42)
            select Case.Closed;
        return check;
    }

    [Fact]
    [DocContent("So, if we add another input to the Checkr, one which does not influence the failure:")]
    [DocExample(typeof(CheckringThingsOut), nameof(AddingAnotherInput_Checkr))]
    [DocContent("The following happens:")]
    [DocCodeFile("AddingAnotherInput.txt", "text")]
    [DocContent("That's basically the same thing as before, because QuickCheckr does not report irrelevant inputs.")]
    public void AddingAnotherInput()
    {
        var article = TheJournalist.Exposes(() =>
            AddingAnotherInput_Checkr().Run(127648209));
        WriteAsserts(article);
        WriteReport(article, "AddingAnotherInput.txt");
        Assert.Equal("Input /= 42", article.FailedExpectation());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("42", article.Execution(1).Input(1).Read().Value);
    }

    [CodeSnippet]
    [CodeRemove("return check;")]
    private static CheckrOf<Case> AddingAnotherInput_Checkr()
    {
        var check =
            from input in Checkr.Input("Input", Fuzzr.Int())
            from irrelevant in Checkr.Input("Irrelevant Input", Fuzzr.Int())
            from expect in Checkr.Expect("Input /= 42", () => input != 42)
            select Case.Closed;
        return check;
    }

    [Fact]
    [DocHeader("Not Without a Trace")]
    [DocContent(
@"QuickCheckr's *shrinking* can sometimes seem magical. Not a problem in this simple test,
but when you are writing a more complex test, it is always helpful to see what's really going on.

QuickCheckr is built with transparency and discoverability as a first class concern:")]
    [DocExample(typeof(CheckringThingsOut), nameof(NotWithoutATrace_Checkr))]
    [DocContent("By adding a `Checkr.Trace` to the `LINQ` chain, we get the following report:")]
    [DocCodeFile("NotWithoutATrace.txt", "text")]
    public void NotWithoutATrace()
    {
        var article = TheJournalist.Exposes(() =>
            NotWithoutATrace_Checkr().Run(769641533));
        WriteAsserts(article);
        WriteReport(article, "NotWithoutATrace.txt");
        Assert.Equal("Input /= 42", article.FailedExpectation());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("42", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Irrelevant Input", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("47", article.Execution(1).Trace(1).Read().Value);
    }

    [CodeSnippet]
    [CodeRemove("return check;")]
    private static CheckrOf<Case> NotWithoutATrace_Checkr()
    {
        var check =
            from input in Checkr.Input("Input", Fuzzr.Int())
            from irrelevant in Checkr.Input("Irrelevant Input", Fuzzr.Int())
            from expect in Checkr.Expect("Input /= 42", () => input != 42)
            from trace in Checkr.Trace("Irrelevant Input", () => irrelevant)
            select Case.Closed;
        return check;
    }

    [Fact]
    [DocHeader("Time to Act!")]
    [DocContent("Testing inputs is fun and all, but really, we want to test some kind of behaviour, right?")]
    [DocExample(typeof(CheckringThingsOut), nameof(TimeToAct_Checkr))]
    [DocContent("Still a trivial example, but it shows how you can use `Checkr.Act` to start mutating things.")]
    [DocContent("And here's the report:")]
    [DocCodeFile("TimeToAct.txt", "text")]
    [DocContent("As demonstrated, the name of the executed action is included in the report, as well as the relevant inputs.")]
    public void TimeToAct()
    {
        var article = TheJournalist.Exposes(() =>
            TimeToAct_Checkr().Run(1642083259));
        WriteAsserts(article);
        WriteReport(article, "TimeToAct.txt");
        Assert.Equal("Sum /= 42", article.FailedExpectation());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(2, article.Total().Inputs());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("One plus Three", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("One", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("24", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Three", article.Execution(1).Input(2).Read().Label);
        Assert.Equal("18", article.Execution(1).Input(2).Read().Value);
    }

    [CodeSnippet]
    [CodeRemove("return check;")]
    private static CheckrOf<Case> TimeToAct_Checkr()
    {
        var check =
            from one in Checkr.Input("One", Fuzzr.Int())
            from two in Checkr.Input("Two", Fuzzr.Int())
            from three in Checkr.Input("Three", Fuzzr.Int())
            from sum in Checkr.Act("One plus Three", () => one + three)
            from expect in Checkr.Expect("Sum /= 42", () => sum != 42)
            select Case.Closed;
        return check;
    }
}
