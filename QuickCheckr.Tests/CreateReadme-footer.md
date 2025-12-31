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