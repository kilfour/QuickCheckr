using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Drafts.Z_Issues.K_SystemTypes;

[DocFile]
public class SystemTypes : BaseTest
{
    [Fact]
    [DocExample(typeof(Thing))]
    [DocExample(typeof(SystemTypes), nameof(GetCheckr))]
    [DocCodeFile("SystemTypes.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckr().Run(1487819190, 1.ExecutionsPerRun()));
        WriteAsserts(article, false);
        WriteReport<SystemTypes>(article, false);
        Verify(article);
    }


    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from thing in Checkr.Input("A thing with a type", Fuzzr.Constant(new Thing()))
            from fail in Checkr.Expect("Fails", () => thing.Type.Name != typeof(int).Name)
            select Case.Closed;
    }

    private static void Verify(Article article)
    {
        Assert.Equal("Fails", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(2, article.Total().Warnings());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("A thing with a type", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ Type: Int32, OtherType: Int32 }", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Object of type 'Type' has no shrinkable properties at input 'A thing with a type.Type'.", article.Execution(1).Warning(1).Read().Value);
        Assert.Equal("Object of type 'Type' has no shrinkable properties at input 'A thing with a type.OtherType'.", article.Execution(1).Warning(2).Read().Value);
    }

    [Fact]
    [DocHeader("Workaround", 1)]
    [DocExample(typeof(SystemTypes), nameof(GetCheckrSolution))]
    [DocCodeFile("SystemTypesSolution.txt", "text")]
    public void RunTestSolution()
    {
        var article = TheJournalist.Exposes(() =>
            GetCheckrSolution().Run(350297528, 1.ExecutionsPerRun()));
        WriteAsserts(article, false);
        WriteReport(article, "SystemTypesSolution.txt", false);
        VerifySolution(article);
    }


    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckrSolution()
    {
        return
            from thing in Checkr.Input("A thing with a type",
                Fuzzr.Constant(new Thing()),
                () => Shrink.Values([typeof(string), typeof(double)]))
            from fail in Checkr.Expect("Fails", () => thing.Type.Name != typeof(int).Name)
            select Case.Closed;
    }

    private static void VerifySolution(Article article)
    {
        Assert.Equal("Fails", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(2, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("A thing with a type", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ Type: Int32 }", article.Execution(1).Input(1).Read().Value);
    }

    [CodeExample]
    public class Thing
    {
        public int Irrelevant { get; set; }
        public Type Type { get; set; } = typeof(int);
        public Type OtherType { get; set; } = typeof(int);
    }
}