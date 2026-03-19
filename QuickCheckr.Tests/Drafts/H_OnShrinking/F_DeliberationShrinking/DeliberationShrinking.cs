using QuickCheckr;
using QuickCheckr.Protocol;
using QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.C_Configuration;
using QuickCheckr.Tests.Drafts.H_OnShrinking.D_InputReduction;
using QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.C_Objects.TheExamples.Models;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Drafts.H_OnShrinking.F_DeliberationShrinking;

[DocFile]
[DocLink(typeof(Configuration))]
[DocLink(typeof(OnShrinking))]
[DocLink(typeof(InputReduction))]
[DocContent("[On Shrinking][OnShrinking] | [Previous][InputReduction]  \n")]
[DocContent(
@"This next step, which is disabled by default, allows the shrinker to find *global* minimal failcases instead of *local* ones.  

A first example, as well as how to enable this feature,
can be found in the entry on Deliberation Policies in the [Configuration][Configuration] chapter.

But here are some more use cases.")]
public class DeliberationShrinking : BaseTest
{

    [Fact(Skip = "Fixed this behaviour")]
    [DocHeader("An Eager Shrinker")]
    [DocContent(
@"Under some rare circumstances, the Shrinker gets *greedy*.  
One currently identified case where this happens is if more than one input triggers the same failure.")]
    [DocExample(typeof(DeliberationShrinking), nameof(GetCheckrGreedy))]
    [DocContent(
@"Now when the list contains multiple elements `1`, we might get:")]
    [DocCodeFile("DeliberationShrinkingGreedy.txt", "text")]
    [DocContent(
@"It seems QuickCheckr thinks that the minimal fail-case is a list with atleast 3 elements (*correct*),
but the element values are considered irrelevant (*wrong*).")]
    public void RunTest_DeliberatedGreedy()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckrGreedy().Run(227109555, 1.ExecutionsPerRun()));
        WriteAsserts(article, false);
        WriteReport(article, "DeliberationShrinkingGreedy.txt", false);
        Verify_DeliberatedGreedy(article);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckrGreedy()
    {
        return
            from input in Checkr.Input("Input", Fuzzr.Int(0, 5).Many(3))
            from trace in Checkr.Trace("List", () => input)
            from expect in Checkr.ExpectWhen("List count > 2 and does not contain 1",
                () => input.Count() > 2,
                () => !input.Contains(1))
            select Case.Closed;
    }

    private static void Verify_DeliberatedGreedy(Article article)
    {
        Assert.Equal("List count > 2 and does not contain 1", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Total().Traces());
        Assert.Equal(3, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("[ _, _, _ ]", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("List", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("[ 1, 2, 1 ]", article.Execution(1).Trace(1).Read().Value);
    }

    [Fact]
    [DocContent("For this rare case we  add a DeliberationPolicy, allowing a more explanatory fail-case to surface:")]
    [DocExample(typeof(DeliberationShrinking), nameof(ConfigureShrinkCount))]
    [DocContent("**Report:**")]
    [DocCodeFile("DeliberationShrinkingGreedyFixed.txt", "text")]
    public void RunTest_DeliberatedGreedyFixed()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckrGreedy().Run(20.Runs(), 1.ExecutionsPerRun(), ConfigureShrinkCount));
        WriteAsserts(article, false);
        WriteReport(article, "DeliberationShrinkingGreedyFixed.txt", false);
        Verify_DeliberatedGreedyFixed(article);
    }

    [CodeExample]
    private static CheckrConfig ConfigureShrinkCount(CheckrConfig config)
        => config with { DeliberationPolicy = DeliberationPolicy.MinimumShrinkCount };

    private static void Verify_DeliberatedGreedyFixed(Article article)
    {
        Assert.NotEqual("[ _, _, _ ]", article.Execution(1).Input(1).Read().Value);
    }

    [Fact]
    [DocContent("A similar thing can happen when shrinking objects, and the solution is exactly the same")]
    [DocExample(typeof(DeliberationShrinking), nameof(GetCheckrGreedyObjects))]
    [DocContent("**With Deliberation Policy:**")]
    [DocCodeFile("DeliberationShrinkingGreedyObjects.txt", "text")]
    public void RunTest_DeliberatedGreedyObjects()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckrGreedyObjects().Run(20.Runs(), 1.ExecutionsPerRun(), ConfigureShrinkCount));
        WriteAsserts(article, false);
        WriteReport(article, "DeliberationShrinkingGreedyObjects.txt", false);
        Verify_DeliberatedGreedyObjects(article);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckrGreedyObjects()
    {
        return
            from obj in Checkr.Input("BaggyInters",
                from cfg in Configr.Primitive(Fuzzr.Int(1, 5))
                from result in Fuzzr.One<BaggyInters>()
                select result)
            from trace in Checkr.Trace("Original", () => obj)
            from expect in Checkr.Expect("obj.IntOne == 1 || obj.IntThree == 1",
                () => obj.IntOne != 1 && obj.IntThree != 1)
            select Case.Closed;
    }

    private static void Verify_DeliberatedGreedyObjects(Article article)
    {
        Assert.Equal("obj.IntOne == 1 || obj.IntThree == 1", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
    }
}