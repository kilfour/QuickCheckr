using QuickCheckr.Tests.Tools;
using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickFuzzr;
using QuickFuzzr.UnderTheHood.WhenThingsGoWrong;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.K_CheckrsCourt.A_TheCheckrClass.InputTests;


[DocFile]
[DocContent(
@"`Checkr.Input()` is the only way an *input* can be defined inside a Checkr.  
Inputs serve as shrinkable parameters that in all likelyhood will be consumed by an Act Checkr down the line.")]
public class InputTests
{
    [Fact]
    [DocUsage]
    [DocExample(typeof(InputTests), nameof(UsageExample))]
    [CodeSnippet]
    public void UsageExample()
    {
        Checkr.Input("AnInt", Fuzzr.Int());
    }

    [Fact]
    [DocHeader("Exception Handling", 1)]
    [DocContent("The Checkr below will throw a QuickFuzzr `OneOfEmptyOptionsException`:")]
    [DocExample(typeof(InputTests), nameof(IfItThrowsCheckr))]
    [DocContent("Whenever an input throws, the exception is wrapped into a `InputFuzzrThrewException`.")]
    [DocExample(typeof(InputTests), nameof(IfItThrowsMessage))]
    [CodeSnippet]
    public void IfItThrows()
    {
        var ex = Assert.Throws<InputFuzzrThrewException>(() => IfItThrowsCheckr().Run());
        Assert.Equal(IfItThrowsMessage(), ex.Message);
        Assert.IsType<OneOfEmptyOptionsException>(ex.InnerException);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> IfItThrowsCheckr()
    {
        return
            from input in Checkr.Input("AnInt", Fuzzr.OneOf<int>((List<int>)[]))
            select Case.Closed;
    }

    [CodeSnippet]
    [CodeRemove("@\"")]
    [CodeRemove("\";")]
    private static string IfItThrowsMessage() =>
@"Input 'AnInt' threw a 'OneOfEmptyOptionsException', see InnerException.
This might mean something is wrong with the way you wrote the Fuzzr.
But it might also mean that the fuzzr is depending on state.
And as 'Input' is eagerly evaluated, this can cause problems.

In the latter case please try using the 'lazy' version. 

Example: 'Checkr.Input(() => Fuzzr.Int()).
";

    [Fact]
    [DocContent(
@"As you can gather from the exception message, this is done in order to inform the user of a rare, but real,
issue when a Fuzzr depends on external state.  
The solution is also supplied in said message: ")]
    [DocExample(typeof(InputTests), nameof(LazyUsageExample))]
    [CodeSnippet]
    public void LazyUsageExample()
    {
        Checkr.Input("AnInt", () => Fuzzr.Int());
    }

    [Fact]
    [DocContent("If the *lazy* version throws:")]
    [DocExample(typeof(InputTests), nameof(IfLazyThrowsCheckr))]
    [DocContent("The Exception is **not** wrapped.")]
    [DocExample(typeof(InputTests), nameof(IfLazyThrowsMessage))]
    [CodeSnippet]
    public void IfLazyThrows()
    {
        var ex = Assert.Throws<OneOfEmptyOptionsException>(() => IfLazyThrowsCheckr().Run());
        Assert.Equal(IfLazyThrowsMessage(), ex.Message);
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> IfLazyThrowsCheckr()
    {
        return
            from input in Checkr.Input("AnInt", () => Fuzzr.OneOf<int>((List<int>)[]))
            select Case.Closed;
    }

    [CodeSnippet]
    [CodeRemove("@\"")]
    [CodeRemove("\";")]
    private static string IfLazyThrowsMessage() =>
@"Fuzzr.OneOf<Int32> cannot select from an empty sequence.

Possible solutions:
- Provide at least one option (ensure the sequence is non-empty).
- Use a fallback: Fuzzr.OneOf(values).WithDefault()
- Guard upstream: values.Any() ? Fuzzr.OneOf(values) : Fuzzr.Constant(default!).
";
}