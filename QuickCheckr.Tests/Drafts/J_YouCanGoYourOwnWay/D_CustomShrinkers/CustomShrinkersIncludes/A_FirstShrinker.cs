using QuickCheckr.Tests.Tools;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.D_CustomShrinkers.CustomShrinkersIncludes;

[DocFileHeader("A Simple Example")]
public class A_FirstShrinker : CheckrTest<A_FirstShrinker>
{
    protected override bool Asserts => false;
    protected override bool Report => false;

    [Fact]
    [DocCheckrHeader]
    [DocCheckr]
    [DocReportHeader]
    [DocReport]
    [DocHeader("Explaining the Example, TODO: REVIEW THIS TEST/DOC")]
    [DocContent(
@"The example above is intentionally simple. The expectation only passes for the value `42`,
which makes it easy to see how the custom shrinker influences the reported case.

QuickCheckr already provides built-in classifiers and reducers for common types such as `int`.
These default strategies work well in most situations, but they are necessarily generic: 
they do not know which values are meaningful in the context of a specific check.

By adding:

```csharp
s => s.Classify(Try.Values(42))
```

we tell QuickCheckr that `42` is a value worth trying explicitly during shrinking.

The reducer then continues simplifying the remaining failing value:

```csharp
s => s.Simplify(Reduce.TowardsZero)
```

In the report, the original failing value is `53`, while the final reported value becomes `43`.
This is useful because `43` is the closest failing value above the only passing value (`42`).

Although the expectation itself is artificial, the example illustrates the purpose of custom shrinkers:
they allow shrinking to be guided by values that are semantically meaningful for the domain, not just structurally smaller.
"
    )]
    public override void Example() =>
        Run(1895340861);

    [CodeSnippet]
    protected override CheckrOf<Case> TheCheckr() =>
        from answers in Checkr.Input("Answer", Fuzzr.Int(),
            Shrink.OnType<int>(
                s => s.Classify(Try.Values(42)),
                s => s.Simplify(Reduce.TowardsZero)))
        from expect in Checkr.Expect("Answer equals 42", () => answers == 42)
        select Case.Closed;

    protected override void Verify(Article article)
    {
        Assert.Equal("Answer equals 42", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(1, article.ShrinkCount);
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Answer", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("53", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("0", article.Execution(1).Input(1).Read().Redux.Value);
    }
}
