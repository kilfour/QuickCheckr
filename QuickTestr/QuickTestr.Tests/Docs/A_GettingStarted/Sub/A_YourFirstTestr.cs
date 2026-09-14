using QuickCheckr.Authoring.ThePress;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickTestr.Tests.Docs.A_PropertyBased;
using QuickTestr.Tests.Docs.B_OracleBased;
using QuickTestr.Tests.Docs.C_ModelBasedTesting;
using QuickTestr.Tests.Tools;

namespace QuickTestr.Tests.Docs.A_GettingStarted.Sub;

[DocFile]
[DocLink(typeof(PropertyBased))]
[DocLink(typeof(OracleBased))]
[DocLink(typeof(ModelBased))]
public class A_YourFirstTestr : QuickTestrPropertyTest<A_YourFirstTestr>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocContent("""
Suppose this implementation of `Double` contains a boundary bug:

```csharp
static int Double(int value) => value > 42 ? 0 : value * 2;
```

We can describe what doubling should always mean and let QuickTestr search for an
input that disproves it.
""")]
    [DocTestrHeader]
    [DocTestr]
    [DocContent("""
The chain reads from top to bottom:

- `Named` labels the claim in the report.
- `For` supplies generated inputs. Here they are integers from 0 through 100.
- `Assert` describes the property that should hold for every generated input.
- `Run` performs the search.

This test fails because the implementation disagrees with the property above 42.
QuickTestr then reduces the failing value to the boundary where the bug begins.
""")]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Document();

    [CodeSnippet]
    [CodeRemoveJournalist]
    [CodeRemove("1471595869")]
    protected override void GetTestr(Journalist journalist) =>
        Testr.Named("Doubling matches addition")
            .For(Fuzzr.Int(0, 100))
            .Assert(value => Double(value) == value + value)
            .StoreCaseFiles(journalist)
            .Run(1471595869);

    private static int Double(int value) => value > 42 ? 0 : value * 2;

    protected override void Verify(Article article)
    {
        Assert.Equal("Doubling matches addition", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Run", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("94", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("43", article.Execution(1).Input(1).Read().Redux.Value);
    }
}
