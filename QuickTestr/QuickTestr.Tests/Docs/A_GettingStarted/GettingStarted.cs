using QuickPulse.Explains;

namespace QuickTestr.Tests.Docs.A_GettingStarted;

[DocFile]
[DocFileHeader("Getting Started")]
[DocContent("""
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
""")]
public class GettingStarted;
