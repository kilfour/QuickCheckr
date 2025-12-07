using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickCheckr.UnderTheHood.Protocol;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickPulse.Show;
using QuickPulse.Show.Bolts;


namespace QuickCheckr.Tests.PersonalNotes.T_Examples.V_Scheduling;

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
    [DocHeader("The Configuration:")]
    [DocExample(typeof(Scheduling), nameof(Configure))]
    [DocHeader("The Report:")]
    [DocCodeFile("Scheduling.txt", "text")]
    public void RunTest()
    {
        var article = TheJournalist.Exposes(() => GetCheckr().Run(1000.Runs(), Configure));
        WriteAsserts(article, false);
        WriteReport<Scheduling>(article, false);
        ExplainMe<Scheduling>(false);
        Verify(article);
    }

    private static void Verify(Article article)
    {

    }

    [CodeExample]
    private static CheckrConfig Configure(CheckrConfig config)
        => config with
        {
            Seed = 511619818,
            Editor = new ManOfWealthAndTaste()
                .ToInline<List<Timeslot>>()
                .ToInline<Timeslot>()
                .ToUseNonLinearTime()
                .ToPrettyPrint()
        };

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> GetCheckr()
    {
        return
            from bookingOne in Checkr.Input("Booking One", TheFuzzr.ValidBooking)
            from bookingTwo in Checkr.Input("Booking Two", TheFuzzr.NonOverlappingBooking(bookingOne))
            from traceOne in Checkr.Trace("Booking One", () => bookingOne)
            from traceTwo in Checkr.Trace("Booking Two", () => bookingTwo)
            from Spec in Checkr.Spec("Bookings do not overlap", () => !bookingOne.OverlapsWith(bookingTwo))
            select Case.Closed;
    }

    [Fact(Skip = "Explicit")]
    public void CheckFuzzr()
    {
        var bookingOne = TheFuzzr.ValidBooking.Generate();
        var bookingTwo = TheFuzzr.NonOverlappingBooking(bookingOne).Generate();
        (bookingOne, bookingTwo).PulseToLog();

    }
    // from before in Fuzzr.Bool()
    // let fuzzr = before ?
    //     from start in Fuzzr.Int(8, otherSlot.Start - 1)
    //     from end in Fuzzr.Int(start, otherSlot.Start)
    //     select new Timeslot { Start = start, End = end }
    //     : from start in Fuzzr.Int(8, otherSlot.Start - 1)
    //       from end in Fuzzr.Int(start, otherSlot.Start)
    //       select new Timeslot { Start = start, End = end }
    // select fuzzr;
}