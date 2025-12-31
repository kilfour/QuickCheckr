# <img src='icon.png' width='40' align='top'/> QuickCheckr
> **CSI: .NET**

Property-based testing for the pragmatic.  

[![Docs](https://img.shields.io/badge/docs-QuickCheckr-blue?style=flat-square&logo=readthedocs)](https://github.com/kilfour/QuickCheckr/blob/main/Docs/A_Guide/Guide.md)
[![NuGet](https://img.shields.io/nuget/v/QuickCheckr.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/QuickCheckr)
[![License: MIT](https://img.shields.io/badge/license-MIT-success?style=flat-square)](https://github.com/kilfour/QuickCheckr/blob/main/LICENSE)

**QuickCheckr** is inspired by Haskell's QuickCheck, and as such, it is a Property Based Testing framework.  
But it is not a straight-up clone. I could not write one of those anyway. I can't read the maths.  
Instead it took the ideas of QuickCheck, not the algorithms, and was built from the ground up,
iteratively, over a long period of time.

So if you are looking for a mathematically correct, powerfull shrinking algorithm, and are dealing with pure functional code,
there are better solutions out there. FsCheck comes to mind, even if it is slightly unwieldly in C#.  
But if you're dealing with the impure, messy world of day to day .NET code ...  

> QuickCheckr is all about state and side-effects, because most C# code is all about state and side-effects.

Now there *is* a non-trivial *learning-curve*. 
Because in order to get the shrinker to work, we have to encapsulate all state under test. QuickCheckr uses `LINQ` composition to achieve that, which is atleast familiar to most C# devs.  
But it does come with it's own dialect and the idea of *stateful LINQ* might feel strange at first sight.

All that said, below is a first, granted, contrived, example. A more [real world one](https://github.com/kilfour/QuickCheckr/blob/main/Docs/A_Guide/E_HorsesAgain/BoltingHorses.md) can be found at the end of the guide. But before you get to that it is advised to start at the [beginning](https://github.com/kilfour/QuickCheckr/blob/main/Docs/A_Guide/A_CheckringThingsOut/CheckringThingsOut.md) of said guide.
  
## Example
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
