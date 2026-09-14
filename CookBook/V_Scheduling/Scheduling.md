# Scheduling
With this example we are venturing closer to *QuickCheck-like* territory.  

It is not about stateful testing, it is in fact testing a pure function. We are validating a validator.

And even though this is not the main focus of QuickCheckr,
this example demonstrates how the design decision to split off input generation from shrinking,
simplifies test definition in non-trivial cases.

  
**Demonstrates:**
- Validator testing for pure functions
- Constraint-preserving input generation
- Separation of generation and shrinking
  

**The Model:**  
```csharp
public class Booking
{
    public DateOnly Start { get; set; }
    public DateOnly End { get; set; }
    public Schedule Schedule { get; set; } = new Schedule();

    public bool OverlapsWith(Booking otherBooking)
    {
        var start = Max(Start, otherBooking.Start);
        var end = Min(End, otherBooking.End);
        if (end < start) return false;
        if (SlotsOverlap(Schedule.Monday, otherBooking.Schedule.Monday)) return true;
        if (SlotsOverlap(Schedule.Tuesday, otherBooking.Schedule.Tuesday)) return true;
        if (SlotsOverlap(Schedule.Wednesday, otherBooking.Schedule.Wednesday)) return true;
        if (SlotsOverlap(Schedule.Thursday, otherBooking.Schedule.Thursday)) return true;
        if (SlotsOverlap(Schedule.Friday, otherBooking.Schedule.Friday)) return true;
        return false;
    }

    private static bool SlotsOverlap(List<Timeslot> slotsOne, List<Timeslot> slotsTwo)
    {
        foreach (var slotOne in slotsOne)
            foreach (var slotTwo in slotsTwo)
                if (slotOne.OverlapsWith(slotTwo))
                    return true;
        return false;
    }

    private static DateOnly Max(DateOnly x, DateOnly y) => x > y ? x : y;
    private static DateOnly Min(DateOnly x, DateOnly y) => x < y ? x : y;
}

public class Schedule
{
    public List<Timeslot> Monday { get; set; } = [];
    public List<Timeslot> Tuesday { get; set; } = [];
    public List<Timeslot> Wednesday { get; set; } = [];
    public List<Timeslot> Thursday { get; set; } = [];
    public List<Timeslot> Friday { get; set; } = [];
}

public class Timeslot
{
    public int Start { get; set; }
    public int End { get; set; }
    public bool OverlapsWith(Timeslot otherTimeSlot)
    {
        if (Start < otherTimeSlot.End && End > otherTimeSlot.Start) return true;
        return false;
    }
}
```
So this model is a scheduling thingy, with a validation function that verifies whether or not Bookings overlap.

The Checkr itself is remarkably simple:
  

**The Checkr:**  
```csharp
from bookingOne in Checkr.Input("Booking One", TheFuzzr.ValidBooking)
from bookingTwo in Checkr.Input("Booking Two", TheFuzzr.NonOverlappingBooking(bookingOne))
from expect in Checkr.Expect("Bookings do not overlap", () => !bookingOne.OverlapsWith(bookingTwo))
select Case.Closed;
```
The trick to this one lies entirely with the Fuzzr.

We deliberately generate only valid, 
non-overlapping bookings, and rely on shrinking to expose edge cases where the validator fails. 

This complicates the Fuzzr somewhat, but not overly so.
The only slightly tricky bit is the `NonOverlappingTimeSlot` part, I reckon.

Generating a valid booking is easy, and then we just use that one to generate another, non-overlapping, booking:
  
```csharp
public class TheFuzzr
{
    private readonly static FuzzrOf<Timeslot> ValidTimeSlot =
        from start in Fuzzr.Int(10, 13)
        from end in Fuzzr.Int(13, 16)
        select new Timeslot { Start = start, End = end };

    public readonly static FuzzrOf<Booking> ValidBooking =
        from start in Fuzzr.DateOnly(1.December(2025), 30.December(2025))
        from monday in ValidTimeSlot
        from tuesday in ValidTimeSlot
        from wednesday in ValidTimeSlot
        from thursday in ValidTimeSlot
        from friday in ValidTimeSlot
        select new Booking
        {
            Start = start,
            End = start.AddDays(6),
            Schedule = new Schedule
            {
                Monday = [monday],
                Tuesday = [tuesday],
                Wednesday = [wednesday],
                Thursday = [thursday],
                Friday = [friday]
            }
        };

    public static FuzzrOf<Booking> NonOverlappingBooking(Booking booking)
    {
        return
            from start in Fuzzr.DateOnly(1.December(2025), 30.December(2025))
            from monday in NonOverlappingTimeSlot(start, booking, DayOfWeek.Monday)
            from tuesday in NonOverlappingTimeSlot(start, booking, DayOfWeek.Tuesday)
            from wednesday in NonOverlappingTimeSlot(start, booking, DayOfWeek.Wednesday)
            from thursday in NonOverlappingTimeSlot(start, booking, DayOfWeek.Thursday)
            from friday in NonOverlappingTimeSlot(start, booking, DayOfWeek.Friday)
            select new Booking
            {
                Start = start,
                End = start.AddDays(6),
                Schedule = new Schedule
                {
                    Monday = [monday],
                    Tuesday = [tuesday],
                    Wednesday = [wednesday],
                    Thursday = [thursday],
                    Friday = [friday]
                }
            };
    }

    private static DateOnly WeekdayInThis7DayRange(DateOnly start, DayOfWeek weekDay) =>
        start.AddDays(((int)weekDay - (int)start.DayOfWeek + 7) % 7);

    private static FuzzrOf<Timeslot> NonOverlappingTimeSlot(DateOnly startDate, Booking booking, DayOfWeek weekDay)
    {
        var day = WeekdayInThis7DayRange(startDate, weekDay);
        var otherBookingDay = WeekdayInThis7DayRange(booking.Start, weekDay);
        if (day != otherBookingDay)
            return
                from start in Fuzzr.Int(8, 17)
                from end in Fuzzr.Int(start + 1, 18)
                select new Timeslot { Start = start, End = end };
        var otherSlot = GetSlotForDay(booking, weekDay);
        return
            from start1 in Fuzzr.Int(8, otherSlot.Start - 1)
            from end1 in Fuzzr.Int(start1 + 1, otherSlot.Start)
            from before in Fuzzr.One(() => new Timeslot { Start = start1, End = end1 })
            from start2 in Fuzzr.Int(otherSlot.End, 17)
            from end2 in Fuzzr.Int(start2 + 1, 18)
            from after in Fuzzr.One(() => new Timeslot { Start = start1, End = end2 })
            from slot in Fuzzr.OneOf(before, after)
            select slot;
    }

    private static Timeslot GetSlotForDay(Booking booking, DayOfWeek day)
    {
        return day switch
        {
            DayOfWeek.Monday => booking.Schedule.Monday.First(),
            DayOfWeek.Tuesday => booking.Schedule.Tuesday.First(),
            DayOfWeek.Wednesday => booking.Schedule.Wednesday.First(),
            DayOfWeek.Thursday => booking.Schedule.Thursday.First(),
            DayOfWeek.Friday => booking.Schedule.Friday.First(),
            _ => throw new ArgumentOutOfRangeException(nameof(day), day, null)
        };
    }
}
```
And ofcourse, there's an edge-case.  

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Scheduling.cs:64:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 8 shrinks)
 Seed:                    511619818
 ------------------------------------------------------------
  Executed:
   - Input: Booking One = { Start: 27.December(2025), End: 2.January(2026), Schedule: { Thursday: [ { Start: 10, End: 15 } ] } }
   - Input: Booking Two = { Start: 25.December(2025), End: 31.December(2025), Schedule: { Thursday: [ { Start: 14, End: 16 } ] } }
   - WARNING: Empty collections deemed irrelevant at:
       Booking One.Schedule: Monday, Tuesday, Wednesday, Friday
       Booking Two.Schedule: Monday, Tuesday, Wednesday, Friday
 ==================================================
  !! Expectation Failed: Bookings do not overlap
 ==================================================
```
