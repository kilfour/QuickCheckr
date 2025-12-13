using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickPulse.Show;


namespace QuickCheckr.Tests.Notes.T_Examples.V_Scheduling;

[DocFile]
public class Scheduling : BaseTest
{
    [Fact]
    [DocHeader("The Model:")]
    [DocCodeFile("Model.cs", "csharp", 2)]
    [DocHeader("The Fuzzr:")]
    [DocCodeFile("TheFuzzr.cs", "csharp", 2)]
    [DocHeader("The Checkr:")]
    [DocExample(typeof(Scheduling), nameof(GetCheckr))]
    [DocHeader("The Report:")]
    [DocCodeFile("Scheduling.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() => GetCheckr().Run(511619818));
        WriteAsserts(article, false);
        WriteReport<Scheduling>(article, false);
        ExplainMe<Scheduling>(false);
        Verify(article);
    }

    private static void Verify(Article article)
    {
        Assert.Equal("Bookings do not overlap", article.FailedSpec());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(2, article.Total().Inputs());
        Assert.Equal(8, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Booking One", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ Start: 27.December(2025), End: 2.January(2026), Schedule: { Thursday: [ { Start: 10, End: 15 } ] } }", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Booking Two", article.Execution(1).Input(2).Read().Label);
        Assert.Equal("{ Start: 25.December(2025), End: 31.December(2025), Schedule: { Thursday: [ { Start: 14, End: 16 } ] } }", article.Execution(1).Input(2).Read().Value);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from bookingOne in Checkr.Input("Booking One", TheFuzzr.ValidBooking)
            from bookingTwo in Checkr.Input("Booking Two", TheFuzzr.NonOverlappingBooking(bookingOne))
            from Spec in Checkr.Expect("Bookings do not overlap", () => !bookingOne.OverlapsWith(bookingTwo))
            select Case.Closed;
    }

    [Fact(Skip = "Explicit")]
    public void CheckFuzzr()
    {
        var bookingOne = TheFuzzr.ValidBooking.Generate();
        var bookingTwo = TheFuzzr.NonOverlappingBooking(bookingOne).Generate();
        (bookingOne, bookingTwo).PulseToLog();
    }
}