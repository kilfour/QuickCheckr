using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Tests.Tools;
using QuickPulse.Explains;


namespace QuickCheckr.Tests.CookBook.V_Scheduling;

[DocFile]
[DocContent(
@"With this example we are venturing closer to *QuickCheck-like* territory.  

It is not about stateful testing, it is in fact testing a pure function. We are validating a validator.

And even though this is not the main focus of QuickCheckr,
this example demonstrates how the design decision to split off input generation from shrinking,
simplifies test definition in non-trivial cases.

")]
public class Scheduling : CheckrTest<Scheduling>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocContent(
@"**Demonstrates:**
- Validator testing for pure functions
- Constraint-preserving input generation
- Separation of generation and shrinking
")]
    [DocModelHeader]
    [DocCodeFile("Model.cs", "csharp", 2)]
    [DocContent(
@"So this model is a scheduling thingy, with a validation function that verifies whether or not Bookings overlap.

The Checkr itself is remarkably simple:
")]
    [DocCheckr]
    [DocContent(
@"The trick to this one lies entirely with the Fuzzr.

We deliberately generate only valid, 
non-overlapping bookings, and rely on shrinking to expose edge cases where the validator fails. 

This complicates the Fuzzr somewhat, but not overly so.
The only slightly tricky bit is the `NonOverlappingTimeSlot` part, I reckon.

Generating a valid booking is easy, and then we just use that one to generate another, non-overlapping, booking:
")]
    [DocCodeFile("TheFuzzr.cs", "csharp", 5)]
    [DocReportHeader]
    [DocContent("And ofcourse, there's an edge-case.")]
    [DocReport]
    public override void Example() => Document(a => a.Run(511619818));

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from bookingOne in Checkr.Input("Booking One", TheFuzzr.ValidBooking)
        from bookingTwo in Checkr.Input("Booking Two", TheFuzzr.NonOverlappingBooking(bookingOne))
        from expect in Checkr.Expect("Bookings do not overlap", () => !bookingOne.OverlapsWith(bookingTwo))
        select Case.Closed;

    // protected override CheckrConfig Configure(CheckrConfig config)
    //     => config with { ReportMode = config.ReportMode & ~(ReportMode.Labels | ReportMode.Warning) };

    protected override void Verify(Article article)
    {
        Assert.Equal("Bookings do not overlap", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(2, article.Total().Inputs());
        // Assert.Equal(8, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Booking One", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("{ Start: 27.December(2025), End: 2.January(2026), Schedule: { Thursday: [ { Start: 10, End: 15 } ] } }", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("Booking Two", article.Execution(1).Input(2).Read().Label);
        Assert.Equal("{ Start: 25.December(2025), End: 31.December(2025), Schedule: { Thursday: [ { Start: 14, End: 16 } ] } }", article.Execution(1).Input(2).Read().Value);
    }
}