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