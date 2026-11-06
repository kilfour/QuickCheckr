using QuickCheckr.Demo.DoctorsOrders.Tests;
using QuickCheckr.Demo.ElevateMe.Sut;
using QuickFuzzr;

namespace QuickCheckr.Demo.ElevateMe;

public class ElevateMeTests
{
    // [DemoFact]
    public void ElevateMeCheckr() //73986571
        => MyCheckr.FileAs("ElevateMe").Run(73986571 /* 50.Runs() */, 200.ExecutionsPerRun());

    public CheckrOf<Case> MyCheckr =
        from info in TheCheckr.Check
        from traceElevator in   /* */ TraceElevator(info.ElevatorSystem)
        from tracePassengers in /* */ TracePassengers()
        select Case.Closed;

    private static CheckrOf<Case> TraceElevator(ElevatorSystem elevatorSystem) =>
        //  from queue in Checkr.Trace("Queue", () => elevatorSystem.CallQueue)
        //  from floorQueue in Checkr.Trace("Floor Queue", () => elevatorSystem.Elevator.FloorQueue)
        from status in Checkr.Trace("Status", b => b.Show(elevatorSystem.Status()))
        select Case.Closed;

    private static CheckrOf<Case> TracePassengers() =>
        Poolr.ForEach<Passenger>(a => Checkr.Trace($"Passenger-{a.Id}  TiS", b => b.Show(a.Status)));
}