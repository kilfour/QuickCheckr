using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Notes.T_Examples.C_AStatefulList;

[DocFile]
public class AStatefulList : BaseTest
{
    [CodeExample]
    public class ListWrap
    {
        private readonly List<int> list = [];
        public IReadOnlyList<int> List => list;
        public void Add(int item) => list.Add(item);
        public void Remove(int item) => list.Remove(item);
    }

    [Fact]
    [DocContent("**The Model:**")]
    [DocExample(typeof(ListWrap))]
    [DocContent("**The Checkr:**")]
    [DocExample(typeof(AStatefulList), nameof(GetCheckr))]
    [DocContent("**The Report:**")]
    [DocCodeFile("AStatefulList.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(254875886, 10.ExecutionsPerRun()));
        WriteAsserts(article, false);
        WriteReport<AStatefulList>(article, false);
        ExplainMe<AStatefulList>(false);
        Verify(article);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from listWrap in Trackr.Stashed(() => new ListWrap())
            from item in Checkr.Input("Item", Fuzzr.Int(0, 10))
            from traceBefore in Checkr.Trace("Before", () => listWrap.List)
            from operations in Checkr.OneOf(
                from add in Checkr.Act("Add", () => listWrap.Add(item))
                from listContainsItem in Checkr.Expect("List Contains Item",
                    () => listWrap.List.Contains(item))
                select Case.Closed,
                from remove in Checkr.Act("Remove", () => listWrap.Remove(item))
                from listDoesNotContainItem in Checkr.Expect("List Does Not Contain Item",
                    () => !listWrap.List.Contains(item))
                select Case.Closed)
            from traceAfter in Checkr.Trace("After", () => listWrap.List)
            select Case.Closed;
    }

    private static void Verify(Article article)
    {
        Assert.Equal("List Does Not Contain Item", article.FailedExpectation());
        Assert.Equal(3, article.Total().Executions());
        Assert.Equal(3, article.Total().Actions());
        Assert.Equal(3, article.Total().Inputs());
        Assert.Equal(6, article.Total().Traces());
        Assert.Equal(2, article.Total().PassedSpecs());
        Assert.Equal(6, article.ShrinkCount);
        Assert.Equal(6, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Add", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Item", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("3", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Before", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("[ ]", article.Execution(1).Trace(1).Read().Value);
        Assert.Equal("After", article.Execution(1).Trace(2).Read().Label);
        Assert.Equal("[ 3 ]", article.Execution(1).Trace(2).Read().Value);
        Assert.Equal(7, article.Execution(2).Read().ExecutionId);
        Assert.Equal("Add", article.Execution(2).Action(1).Read().Label);
        Assert.Equal("Item", article.Execution(2).Input(1).Read().Label);
        Assert.Equal("3", article.Execution(2).Input(1).Read().Value);
        Assert.Equal("Before", article.Execution(2).Trace(1).Read().Label);
        Assert.Equal("[ 3 ]", article.Execution(2).Trace(1).Read().Value);
        Assert.Equal("After", article.Execution(2).Trace(2).Read().Label);
        Assert.Equal("[ 3, 3 ]", article.Execution(2).Trace(2).Read().Value);
        Assert.Equal(9, article.Execution(3).Read().ExecutionId);
        Assert.Equal("Remove", article.Execution(3).Action(1).Read().Label);
        Assert.Equal("Item", article.Execution(3).Input(1).Read().Label);
        Assert.Equal("3", article.Execution(3).Input(1).Read().Value);
        Assert.Equal("Before", article.Execution(3).Trace(1).Read().Label);
        Assert.Equal("[ 3, 3 ]", article.Execution(3).Trace(1).Read().Value);
        Assert.Equal("After", article.Execution(3).Trace(2).Read().Label);
        Assert.Equal("[ 3 ]", article.Execution(3).Trace(2).Read().Value);
        Assert.Equal("List Does Not Contain Item", article.PassedSpec(1).Read().Label);
        Assert.Equal(4, article.PassedSpec(1).Read().TimesPassed);
        Assert.Equal("List Contains Item", article.PassedSpec(2).Read().Label);
        Assert.Equal(4, article.PassedSpec(2).Read().TimesPassed);
    }
}