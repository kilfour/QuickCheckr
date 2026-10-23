# <img src='icon.png' width='40' align='top'/> QuickCheckr
> **CSI: .NET**

Property-based testing for the pragmatic.  

[![Docs](https://img.shields.io/badge/docs-QuickCheckr-blue?style=flat-square&logo=readthedocs)](https://github.com/kilfour/QuickCheckr/blob/main/Docs/A_Guide/Guide.md)
[![NuGet](https://img.shields.io/nuget/v/QuickCheckr.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/QuickCheckr)
[![License: MIT](https://img.shields.io/badge/license-MIT-success?style=flat-square)](https://github.com/kilfour/QuickCheckr/blob/main/LICENSE)

**QuickCheckr** is inspired by Haskell's QuickCheck, but it is not a straight-up clone.  
It borrows the *ideas* of property-based testing, not the algorithms, and was built from the ground up for the realities of day-to-day .NET code.

If you are testing *pure* functions and are primarily interested in mathematically minimal counterexamples,
there are excellent tools available, FsCheck being the obvious choice, even if it can feel slightly unwieldy in C#.

But most C# code is not pure.

> QuickCheckr is all about state and side-effects, because most C# code is all about state and side-effects.

QuickCheckr shrinks both individual input values and *behaviour*:
sequences of inputs, actions, branching decisions, and state transitions.
Failures are treated as things that *emerge over time*, not necessarily as the result of a single call.

This comes with a non-trivial learning curve.
To make replay and shrinking meaningful, keep state under test in stashed objects or tracked pools, and express operations through the Checkr workflow.
QuickCheckr uses `LINQ` composition to express this, which is familiar to most C# developers, but the idea of *stateful LINQ* may feel unusual at first.

The payoff is that bugs which depend on a sequence of operations become both detectable *and explainable*.

Below is a small, deliberately contrived example.
A more realistic, end-to-end [scenario](https://github.com/kilfour/QuickCheckr/blob/main/Docs/A_Guide/E_HorsesAgain/BoltingHorses.md) can be found at the end of the guide,
but it is strongly recommended to start at the [beginning](https://github.com/kilfour/QuickCheckr/blob/main/Docs/A_Guide/A_CheckringThingsOutChapter/A_CheckringThingsOut.md) and follow it step by step.

## Example
The following example demonstrates a class of bug that is easy to miss in traditional tests and difficult to express using classic property-based testing.

A call on a fresh instance succeeds.
The failure only occurs after a *specific sequence* of operations has been performed on the same instance.  
QuickCheckr is designed to explore and shrink exactly this kind of behaviour.  

  
**Buggy Code:**  
```csharp
public class BugHouse
{
    private int count;

    public bool Run(int number)
    {
        if (count == 5) throw new Exception("Here's Johnny");
        if (number == 6 && count != 3) count++;
        if (count >= 3) count++;
        return true;
    }
}
```
**The Checkr:**  
```csharp
from bughouse in Trackr.Stashed(() => new BugHouse())
from number in Checkr.Input("number", Fuzzr.Int())
from output in Checkr.Act("BugHouse.Run", () => bughouse.Run(number))
from expect in Checkr.Expect("Returns true", () => output)
select Case.Closed;
```
**Reports:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                CreateReadMe.cs:27:1
 Original failing run:    85 executions
 Minimal failing case:    5 executions (after 82 shrinks)
 Seed:                    1141724745
 ------------------------------------------------------------
  Executed: BugHouse.Run (3 Times)
   - Input: number = 6
 ------------------------------------------------------------
  Executed: BugHouse.Run (2 Times)
   - WARNING: All inputs were considered irrelevant.
 ===========================================================================
  !! Exception: Here's Johnny
 ===========================================================================
 Passed Expectations
 - Returns true: 84x
 ------------------------------------------------------------
```
## Running the example

Add these namespaces to your test file:

```csharp
using QuickCheckr;
using QuickFuzzr;
```

Assign the query above to a variable named `checkr`, then run it:

```csharp
checkr.Run(100.Runs(), 100.ExecutionsPerRun());
```

Each run starts with fresh stashed state. Within a run, executions reuse the same
`BugHouse` instance while generating new inputs. A failure triggers shrinking and
a report; the runner throws so your test framework marks the test as failed.

To reproduce the failure shown above, use its seed and execution limit:

```csharp
checkr.Run(1141724745, 100.ExecutionsPerRun());
```

Use `await checkr.RunAsync(...)` for workflows with async callbacks.
A bare `Run()` performs just one run with one execution, so specify an execution
count when testing behaviour that builds up over time.

## Highlights

* **LINQ-based workflow:** Build tests using familiar query expressions.
* **Shrinking:** Reduce failing sequences and input values, with custom strategies through `Shrinkr.For<T>()`.
* **Stateful testing:** Reuse objects with `Trackr.Stashed`, manage collections with `Poolr`, and explore multi-step behaviours.
* **Async support:** Await actions and expectations through execution, replay, and shrinking with `RunAsync(...)` and cancellation support.
* **Deterministic:** Reproduce failures using the seed in the report.
* **Built for Devs:** Transparency and traceability as a first-class concern.
  
## Installation

QuickCheckr targets .NET 8 and .NET 10 and is available on NuGet.

In the Visual Studio Package Manager Console:
```powershell
Install-Package QuickCheckr
```
Or via the .NET CLI:
```bash
dotnet add package QuickCheckr
```  
## Documentation

The guide introduces inputs, actions, expectations, branching, stashed state, and pools through executable examples backed by tests.

* **[Index](https://github.com/kilfour/QuickCheckr/blob/main/Docs/ToC.md)**
* **[The Guide](https://github.com/kilfour/QuickCheckr/blob/main/Docs/A_Guide/Guide.md)**
  
## Dependencies

* [QuickFuzzr](https://github.com/kilfour/QuickFuzzr/blob/main/README.md): For random input generation.
* [QuickPulse.Show](https://github.com/kilfour/QuickPulse.Show/blob/main/README.md): For *stringifying* any C# object or value.
* [QuickPulse](https://github.com/kilfour/QuickPulse/blob/main/README.md): Implicit through QuickPulse.Show. But also used for diagnostics filtering and transformations.
  
## The Wonder(ing) Years
> [Why QuickCheckr Exists](https://github.com/kilfour/QuickCheckr/blob/main/why-quickcheckr-exists.md)  
## License

This project is licensed under the [MIT License](https://github.com/kilfour/QuickCheckr/blob/main/LICENSE).    
