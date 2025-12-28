using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickFuzzr;
using QuickFuzzr.UnderTheHood.WhenThingsGoWrong;
using QuickPulse.Explains;
using QuickPulse.Instruments;


namespace QuickCheckr.Tests.Drafts.D_CheckrsCourt.A_TheCheckrClass.Methods;

[DocFile]
[DocContent(
@"`Checkr.Input()` is the only way an *input* can be defined inside a Checkr.  
Inputs serve as shrinkable parameters that in all likelyhood will be consumed by an Act Checkr down the line.")]
public class A_Input : BaseTest
{
    [Fact]
    [DocContent("Example:")]
    [DocExample(typeof(A_Input), nameof(UsageExample))]
    [CodeSnippet]
    public void UsageExample()
    {
        Checkr.Input("AnInt", Fuzzr.Int());
    }

    [Fact]
    [DocContent("If it throws:")]
    [DocExample(typeof(A_Input), nameof(IfItThrowsCheckr))]
    [DocContent("Exception is wrapped:")]
    [DocExample(typeof(A_Input), nameof(IfItThrowsMessage))]
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
    [CodeRemove("return")]
    private static string IfItThrowsMessage()
    {
        return
@$"Input 'AnInt' threw a 'OneOfEmptyOptionsException', see InnerException.
This might mean something is wrong with the way you wrote the Fuzzr.
But it might also mean that the fuzzr is depending on state, and as 'Input' is eagerly evaluated, this can cause problems.
In the latter case please try using the 'lazy' version, f.i.: 'Checkr.Input(() => Fuzzr.Int()).
";
    }

    [Fact]
    [DocContent("Lazy Example:")]
    [DocExample(typeof(A_Input), nameof(LazyUsageExample))]
    [CodeSnippet]
    public void LazyUsageExample()
    {
        Checkr.Input("AnInt", () => Fuzzr.Int());
    }

    [Fact]
    [DocContent("If the lazy version throws:")]
    [DocExample(typeof(A_Input), nameof(IfLazyThrowsCheckr))]
    [DocContent("The Exception is **not** wrapped:")]
    [CodeSnippet]
    public void IfLazyThrows()
    {
        Assert.Throws<OneOfEmptyOptionsException>(() => IfLazyThrowsCheckr().Run());
    }

    [CodeSnippet]
    [CodeRemove("return")]
    private static CheckrOf<Case> IfLazyThrowsCheckr()
    {
        return
            from input in Checkr.Input("AnInt", () => Fuzzr.OneOf<int>((List<int>)[]))
            select Case.Closed;
    }
}