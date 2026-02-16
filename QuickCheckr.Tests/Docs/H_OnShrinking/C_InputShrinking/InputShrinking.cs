using QuickCheckr.Tests.Docs.H_OnShrinking.A_ExecutionShrinking;
using QuickCheckr.Tests.Docs.H_OnShrinking.D_InputReduction;
using QuickCheckr.Tests.Docs.M_HoneyIShrunkTheInputs;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.H_OnShrinking.C_InputShrinking;

[DocFile]
[DocLink(typeof(HoneyIShrunkTheInputs))]
[DocLink(typeof(OnShrinking))]
[DocLink(typeof(ExecutionShrinking))]
[DocLink(typeof(InputReduction))]
[DocContent("[On Shrinking][OnShrinking] | [Previous][ExecutionShrinking] | [Next][InputReduction]  \n")]
[DocContent(
@"This is the next step in QuickCheckr shrinking and the most complex one.  
QuickCheckr shrinking tries to remove inputs, one by one, to find a minimal failing case.")]
public class InputShrinking : BaseTest
{
    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> Example_Check()
    {
        return
            from irrelevant in Checkr.Input("Irrelevant Input", Fuzzr.Int())
            from expect in Checkr.Expect("Always Fails", () => false)
            select Case.Closed;
    }

    [Fact]
    [DocContent("The following simple example has one input but always fails.")]
    [DocExample(typeof(InputShrinking), nameof(Example_Check))]
    [DocContent(
@"Input shrinking reruns the failing execution and prunes inputs whose values don't affect the failure.

So, in the previous example, after *Input Shrinking*, the result is:
")]
    [DocCodeFile("InputShrinking.txt", "text")]
    public void Example()
    {
        var article = TheJournalist.Exposes(() =>
            Example_Check().Run(165641598));
        WriteAsserts(article);
        WriteReport(article, "InputShrinking.txt");
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("Always Fails", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> Example_Two_Check()
    {
        return
            from one in Checkr.Input("Input One", Fuzzr.Int())
            from two in Checkr.Input("Input Two", Fuzzr.Int())
            from three in Checkr.Input("Input Three", Fuzzr.Int())
            from result in Checkr.Act("Result", () => one + three)
            from expect in Checkr.Expect("Should not be 42", () => result != 42)
            select Case.Closed;
    }

    [Fact]
    [DocContent("Another example, slightly trickier.")]
    [DocExample(typeof(InputShrinking), nameof(Example_Two_Check))]
    [DocContent(
@"As you can see from the test definition, two inputs are involved in a possible failure.

QuickCheckr correctly identifies the relevant inputs.

Result:
")]
    [DocCodeFile("InputShrinking_Two.txt", "text")]
    [DocContent("Many more examples focused on shrinking behaviour can be found [here][HoneyIShrunkTheInputs].")]
    public void Example_Two()
    {
        var article = TheJournalist.Exposes(() =>
            Example_Two_Check().Run(231122398));
        WriteAsserts(article);
        WriteReport(article, "InputShrinking_Two.txt");
        Verify_Two(article);
    }

    private static void Verify_Two(Article article)
    {
        Assert.Equal("Should not be 42", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(2, article.Total().Inputs());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Result", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Input One", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("12", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Input Three", article.Execution(1).Input(2).Read().Label);
        Assert.Equal("30", article.Execution(1).Input(2).Read().Value);
    }
}
