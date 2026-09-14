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

Instead of shrinking individual input values, QuickCheckr shrinks *behaviour*:
sequences of inputs, actions, branching decisions, and state transitions.
Failures are treated as things that *emerge over time*, not necessarily as the result of a single call.

This comes with a non-trivial learning curve.
To make shrinking meaningful, all state under test must be encapsulated and described explicitly.
QuickCheckr uses `LINQ` composition to express this, which is familiar to most C# developers, but the idea of *stateful LINQ* may feel unusual at first.

The payoff is that bugs which are invisible to traditional property-based tests, because no single operation fails in isolation, become both detectable *and explainable*.

Below is a small, deliberately contrived example.
A more realistic, end-to-end [scenario](https://github.com/kilfour/QuickCheckr/blob/main/Docs/A_Guide/E_HorsesAgain/BoltingHorses.md) can be found at the end of the guide,
but it is strongly recommended to start at the [beginning](https://github.com/kilfour/QuickCheckr/blob/main/Docs/A_Guide/A_CheckringThingsOut/CheckringThingsOut.md) and follow it step by step.

## Example
The following example demonstrates a class of bug that is easy to miss in traditional tests and difficult to express using classic property-based testing.

No single call fails.  
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
## Highlights

* **LINQ-based workflow:** Build tests using familiar query expressions.
* **Shrinking:** Reduce randomly generated scenarios to minimal failing cases.
* **Stateful testing:** Pools, stashed objects, and multi-step behaviours supported out of the box.
* **Deterministic:** Reproducible runs using seeds.
* **Built for Devs:** Transparency and traceability as a first-class concern.
  
## Installation

QuickCheckr is available on NuGet:
```bash
Install-Package QuickCheckr
```
Or via the .NET CLI:
```bash
dotnet add package QuickCheckr
```  
## Documentation

QuickCheckr is in the process of being fully documented, with real, executable examples for every feature,
and every statement in the docs is backed by a test.

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
