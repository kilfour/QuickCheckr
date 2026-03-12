using QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.A_Showr.Reference;
using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickPulse.Show;


namespace QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.A_Showr;

[DocFile]
[DocLink(typeof(A_ShowrReference))]
[DocContent(
@"This static factory class produces a CheckrOf<Case> which allows one to configure the pretty-printer used in the report.

It is implemented as such, instead of as an option on the configuration object,
because this allows for two useful things:")]
[DocInclude(typeof(B_TheShowrClassDynamicConfiguration), true)]
[DocInclude(typeof(C_TheShowrClassContextAware), true)]
[DocContent("A full reference to the available options can be found [here][A_ShowrReference].")]
public class A_TheShowrClass : BaseTest
{
    [Fact]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(344916298, 2.ExecutionsPerRun()));
        WriteAsserts(article, false);
        WriteReport<A_TheShowrClass>(article, false);
        Verify(article);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from acc in Trackr.Stashed(() => new Accumelator())
            from count in Checkr.Input("Counter", Fuzzr.Counter("count"))
            from show in Showr.ForAll(() => Please.AllowMe().ToReplace<int>(a => $"Int {a}"))
            from a in Checkr.Input("a", Fuzzr.Int())
            from b in Checkr.Input("b", Fuzzr.Int())
            from c in Checkr.Input("c", Fuzzr.Int())
            from add in Checkr.Act("Add", () => acc.Add(a, b))
            from notAnswerOnTwo in Checkr.Expect("not 42 on execution 2", () => !(count == 2 && acc.Total == 42))
            select Case.Closed;
    }

    private static void Verify(Article article)
    {
        Assert.Equal("not 42 on execution 2", article.FailureDescription());
        Assert.Equal(2, article.Total().Executions());
        Assert.Equal(2, article.Total().Actions());
        Assert.Equal(5, article.Total().Inputs());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal(3, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Add", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("a", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("8", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("b", article.Execution(1).Input(2).Read().Label);
        Assert.Equal("16", article.Execution(1).Input(2).Read().Value);
        Assert.Equal(2, article.Execution(2).Read().ExecutionId);
        Assert.Equal("Add", article.Execution(2).Action(1).Read().Label);
        Assert.Equal("Counter", article.Execution(2).Input(1).Read().Label);
        Assert.Equal("2", article.Execution(2).Input(1).Read().Value);
        Assert.Equal("a", article.Execution(2).Input(2).Read().Label);
        Assert.Equal("1", article.Execution(2).Input(2).Read().Value);
        Assert.Equal("b", article.Execution(2).Input(3).Read().Label);
        Assert.Equal("17", article.Execution(2).Input(3).Read().Value);
        Assert.Equal("not 42 on execution 2", article.PassedExpectation(1).Read().Label);
        Assert.Equal(1, article.PassedExpectation(1).Read().TimesPassed);
    }

    [CodeExample]
    public class Accumelator
    {
        private int total;
        public int Total => total;
        public void Add(params int[] numbers)
            => total += numbers.Sum();
    }
}