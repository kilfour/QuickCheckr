namespace QuickCheckr.Tests.PersonalNotes.T_Examples.V_Scheduling;

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
        // this is a hack for quickcheckr RemoveOneByOne deficiency
        if (slotsOne == null || slotsTwo == null) return false;

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