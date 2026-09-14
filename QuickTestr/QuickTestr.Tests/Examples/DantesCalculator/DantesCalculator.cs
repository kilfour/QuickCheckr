using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Authoring.ThePress;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickTestr.Tests.Tools;

namespace QuickTestr.Tests.Examples.DantesCalculator;

[DocFile]
[DocFileHeader("Dante's Calculator")]
public class DantesCalculator : QuickTestrOracleTest<DantesCalculator>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocContent(
"""
This is an example of how oracle-based testing can be used to deal with legacy code.  
It is taken from a course I gave in the past.

Some students mentioned it wasn't an exercise, ... it was a crime.
""")]
    [DocExample(typeof(Calculator))]
    [DocTestrHeader]
    [DocTestr]
    [DocFuzzrHeader]
    [DocExample(typeof(ItemFuzzr))]
    [DocContent(
"""
This approach quickly drives the implementation through a very large
range of behavioural combinations, giving us confidence to refactor safely.

For instance:
""")]
    [DocExample(typeof(CalculatorNew))]
    [DocContent(
"""
For completeness, let me just note that the same thing can be achieved using `Theory` and `InlineData`.

I have included an example in this repository [here](https://github.com/kilfour/QuickTestr/blob/main/QuickTestr.Tests/Examples/DantesCalculator/CalculatorCharacterizationTests.cs).

The `Theory` version works, but it scales poorly.

Every new business rule increases the number of combinations we should test,
which quickly turns into an explosion of hand-written examples.

With property-based testing we describe the *shape* of valid data once,
and the engine continuously explores new combinations automatically.

That makes it particularly effective for legacy systems with many interacting rules,
where the real bugs tend to hide in unexpected edge-case combinations.
""")]
    public override void Example() => Document();

    [CodeSnippet]
    [CodeRemoveJournalist]
    protected override void GetTestr(Journalist journalist) =>
        Testr.Named("Calculator Oracle")
            .For(ItemFuzzr.Get.Many(1, 20).ToList())
            .Expected(Calculator.Total)
            .Actual(CalculatorNew.Total)
            .StoreCaseFiles(journalist)
            .Run();

    protected override void Verify(Article article)
    {
        Assert.Equal("", article.FailureDescription());
        Assert.Equal(1, article.Total().PassedExpectations());
        Assert.Equal("Calculator Oracle", article.PassedExpectation(1).Read().Label);
        Assert.Equal(100, article.PassedExpectation(1).Read().TimesPassed);
    }
}




