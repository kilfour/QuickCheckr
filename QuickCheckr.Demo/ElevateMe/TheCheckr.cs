using QuickCheckr.Shrinking;
using QuickFuzzr;
using QuickFuzzr.UnderTheHood;
using QuickPulse.Bolts;

namespace QuickCheckr.Demo.ElevateMe.Sut;

public record ElevatorInfo(ElevatorSystem ElevatorSystem, bool IsStepExecution, int Tick);

public record Passenger(int Id, PassengerStatus Status);

public abstract record PassengerStatus
{
    public abstract int TimeInSystem(int tick);
}

public record WaitingOn(int Floor, int Since, Direction Direction) : PassengerStatus
{
    public override int TimeInSystem(int tick) => tick - Since;
}

public record TravellingTo(int Floor, int Since) : PassengerStatus
{
    public override int TimeInSystem(int tick) => tick - Since;
}

public class TheCheckr
{
    const int NumberOfFloors = 5;
    const int Capacity = 4;
    const int MaxPassengersInSystem = Capacity;
    const int CounterMax = 3;
    const int MaxTimeInSystemThreshold = 30;

    public static CheckrOf<ElevatorInfo> Check =
        from counter in         /* */ Checkr.Input("Counter", RollingCounter("counter"), Shrink.None<int>())
        from elevatorSystem in  /* */ Trackr.Stashed(() => new ElevatorSystem(NumberOfFloors, Capacity))
        from tick in            /* */ Trackr.Stashed("tick", () => new Cell<int>(0))
        from isStepExecution in /* */ Checkr.Capture(() => counter == CounterMax)
        from option in          /* */ CallOption(elevatorSystem, tick, isStepExecution)
        from call in            /* */ Checkr.When(option.Condition, option.Checkr)
        from step in            /* */ Checkr.ActWhen("Step", () => isStepExecution, () => { elevatorSystem.Step(); tick.Value = tick.Value + 1; })
        from exit in            /* */ Exit(elevatorSystem, isStepExecution, tick.Value)
        from enter in           /* */ Enter(elevatorSystem, isStepExecution)
        from check in           /* */ CheckTimeInSystem(tick)
        select new ElevatorInfo(elevatorSystem, isStepExecution, tick.Value);

    private static CheckrOf<CheckrOption> CallOption(ElevatorSystem elevatorSystem, Cell<int> tick, bool isStepExecution) =>
        Poolr.When<Passenger>(a => a.Count < MaxPassengersInSystem && !isStepExecution,
            Poolr.Add("Store",
                from id in Checkr.Input("Passenger Name", Fuzzr.Counter("passenger-id"), Shrink.None<int>())
                from floor in Checkr.Input("Waiting OnFloor", Fuzzr.Int(0, NumberOfFloors),
                    Shrinkr.For<int>().Classify(Try.Values(Enumerable.Range(0, NumberOfFloors).ToArray())))
                from direction in Checkr.Input("Direction", Fuzzr.OneOf(Direction.Up, Direction.Down))
                from call in Checkr.Act("Call", () => elevatorSystem.Call(floor, direction))
                select new Passenger(id, new WaitingOn(floor, tick.Value, direction))));

    private static CheckrOf<Case> Enter(ElevatorSystem elevatorSystem, bool isStepExecution) =>
        Poolr.ForEach<Passenger>(
            passenger => CanEnter(elevatorSystem, isStepExecution, passenger),
            (passenger, ops) =>
                from floor in Checkr.Input($"Passenger-{passenger.Id} Enters, Destination", Fuzzr.Int(0, NumberOfFloors)
                        .Where(a => passenger.Status is WaitingOn waitingOn && a != waitingOn.Floor))
                from enter in Checkr.Perform(() => elevatorSystem.Enter(floor))
                from _ in ops.Update(EnteringElevator(passenger, floor))
                select Case.Closed);

    private static bool CanEnter(ElevatorSystem elevatorSystem, bool isStepExecution, Passenger passenger) =>
        isStepExecution
        && passenger.Status is WaitingOn waitingOn
        && elevatorSystem.Status().CurrentFloor == waitingOn.Floor
        && elevatorSystem.Status().DoorsAreOpen
        && elevatorSystem.Status().Passengers < Capacity;

    private static Passenger EnteringElevator(Passenger passenger, int destination)
    {
        var waitingOn = (WaitingOn)passenger.Status;
        return passenger with
        {
            Status = new TravellingTo(destination, waitingOn.Since)
        };
    }

    private static CheckrOf<Case> Exit(ElevatorSystem elevatorSystem, bool isStepExecution, int tick) =>
        Poolr.ForEach<Passenger>(a =>
            isStepExecution
            && a.Status is TravellingTo travellingTo
            && elevatorSystem.Status().CurrentFloor == travellingTo.Floor
            && elevatorSystem.Status().DoorsAreOpen,
            (_, ops) => Checkr.Perform((() => ops.Remove())));

    private static CheckrOf<Case> CheckTimeInSystem(Cell<int> tick) =>
        Poolr.ForEach<Passenger>(passenger =>
            Checkr.Expect($"TimeInSystem < {MaxTimeInSystemThreshold}",
            () => passenger.Status.TimeInSystem(tick.Value) < MaxTimeInSystemThreshold));

    public static FuzzrOf<int> RollingCounter(object key)
    {
        ArgumentNullException.ThrowIfNull(key);
        return state =>
        {
            var cnt = state.Get(key, 1) + 1;
            if (cnt > CounterMax) cnt = 1;
            state.Set(key, cnt);
            return new Result<int>(cnt, state);
        };
    }
}
