# Cook Book
The QuickCheckr Cookbook is a collection of examples showing common patterns, 
testing techniques, and pitfalls you can check using QuickCheckr.  
## Verifying List Reversal (Double-Reverse Law)
**Demonstrates:**
- Custom Fuzzrs
- Relevant input shrinking
- Double-application property


I guess this one is very much tradition in PBT world, and who am I to argue with tradition, ... so here we go.

**The (buggy) implementation:**  
```csharp
public class TheReverser
{
    public static List<int> Reverses(List<int> list)
        => [.. HideTheAnswer(list.AsEnumerable()).Reverse()];
    private static IEnumerable<int> HideTheAnswer(IEnumerable<int> list)
        => list.Where(a => a != 42).Concat(list.Where(a => a == 42));
}
```
**The Fuzzr:**  
```csharp
from listLength in Fuzzr.Int(0, 10)
from enumerable in Fuzzr.Int().Many(listLength)
select enumerable.ToList();
```
**The Checkr:**  
```csharp
from showr in Showr.ForInput()
from input in Checkr.Input("Input", ListFuzzr())
from result in Checkr.Act("Result",
    () => TheReverser.Reverses(TheReverser.Reverses(input)))
from traceResult in Checkr.Trace("Result", () => result)
from expect in Checkr.Expect(
    "Input == Result", () => input.SequenceEqual(result))
select Case.Closed;
```
**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ListReverse.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 8 shrinks)
 Seed:                    498116108
 ------------------------------------------------------------
  Executed: Result
   - Input  = [ _, 42 ]
              [ 62, 13, 42, 71, 81, 27, 62, 28, 85 ]
   - Result = [ 42, 62, 13, 71, 81, 27, 62, 28, 85 ]
 ==========================================
  !! Expectation Failed: Input == Result
 ==========================================
```
## Testing Aggregate Behaviour (Trackr.Verify)
**Demonstrates:**
- Accumulated-state testing
- `Trackr.Verify`
- `Trackr.StopWhenProven`

While not strictly speaking a property-test, as there is no shrinking involved,
this kind of test is nevertheless very useful.  
So much so that QuickCheckr provides a dedicated tool for it. 

We're testing a simple implementation of a die:
  
```csharp
public class DieSix
{
    public int Roll() => new Random().Next(1, 6);
}
```
And we want to ensure that it rolls the numbers 1-6.  For which use the following Checkr:  
```csharp
    from die in Trackr.Stashed(() => new DieSix())
    from observer in Trackr.Stashed(() => new HashSet<int>())
    from roll in Checkr.Act("Roll", () => observer.Add(die.Roll()))
    from verify in Trackr.Verify("Rolls",
        ("Contains 1", () => observer.Contains(1)),
        ("Contains 2", () => observer.Contains(2)),
        ("Contains 3", () => observer.Contains(3)),
        ("Contains 4", () => observer.Contains(4)),
        ("Contains 5", () => observer.Contains(5)),
        ("Contains 6", () => observer.Contains(6)))
    select Case.Closed;
```
`Trackr.Verify` is the tool you reach for when the correctness of a test depends on the accumulated state at the end of a run,
rather than the outcome of any single execution.
In this Loaded Die example, each roll is recorded in a stashed HashSet<int>, and the *verify* is evaluated only after the final execution has completed.
This makes it ideal for tests where you want to confirm that a whole series of operations, as a group, produced the expected overall behaviour.

**Report:**  
```text
------------------------------------------------------------
 Test:                    RunTest
 Location:                CookBook\L_TheLoadedDie\TheLoadedDie.cs:71:1
 Original failing run:    100 executions
 Seed:                    1273117969
 ------------------------------------------------------------
  Executed: Roll (100 Times)
 =================================================
  !!  The Verifier Disagrees: Rolls: Contains 6
 =================================================
```
Well, you probably saw this coming, but the die is bugged. `Random.Next` is upper-bound exclusive, so six is never rolled.  
A common mistake, and the verifier pointed it out in above report.

*Note:* `Trackr.Verify`never triggers shrinking, because there is no meaningful *minimal failing case*,
either the final state satisfies all verifications or it doesn't.  
### Stop! In the Name of ... Perf?
```csharp
public class DieSixFixed
{
    public int Roll() => new Random().Next(1, 7);
}
```
Ok, we fixed the bug, so now our Checkr passes.
But what if we get lucky and we satisfy the verify after the first six rolls ?
Bit of a waste to continue running the Checkr, no ?  

Let's use `Trackr.StopWhenProven`:  
```csharp
    from die in Trackr.Stashed(() => new DieSixFixed())
    from observer in Trackr.Stashed(() => new HashSet<int>())
    from roll in Checkr.Act("Roll", () => observer.Add(die.Roll()))
    from proven in Trackr.StopWhenProven("All sides rolled",
        () => observer.Order().SequenceEqual([1, 2, 3, 4, 5, 6])) // <=
    from verify in Trackr.Verify("Rolls",
        ("Contains 1", () => observer.Contains(1)),
        ("Contains 2", () => observer.Contains(2)),
        ("Contains 3", () => observer.Contains(3)),
        ("Contains 4", () => observer.Contains(4)),
        ("Contains 5", () => observer.Contains(5)),
        ("Contains 6", () => observer.Contains(6)))
    select Case.Closed;
```
Whenever the predicate passed into `Trackr.StopWhenProven` returns true,
the Checkr is considered proven and it concludes immediately, ... no more executions.  
## Scheduling
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

**The Report:**  
And ofcourse, there's an edge-case.  
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
## Parsing Numbers
Another non-stateful testing example. And one where a Reducer is absolutely necessary.

I used this one, during a course I was teaching to verify student code,
because I did not feel like combing through all the code looking for bugs manually.  
So you could say it's a real world example, and I deliberately did not clean up the code.  

I had a working model, so all that was needed was a Fuzzr, and a way to get to a minimal fail-case.  
The Reducer is far from perfect, but by adding a deliberation policy, this test quickly and reliably came up with said minimal fail-case.  

  
**Demonstrates:**
- Oracle style testing
- Custom Reducers
- Custom Deliberation Policies
  

**The Checkr:**  
```csharp
from input in Checkr.Input("Expression",
        ParseFuzzr.Expression(),
        Reduce.Function<string>(ParseReducer.Reducer, true))
from oracleResult in Checkr.Act("Oracle Result", () => LostIn.Translation(input).Eval())
from result in Checkr.Act("SUT Result", () => LostIn.FaultyTranslation(input).Eval())
from traceResult in Checkr.Trace("Compare", () => $"Oracle: {oracleResult} /= SUT: {result}")
from match in Checkr.ExpectWhen("Match",
    () => !double.IsNaN(oracleResult),
    () => oracleResult == result)
select Case.Closed;
```

**The Configuration:**  
```csharp
protected override CheckrConfig Configure(CheckrConfig config)
    => config with
    {
        DeliberationPolicy = caseFile => caseFile.InputsNamed<string>("Expression", a => a.Length),
        DeliberationTarget = 4,
        ReportMode = config.ReportMode & ~ReportMode.Labels
    };
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ParsingNumbers.cs:81:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1166603196
 ------------------------------------------------------------
  Executed: Oracle Result, SUT Result
   - Expression = "- (3 * 1 - 2 * 2 / 2 * 3) ^ (1 * 1 - 2 * 3 / 2) ^ (1 / 1) ^ 1 - (2 / 3 * 1 * 2 + 1 + 2 / 3 / 2) * (1 / 2 / 3 * 3 - 1) ^ 3"
     Redux        "-2^2"
   - Compare    = Oracle: 0,2222222222222222 /= SUT: 0,4444444444444444
 ================================
  !! Expectation Failed: Match
 ================================
```

**The Fuzzr:**  
```csharp
public class ParseFuzzr
{
    public static FuzzrOf<string> Expression(int maxDepth = 4) => Expr(maxDepth);
    public static readonly FuzzrOf<string> Number = from i in Fuzzr.Int(1, 4) select i.ToString();
    private static readonly FuzzrOf<string> AddOp = Fuzzr.OneOf("+", "-");
    private static readonly FuzzrOf<string> MulOp = Fuzzr.OneOf("*", "/");
    private static readonly FuzzrOf<string> PowOp = Fuzzr.Constant("^");
    private static readonly FuzzrOf<string> WS =
        from chs in Fuzzr.Constant(' ').Many(1)
        select new string([.. chs]);
    private static readonly FuzzrOf<string> UnaryOp = Fuzzr.OneOf("-");
    private static FuzzrOf<string> Primary(int maxDepth) =>
        maxDepth <= 0
        ? Number
        : Fuzzr.OneOf(
            Number,
            from l in Fuzzr.Constant("(")
            from e in Expr(maxDepth - 1)
            from r in Fuzzr.Constant(")")
            select l + e + r,
            from op in UnaryOp
            from ws in WS
            from inner in Primary(maxDepth - 1)
            select op + ws + inner
          );
    private static FuzzrOf<string> Factor(int maxDepth) =>
        from a in Primary(maxDepth)
        from tail in maxDepth <= 0
            ? Fuzzr.Constant("")
            : Fuzzr.OneOf(
                  Fuzzr.Constant(""),
                  from w1 in WS
                  from op in PowOp
                  from w2 in WS
                  from b in Factor(maxDepth - 1)
                  select w1 + op + w2 + b)
        select a + tail;
    private static FuzzrOf<string> Term(int maxDepth) =>
        from head in Factor(maxDepth)
        from pieces in (
            from w1 in WS
            from op in MulOp
            from w2 in WS
            from rhs in Factor(maxDepth - 1)
            select w1 + op + w2 + rhs
        ).Many(0, 3)
        select head + string.Concat(pieces);
    private static FuzzrOf<string> Expr(int maxDepth) =>
        from depth in Fuzzr.Int(0, maxDepth)
        from head in Term(depth)
        from pieces in (
            from w1 in WS
            from op in AddOp
            from w2 in WS
            from rhs in Term(depth - 1)
            select w1 + op + w2 + rhs
        ).Many(0, 3)
        select head + string.Concat(pieces);
}
```
This Fuzzr generates a valid numeric expression string of variable length.  
 *Some Examples:*  
```text
(2 * 1 / 1 / 3 + 3) ^ - 1 ^ 1 / - 2 / 3 ^ 3 * - 1
- 1 ^ 3 / 2 / 1 * 1
- 3
- 2 * 1 * 2 * 3 - 1 * 1 * 1 / 3 + 2
(1 - 1 - 1 / 2) ^ - 2 ^ 3 / (1 / 2 * 3 / 2 - 1 * 2 / 1) ^ 2 - - 2 ^ 2 + - 3 ^ 3
```

**The Reducer:**  
```csharp
public static class ParseReducer
{
    public static IEnumerable<string> Reducer(string expr)
    {
        foreach (var item in RemoveOperations(expr))
            yield return item;
        string output = Regex.Replace(expr, @"\([^()]*\)", "2");
        yield return output;
        foreach (var item in RemoveOperations(output))
            yield return item;
    }
    private static IEnumerable<string> RemoveOperations(string expr)
    {
        var tokens = Lexer.Tokenize(expr);
        tokens = tokens.Where(a => a.Kind != TokenKind.LParen && a.Kind != TokenKind.RParen);
        yield return TokensToString(tokens);
        var list = tokens.ToList();
        for (int i = 0; i < list.Count; i++)
        {
            yield return TokensToString(tokens.Skip(i));
            if (list[i].IsOperator && !list[i + 1].IsOperator)
                yield return TokensToString(tokens.Skip(i + 1));
        }
        for (int i = list.Count - 1; i > 0; i--)
        {
            if (list[i].IsOperator && !list[i - 1].IsOperator)
                yield return TokensToString(tokens.Take(i));
        }
    }
    private static string TokensToString(IEnumerable<Token> tokens)
        => string.Join("", tokens.Select(a => a.Text));
}
```
