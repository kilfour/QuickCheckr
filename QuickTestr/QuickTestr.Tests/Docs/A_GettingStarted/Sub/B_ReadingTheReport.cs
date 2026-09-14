using QuickPulse.Explains;

namespace QuickTestr.Tests.Docs.A_GettingStarted.Sub;

[DocFile]
[DocFileHeader("Reading the Report")]
[DocContent("""
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
""")]
public class B_ReadingTheReport;
