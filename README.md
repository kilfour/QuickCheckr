# <img src='icon.png' width='40' align='top'/> QuickCheckr
> **CSI: .NET**

Property-based testing for the pragmatic.  

[![Docs](https://img.shields.io/badge/docs-QuickCheckr-blue?style=flat-square&logo=readthedocs)](https://github.com/kilfour/QuickCheckr/blob/main/Docs/A_Guide/Guide.md)
[![NuGet](https://img.shields.io/nuget/v/QuickCheckr.svg?style=flat-square&logo=nuget)](https://www.nuget.org/packages/QuickCheckr)
[![License: MIT](https://img.shields.io/badge/license-MIT-success?style=flat-square)](https://github.com/kilfour/QuickCheckr/blob/main/LICENSE)  
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
from spec in Checkr.Spec("Returns true", () => output)
select Case.Closed;
```
**Reports:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                CreateReadMe.cs:26:1
 Original failing run:    85 executions
 Minimal failing case:    5 executions (after 82 shrinks)
 Seed:                    1141724745
 ------------------------------------------------------------
  Executed: BugHouse.Run (3 Times)
   - Input: number = 6
 ------------------------------------------------------------
  Executed: BugHouse.Run (2 Times)
 ===========================================================================
  !! Exception Thrown: System.Exception: Here's Johnny
   at QuickCheckr.Tests.CreateReadMe.BugHouse.Run(Int32 number) in CreateReadMe.cs:line 69
   at QuickCheckr.Tests.CreateReadMe.<>c__DisplayClass2_0.GetCheckr() in CreateReadMe.cs:line 57
   at QuickCheckr.Checkr.<>c__DisplayClass8_0`1.TryCatch(CheckrState state) in CheckrAct.cs:line 74
 ===========================================================================
 Passed Specs
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

- **Start here:** [Checkr-ing Things Out](https://github.com/kilfour/QuickCheckr/blob/main/Docs/A_Guide/A_CheckringThingsOut/CheckringThingsOut.md)
- **Then see it catch a real bug:** [Bolting Horses](https://github.com/kilfour/QuickCheckr/blob/main/Docs/A_Guide/E_HorsesAgain/BoltingHorses.md)

**[Full Guide](https://github.com/kilfour/QuickCheckr/blob/main/Docs/A_Guide/Guide.md):**
This is a work in progess. Follow the link to find out what's coming next.
  
## Roadmap

- Elm-style helpful exceptions.
- Full Guide + Reference documentation.
- Additional diagnostic flows.
- True async execution throughout the engine.
  
## Dependencies

* [QuickFuzzr](https://github.com/kilfour/QuickFuzzr/blob/main/README.md): For random input generation.
* [QuickPulse.Show](https://github.com/kilfour/QuickPulse.Show/blob/main/README.md): For *stringifying* any C# object or value.
* [QuickPulse](https://github.com/kilfour/QuickPulse/blob/main/README.md): Implicit through QuickPulse.Show. But also used for diagnostics filtering and transformations.
  
## The Wonder(ing) Years
> [Why QuickCheckr Exists](https://github.com/kilfour/QuickCheckr/blob/main/why-quickcheckr-exists.md)  
## License

This project is licensed under the [MIT License](https://github.com/kilfour/QuickCheckr/blob/main/LICENSE).    
