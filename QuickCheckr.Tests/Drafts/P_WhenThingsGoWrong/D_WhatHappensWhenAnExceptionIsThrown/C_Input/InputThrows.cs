using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickFuzzr;
using QuickFuzzr.UnderTheHood.WhenThingsGoWrong;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.P_WhenThingsGoWrong.D_WhatHappensWhenAnExceptionIsThrown.C_Input;


[DocFile]
[DocContent("Whenever an input throws, the exception is wrapped into a `InputFuzzrThrewException`.")]
public class InputThrows
{
    [Fact]
    [DocContent("The Checkr below will throw a QuickFuzzr `OneOfEmptyOptionsException`:")]
    [DocExample(typeof(InputThrows), nameof(IfItThrowsCheckr))]
    [DocExample(typeof(InputThrows), nameof(IfItThrowsMessage))]
    [CodeSnippet]
    public void IfItThrows()
    {
        var ex = Assert.Throws<InputFuzzrThrewException>(() => IfItThrowsCheckr().Run());
        Assert.Equal(IfItThrowsMessage().Replace("\r\n", "\n").Replace("\n", Environment.NewLine), ex.Message);
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
    [DocExample(typeof(InputThrows), nameof(LazyUsageExample))]
    [CodeSnippet]
    public void LazyUsageExample()
    {
        Checkr.Input("AnInt", () => Fuzzr.Int());
    }

    [Fact]
    [DocContent("If the *lazy* version throws:")]
    [DocExample(typeof(InputThrows), nameof(IfLazyThrowsCheckr))]
    [DocContent("The Exception is **not** wrapped.")]
    [DocExample(typeof(InputThrows), nameof(IfLazyThrowsMessage))]
    [CodeSnippet]
    public void IfLazyThrows()
    {
        var ex = Assert.Throws<OneOfEmptyOptionsException>(() => IfLazyThrowsCheckr().Run());
        // Assert.Equal(IfLazyThrowsMessage().Replace("\r\n", "\n").Replace("\n", Environment.NewLine), ex.Message);
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