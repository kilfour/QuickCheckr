# QuickTestr
Start with [Getting Started][GettingStarted], then choose the style that fits the
thing you want to verify:

- [Property-based][PropertyBased] : Define what should always hold.
- [Oracle-based][OracleBased]: Compare against something that already works.
- [Model-based][ModelBased]: Compare state transitions against a model.  

[GettingStarted]: #getting-started

[PropertyBased]: #property-based-style-testing

[OracleBased]: #oracle-based-style-testing

[ModelBased]: #model-based-testing
## Getting Started
QuickTestr checks a claim against many generated inputs. When it finds a counterexample,
it tries to reduce that input to something smaller and reports both the failure and the
seed needed to reproduce it.

### Install

Add QuickTestr to your test project:

```bash
dotnet add package QuickTestr
```

The examples below use:

```csharp
using QuickFuzzr;
using QuickTestr;
```

QuickTestr works inside an ordinary test method. It does not require a custom test
runner: when a claim is falsified, `Run()` throws and your existing test framework
records the failure.  
### Your First Testr
Suppose this implementation of `Double` contains a boundary bug:

```csharp
static int Double(int value) => value > 42 ? 0 : value * 2;
```

We can describe what doubling should always mean and let QuickTestr search for an
input that disproves it.  

**The Testr:**  
```csharp
Testr.Named("Doubling matches addition")
    .For(Fuzzr.Int(0, 100))
    .Assert(value => Double(value) == value + value)
    .Run();
```
The chain reads from top to bottom:

- `Named` labels the claim in the report.
- `For` supplies generated inputs. Here they are integers from 0 through 100.
- `Assert` describes the property that should hold for every generated input.
- `Run` performs the search.

This test fails because the implementation disagrees with the property above 42.
QuickTestr then reduces the failing value to the boundary where the bug begins.  

**The Report:**  
```text
------------------------------------------------------------
  Doubling matches addition
  Seed: 1471595869
 ------------------------------------------------------------
  Falsified:
    Input = 94
    Redux = 43
 ------------------------------------------------------------
```

[PropertyBased]: #property-based-style-testing

[OracleBased]: #oracle-based-style-testing

[ModelBased]: #model-based-testing
### Reading the Report
The report separates three useful values:

- **Input** is the failing value after structural reduction.
- **Redux** is the value-reduced form, when value reduction can make the failure
  clearer.
- **Original** appears when structural reduction changed the input and preserves
  the generated value that first exposed the problem.

The seed identifies the random run. Pass it to `Run(seed)` while investigating a
failure:

```csharp
.Run(1471595869);
```

Once the problem is fixed, return to `Run()` so later test runs continue exploring
new inputs.

#### The Basic Shape

Most QuickTestr properties follow the same four-part shape:

```csharp
Testr.Named("A useful description")
    .For(inputGenerator)
    .Assert(input => claim)
    .Run();
```

Generation is provided by
[QuickFuzzr](https://github.com/kilfour/QuickFuzzr/blob/main/README.md).
Generators compose, so inputs can range from a single integer to collections and
domain-specific objects.

From here:

- Use [property-based testing][PropertyBased] when you can state what must always
  be true.
- Use [oracle-based testing][OracleBased] when you can compare with a trusted
  implementation.
- Use [model-based testing][ModelBased] when correctness depends on a sequence of state
  transitions.  
## Property-based Style Testing
In property-based testing you describe **what should always be true**, regardless of the input.  
QuickTestr generates many inputs and tries to falsify your rule, shrinking failures to a minimal example.

A classic to begin with:  
### Reversing a List
####  
```csharp
Testr.Named("Reverse is its own inverse")              // The name of the property.
    .For(Fuzzr.Int().Many(1, 10))                      // The input Fuzzr.
    .Assert(s => Reverse(Reverse(s)).SequenceEqual(s)) // The property to assert.
    .Run();                                            // Run the test.
// --------------------------------------------------------------------------------
static IEnumerable<int> Reverse(IEnumerable<int> l) => [.. l.Reverse()];
```
####  
That passes, which isn't very interesting.
Let's break it by moving all `42`s to the end.  
```csharp
Testr.Named("Reverse is its own inverse")
    .For(Fuzzr.Int().Many(1, 10))
    .Assert(s => Reverse(Reverse(s)).SequenceEqual(s))
    .Run();
// ---------------------------------------------------------------------
static IEnumerable<int> Reverse(IEnumerable<int> l) => [.. HideTheAnswer(l.Reverse())];
static IEnumerable<int> HideTheAnswer(IEnumerable<int> l)
    => l.Where(a => a != 42).Concat(l.Where(a => a == 42));
```

**The Report:**  
```text
------------------------------------------------------------
  Reverse is its own inverse
  Seed: 174616483
 ------------------------------------------------------------
  Falsified:
    Input = [ 42, _ ]

  Original:
    [ 86, 33, 42, 21, 7, 62, 44, 10 ]
 ------------------------------------------------------------
```
### Moving On
Now for something less trivial, one of the `jqwik` challenges.  

The property we are testing states:  
> No two different elements point to each other when used as indexes into the list.  

**The Testr:**  
```csharp
Testr
    .Named("No two different indexes point to each other.")
    .DisableValueReduction()
    .For(
        from length in Fuzzr.Int()
        from enumerable in Fuzzr.Int(0, length - 1).Many(length)
        select enumerable.ToList())
    .Deliberate(a => a.Count, 2)
    .Assert(list => list.All(
        element =>
        {
            int index = list[element];
            if (index != element && list[index] == element)
                return false;
            return true;
        }))
    .Run();
```
**Some Notes:**
- `DisableValueReduction()`: By default QuickTestr tries to move ints towards zero, but here that isn't necessary.
- `Deliberate(a => a.Count, 2)`: This is a *Deliberation Policy*. It helps escape local minima during shrinking. In this case, we're looking for smaller lists.
- The compositional abilities of QuickFuzzr allow us to generate only valid indexes, keeping inputs structurally sound.  

**The Report:**  
```text
------------------------------------------------------------
  No two different indexes point to each other.
  Seed: 1934478623
 ------------------------------------------------------------
  Falsified:
    Input = [ 1, 0 ]

  Original:
    [ 1, 1, 0, 4, 3, 3, 2, 3, 11, 6, 4, 4, 4, 12 ]
 ------------------------------------------------------------
```
## Oracle-based Style Testing
The second style of QuickTestr verification.

Still stateless, still one invariant, but now we compare against something that already works: an oracle.

This is a very viable option in many cases. Refactoring legacy code for example.
Or in my case, verifying student code against a golden model.  
### First Example
Starting out with a toy example once again, assume we have the following working method:  
```csharp
public int AddCorrect(int a, int b) => a + b;
```
And we want to compare it against a newer implementation:  
```csharp
public int AddBuggy(int a, int b) => a > 42 ? 0 : a + b;
```
This implementation silently breaks when `a > 42`.  

**The Testr:**  
```csharp
Testr.Named("AddBuggy Matches AddCorrect")
    .For(Fuzzr.Tuple(Fuzzr.Int(), Fuzzr.Int()))
    .Expected(a => AddCorrect(a.Item1, a.Item2))
    .Actual(a => AddBuggy(a.Item1, a.Item2))
    .Run();
```

**The Report:**  
```text
------------------------------------------------------------
  AddBuggy Matches AddCorrect
  Seed: 1471595869
 ------------------------------------------------------------
  Falsified:

    Input = ( 94, _ )
      Observed:
        Expected = 129
        Actual   = 0

    Redux = ( 43, _ )
      Observed:
        Expected = 78
        Actual   = 0

  Original:
    ( 94, 35 )
 ------------------------------------------------------------
```
### An Interpreter

**The Testr:**  
```csharp
Testr
    .Named("Interpreter matches golden model.")
    .For(ExpressionFuzzr.GetExpression(),
        Shrink.OnType<string>(
            s => s.Simplify(
                Remove.WhiteSpace,
                Remove.BalancedParentheses,
                Remove.LeftHandSideAndOperator,
                Remove.UnaryMinusNoise)))
    .Deliberate(a => a.Length, 4) // prefer smaller length strings
    .Expected(a => LostIn.Translation(a).Eval())
    .Actual(a => LostIn.FaultyTranslation(a).Eval())
    .Run();
```

**The Report:**  
```text
------------------------------------------------------------
  Interpreter matches golden model.
  Seed: 443608219
 ------------------------------------------------------------
  Falsified:

    Input = "1 ^ 1 ^ - 3 ^ 1 / - 1 ^ (2 / 2 + 1 - 3 / 3 / 3) * 3 - 1 ^ (2 - 2 / 1 + 2 / 1 / 3) + 3 / (1 + 3 + 2 * 3) / 2 * - 2 ^ 2"
      Observed:
        Expected = -4.6
        Actual   = NaN

    Redux = "-2^2"
      Observed:
        Expected = -4
        Actual   = 4

  WARNING: Reducer stopped at Input. Max steps (1024) exceeded.
 ------------------------------------------------------------
```

**Domain Aware String Reduction Rules:**  
```csharp
public class Remove
{
    public static readonly SelectrOf<Selection> WhiteSpace =
        Stringr.AtleastOnce(char.IsWhiteSpace).Remove().Defined();

    public static readonly SelectrOf<Selection> BalancedParentheses =
        Stringr.Balanced('(', ')').Replace(a => a[1..^1]).Defined();

    public static readonly SelectrOf<Selection> LeftHandSideAndOperator =
        from lhs in Stringr.AtleastOnce(char.IsDigit).Remove()
        from op in Stringr.Once('+', '-', '*', '/', '^').Remove()
        select Selection.Defined;

    public static readonly SelectrOf<Selection> UnaryMinusNoise =
        Stringr.AtleastOnce('-').Replace(_ => "-").Defined();
}
```
## Model-based Testing
The third supported style of QuickTestr verification.

Here we introduce a model that tracks state alongside the real system.

Similar to Oracle-based testing but now behavior depends on sequences of operations, not just isolated inputs.
  
### Calculator Memory

**The Model:**  
```csharp
public class CalculatorModel
{
    public int Result { get; private set; } = 0;
    public void Add(int a) => Result += a;
    public void Subtract(int a) => Result -= a;
    public void Clear() => Result = 0;
}
```

**SUT:**  
```csharp
public class Calculator
{
    public int Result { get; private set; } = 0;
    private int counter = 0;
    public void Add(int a)
    {
        counter++;
        Result += a;
    }

    public void Subtract(int a) => Result -= a;
    public void Clear()
    {
        if (counter != 3)
            Result = 0;
    }
}
```

**The Testr:**  
```csharp
Testr.Named("Calculator Clear matches model")
    .Model(() => new CalculatorModel())
    .Sut(() => new Calculator())
    .Operation("Add", Fuzzr.Int(),
        (model, a) => model.Add(a),
        (sut, a) => sut.Add(a))
    .Operation("Subtract", Fuzzr.Int(),
        (model, a) => model.Subtract(a),
        (sut, a) => sut.Subtract(a))
    .Operation("Clear",
        model => model.Clear(),
        sut => sut.Clear())
    .Observe("Result Matches",
        (model, sut) => model.Result == sut.Result, a => a.Trace())
    .Run();
```

**The Report:**  
```text
------------------------------------------------------------
  Falsified after:         17 executions
  Minimal scenario:        4 executions
  Seed:                    1626335899
 ------------------------------------------------------------
  1. Add
  2. Add
  3. Add
  4. Clear
 ------------------------------------------------------------
  !! Failed: Result Matches

     Model: { Result: 0 }
     Sut:   { Result: 124 }
 ------------------------------------------------------------
```
### Rolodex

**The Model:**  
```csharp
public class RolodexOracle
{
    private readonly List<Person> people = [];

    public IReadOnlyList<Person> People => people;

    public void Add(string firstName, string lastName)
    {
        people.Add(new Person { FirstName = firstName, LastName = lastName });
    }

    public void DeleteAllByName(string name)
    {
        people.RemoveAll(person => person.FirstName == name || person.LastName == name);
    }
}
```

**SUT:**  
```csharp
public class Rolodex
{
    private readonly List<Person> people = [];

    public IReadOnlyList<Person> People => people;

    public void Add(string firstName, string lastName)
    {
        people.Add(new Person { FirstName = firstName, LastName = lastName });
    }

    public void DeleteAllByName(string name)
    {
        for (int i = 0; i < people.Count(); i++)
        {
            if (people[i].FirstName == name || people[i].LastName == name)
            {
                people.RemoveAt(i);
            }
        }
    }
}
```
```csharp
public class Person
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}
```

**The Testr:**  
```csharp
Testr.Named("Rolodex")
    .Model(() => new RolodexOracle())
    .Sut(() => new Rolodex())
    .Operation("Add", NameFuzzr,
        (model, a) => model.Add(a.First, a.Last),
        (sut, a) => sut.Add(a.First, a.Last))
    .Operation("Delete All By Name",
        model => model.People.Any(), // precondition for this operation
        model =>
            from names in Fuzzr.FromEach(model.People, a => Fuzzr.OneOf(a.FirstName, a.LastName))
            from name in Fuzzr.OneOf(names)
            select name,
        (model, a) => model.DeleteAllByName(a),
        (sut, a) => sut.DeleteAllByName(a))
    .Observe("People Match", PeopleMatch, a => a.Trace())
    .Run();
```

**The Report:**  
```text
------------------------------------------------------------
  Falsified after:         75 executions
  Minimal scenario:        3 executions
  Seed:                    322254521
 ------------------------------------------------------------
  1. Add
     ( "w", _ )
  2. Add
     ( _, "w" )
  3. Delete All By Name
     "w"
 ------------------------------------------------------------
  !! Failed: People Match

     Model: { People: [ ] }
     Sut:   { People: [ { FirstName: "pdqhcix", LastName: "w" } ] }
 ------------------------------------------------------------
```
### Checking The Exception
Operation exceptions do not fail the model test by themselves.  
They only matter if they lead to an observed state mismatch.  

**The Model:**  
```csharp
public class NameCollectorModel
{
    private readonly List<string> names = [];
    public IReadOnlyList<string> Names => names;
    public void Add(string name)
    {
        if (!names.Contains(name))
            names.Add(name);
    }
}
```

**SUT:**  
```csharp
public class NameCollector
{
    private readonly List<string> names = [];
    public IReadOnlyList<string> Names => names;
    public void Add(string name)
    {
        if (names.Contains(name))
            ComputerSays.No("Already have that one ...");
        names.Add(name);
    }
}
```

**The Testr:**  
```csharp
Testr.Named("NameCollector matches model")
    .Model(() => new NameCollectorModel())
    .Sut(() => new NameCollector())
    .Operation("Add", Fuzzr.String(1),
        (model, a) => model.Add(a),
        (sut, a) => sut.Add(a))
    .Observe("Result Matches",
        (model, sut) => model.Names.SequenceEqual(sut.Names), a => a.Trace())
    .Run();
```

**The Report:**  
```text
------------------------------------------------------------
 10 Runs
 ------------------------------------------------------------
 Passed Expectations
 - Result Matches: 500x
 ------------------------------------------------------------
```
### But I Care About The Exception
When activating *strict mode* a mismatch in exceptions do fail the model test.  

**The Model:**  
```csharp
public class NameCollectorModel
{
    private readonly List<string> names = [];
    public IReadOnlyList<string> Names => names;
    public void Add(string name)
    {
        if (!names.Contains(name))
            names.Add(name);
    }
}
```

**SUT:**  
```csharp
public class NameCollector
{
    private readonly List<string> names = [];
    public IReadOnlyList<string> Names => names;
    public void Add(string name)
    {
        if (names.Contains(name))
            ComputerSays.No("Already have that one ...");
        names.Add(name);
    }
}
```

**The Testr:**  
```csharp
Testr.Named("NameCollector matches model")
    .Model(() => new NameCollectorModel())
    .Sut(() => new NameCollector())
    .VerifyOperationResults()
    .Operation("Add", Fuzzr.String(1),
        (model, a) => model.Add(a),
        (sut, a) => sut.Add(a))
    .Observe("Result Matches",
        (model, sut) => model.Names.SequenceEqual(sut.Names), a => a.Trace())
    .Run();
```

**The Report:**  
```text
------------------------------------------------------------
  Falsified after:         5 executions
  Minimal scenario:        2 executions
  Seed:                    1830780673
 ------------------------------------------------------------
  1. Add
     "k"
  2. Add
     "k"
 ------------------------------------------------------------
  !! Failed: Add, results do not match

     Actual   ComputerSaysNo: Already have that one ...
 ------------------------------------------------------------
```
### Checking The Results As Well

**The Model:**  
```csharp
public class IdentityCounterModel
{
    public int Counter { get; private set; }
    public int Do(int a)
    {
        Counter++;
        return a;
    }
}
```

**SUT:**  
```csharp
public class IdentityCounter
{
    public int Counter { get; private set; }
    public int Do(int a)
    {
        Counter++;
        if (Counter > 3)
            return 0;
        return a;
    }
}
```

**The Testr:**  
```csharp
Testr.Named("IdentityCounter matches model")
    .Model(() => new IdentityCounterModel())
    .Sut(() => new IdentityCounter())
    .VerifyOperationResults()
    .Operation("Do", Fuzzr.Int(),
        (model, a) => model.Do(a),
        (sut, a) => sut.Do(a))
    .Observe("Counter Matches",
        (model, sut) => model.Counter == sut.Counter)
    .Run();
```

**The Report:**  
```text
------------------------------------------------------------
  Falsified after:         4 executions
  Minimal scenario:        4 executions
  Seed:                    520188124
 ------------------------------------------------------------
  1. Do
  2. Do
  3. Do
  4. Do
     78
 ------------------------------------------------------------
  !! Failed: Do, results do not match

     Expected 78
     Actual   0
 ------------------------------------------------------------
```
