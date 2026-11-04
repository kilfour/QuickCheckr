namespace QuickCheckr.Demo.ElevateMe.Sut;

public enum Direction { Up, Down, Idle }

public record ElevatorStatus( // zoals class en constractor vood deze class
    int CurrentFloor,
    Direction Direction,
    bool DoorsAreOpen,
    int Passengers);


public class ElevatorSystem
{
    private int floors;
    private int capacity;
    private int currentFloor;
    private Direction direction;
    private bool doorsOpen;
    private int passengers;
    private List<int> calls = new List<int>(); // De lift van buitenaf aanvragen
    private Dictionary<int, int> destinations = new Dictionary<int, int>(); // De lift van binnenaf aanvragen
    private int nextStop;

    public ElevatorSystem(int floors, int capacity)
    {
        this.floors = floors;
        this.capacity = capacity;
        currentFloor = 0;
        direction = Direction.Idle;
        doorsOpen = false;
        passengers = 0;
    }
    public void Call(int floor, Direction direction) // bij buiten
    {
        if (!(floor >= 0 && floor < floors)) return; // doet niets als floor ongeldig is
        if (!calls.Contains(floor)) calls.Add(floor);

    }
    public void Enter(int destination) // bij binnen
    {
        if (!doorsOpen)
            throw new Exception("Doors are clased.");
        if (!(destination >= 0 && destination < floors))
            throw new Exception("Invalid floor.");
        if (passengers >= capacity)
            throw new Exception("Elevator is full.");
        if (destination == currentFloor)
            throw new Exception("Destination cannot be current floor.");

        passengers++;
        /*
        if (!destinations.Contains(destination))
            destinations.Add(destination); */

        // destinations = { 3: 2 } >> 2 ADD ONE passenger (+1)
        destinations[destination] = destinations.GetValueOrDefault(destination, 0) + 1;
    }
    public void Step()
    {
        // 1- Als de deur open is 
        if (doorsOpen)
        {
            doorsOpen = false;
            return; // Ga naar buiten (Stap)
        }
        // 2- Als we nu op de gewenste verdieping zijn, openen we de deur.
        if (calls.Contains(currentFloor) || destinations.ContainsKey(currentFloor))
        {
            calls.Remove(currentFloor);
            if (destinations.ContainsKey(currentFloor))
            {
                passengers -= destinations[currentFloor];
                destinations.Remove(currentFloor);
                /*
                if (passengers > 0)
                    //  passengers = -1 >> Dat mag niet
                    passengers--;*/
            }
            doorsOpen = true;
            direction = Direction.Idle;
            return; // Ga naar buiten (uitvoer)
        }

        // 3- Als er geen werk is
        if (calls.Count == 0 && destinations.Count == 0)
        {
            direction = Direction.Idle;
            return;
        }

        // 4- Beweeg een stap in de richting van het dichtstbijzijnde verzoek

        /* this.nextStop = calls.Concat(destinations.Keys)
            .OrderBy(f => Math.Abs(f - currentFloor)) // |3-8| = 5
            .First(); */

        List<int> allStops = calls.Concat(destinations.Keys).ToList();
        // calls = [1, 3]... destinations = { 4:2, 6:1 }.... destinations.Keys = [4, 6] ----> allStops = [1, 3, 4, 6]

        var stopsAhead = direction == Direction.Up
            ? allStops.Where(floor => floor > currentFloor) // UP
            : allStops.Where(floor => floor < currentFloor); // Down

        this.nextStop = stopsAhead.Any()
            ? (direction == Direction.Up ? stopsAhead.Min() : stopsAhead.Max())
            // Up => Min >> Zoals [4,5] Ik ben in FLOOR (3) Dus ga naar 4 |> [4,5].Min() => 4
            // Down => Max >> Zoals [1,3] Ik ben in FLOOR (5) Dus ga naar 3 |> [1,3].Max() => 3
            : allStops.OrderBy(floor => Math.Abs(floor - currentFloor)).First();


        if (nextStop > currentFloor)
        {
            direction = Direction.Up;
            currentFloor++; // Eeeeeeeeeeeen stap
        }
        else if (nextStop < currentFloor)
        {
            direction = Direction.Down;
            currentFloor--;
        }

        // 5- Nadat we de verplaatsing hebben voltooid en de gewenste verdieping hebben bereikt, openen we de deur direct.
        if (calls.Contains(currentFloor) || destinations.ContainsKey(currentFloor))
        {
            calls.Remove(currentFloor);
            if (destinations.ContainsKey(currentFloor))
            {
                passengers -= destinations[currentFloor];
                destinations.Remove(currentFloor);
                /*
                if (passengers > 0)
                    passengers--;
                    */
            }
            doorsOpen = true;
            direction = Direction.Idle;
        }
    }
    public ElevatorStatus Status()
    {
        return new ElevatorStatus( // Zoals opject in js 
            currentFloor,
            direction,
            doorsOpen,
            passengers
        );
    }
}