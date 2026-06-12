using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.Notes.B_BuildingBlocks.S_TrackrVerify;

[DocFile]
public class TrackrVerify : BaseTest
{
    [Fact]
    [DocExample(typeof(TrackrVerify), nameof(GetCheckr))]
    [DocCodeFile("TrackrVerify.txt", "text")]
    public void RunTest()
    {
        var article = Journalist.Publish(GetCheckr(), a => a.Run(293407282, 20.ExecutionsPerRun()));
        WriteAsserts(article);
        WriteReport<TrackrVerify>(article);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("Contains 3", article.VerifyFailed());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal(20, article.Execution(1).Times);
        Assert.Equal("Roll", article.Execution(1).Action(1).Read().Label);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from observer in Trackr.Stashed(() => new HashSet<int>())
            // using quickfuzzr below, but really it's a SUT
            from roll in Checkr.Act("Roll", () => observer.Add(Fuzzr.Int(1, 3).Generate()))
            from verify in Trackr.Verify("Contains 3", () => observer.Contains(3)) // <= gen = [1, 3) : fails
            select Case.Closed;
    }

    [Fact]
    [DocExample(typeof(TrackrVerify), nameof(GetCheckr_Multiple))]
    [DocCodeFile("TrackrVerifyMultiple.txt", "text")]
    public void Multiple()
    {
        var article = Journalist.Publish(GetCheckr_Multiple(), a => a
            .Run(293407282, 20.ExecutionsPerRun()));
        WriteAsserts(article);
        WriteReport(article, "TrackrVerifyMultiple.txt");
        VerifyMultiple(article);
    }

    private static void VerifyMultiple(Article article)
    {
        Assert.Equal("Rolls: Contains 3, Contains 4", article.VerifyFailed());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal(20, article.Execution(1).Times);
        Assert.Equal("Roll", article.Execution(1).Action(1).Read().Label);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr_Multiple()
    {
        return
            from observer in Trackr.Stashed(() => new HashSet<int>())
            from roll in Checkr.Act("Roll", () => observer.Add(Fuzzr.Int(1, 3).Generate()))
            from verify in Trackr.Verify("Rolls",
                ("Contains 1", () => observer.Contains(1)),
                ("Contains 2", () => observer.Contains(2)),
                ("Contains 3", () => observer.Contains(3)),
                ("Contains 4", () => observer.Contains(4)))
            select Case.Closed;
    }
}