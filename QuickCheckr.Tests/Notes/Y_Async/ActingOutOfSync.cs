using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Notes.Y_Async;

[DocFile]
public class ActingOutOfSync : BaseTest
{
    [Fact]
    [DocExample(typeof(Eventually))]
    [DocExample(typeof(ActingOutOfSync), nameof(GetCheckr))]
    [DocCodeFile("ActingOutOfSync.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(367926403));
        WriteAsserts(article);
        WriteReport<ActingOutOfSync>(article);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("Is not 42", article.FailedSpec());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Eventually.AddOne", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("41", article.Execution(1).Input(1).Read().Value);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from input in Checkr.Input("Input", Fuzzr.Int())
            from result in Checkr.Act("Eventually.AddOne", Eventually.AddOne(input))
            from spec in Checkr.Expect("Is not 42", () => result != 42)
            select Case.Closed;
    }

    [CodeExample]
    public class Eventually
    {
        public static Task<int> AddOne(int input)
        {
            return Task.FromResult(input + 1);
        }
    }

    [Fact]
    public void FuncAsTaskIf()
    {
        var check =
            from input in Checkr.Input("Input", Fuzzr.Constant(42))
            from result in Checkr.ActIf("Eventually.AddOne", () => true, Eventually.AddOne(input))
            from spec in Checkr.Expect("Is not 42", () => result != 42)
            select Case.Closed;
        check.Run(); // <= test passed == Task completed
    }

    [Fact]
    public void FuncAsTaskCarefully()
    {
        var check =
            from input in Checkr.Input("Input", Fuzzr.Constant(42))
            from result in Checkr.ActCarefully("Eventually.AddOne", Eventually.AddOne(input))
            from spec in Checkr.Expect("Is not 42", () => result.Value != 42)
            select Case.Closed;
        check.Run(); // <= test passed == Task completed
    }

    [Fact]
    public void ActionAsTask()
    {
        var check =
            from eventualAction in Trackr.Stashed(() => new EventualAction())
            from act in Checkr.Act("The Void", eventualAction.TheVoid())
            from spec in Checkr.Expect("Flagged", () => eventualAction.Flag)
            select Case.Closed;
        check.Run(); // <= test passed == flag is set == Task completed
    }

    [Fact]
    public void ActionAsTaskIf()
    {
        var check =
            from eventualAction in Trackr.Stashed(() => new EventualAction())
            from act in Checkr.ActIf("The Void", () => true, eventualAction.TheVoid())
            from spec in Checkr.Expect("Flagged", () => eventualAction.Flag)
            select Case.Closed;
        check.Run(); // <= test passed == flag is set == Task completed
    }

    public class EventualAction
    {
        public bool Flag { get; set; }
        public Task TheVoid()
        {
            return Task.Run(() => { Flag = true; });
        }
    }
}