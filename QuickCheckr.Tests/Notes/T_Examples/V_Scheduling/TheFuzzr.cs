using QuickFuzzr;
using WibblyWobbly;

namespace QuickCheckr.Tests.Notes.T_Examples.V_Scheduling;

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
            from start in Fuzzr.Int(8, otherSlot.Start - 1)
            from end in Fuzzr.Int(start + 1, otherSlot.Start)
            select new Timeslot { Start = start, End = end };
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