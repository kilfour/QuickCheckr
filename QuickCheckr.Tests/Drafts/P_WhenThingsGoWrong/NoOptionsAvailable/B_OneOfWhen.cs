using QuickCheckr.Authoring;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.P_WhenThingsGoWrong.NoOptionsAvailable;

[DocFile]
[DocFileHeader("OneOfWhen")]
[DocContent("Throws a `NoCheckrsAvailableException` when no Checkr can be chosen.")]
[DocBoldHeader("Message:")]
[DocExample(typeof(B_OneOfWhen), nameof(Message))]
public class B_OneOfWhen : DocAttributeBaseTest
{
    [Fact]
    public void NoOptionsThrows()
    {
        var ex = Assert.Throws<NoCheckrsAvailableException>(() => Checkr.OneOfWhen(Checkr.Option(() => false, s => CheckrResult.CaseOnly(s))).Run());
        Assert.Equal(Message(), ex.Message);
    }

    [CodeSnippet]
    private static string Message() =>
@"Checkr.OneOfWhen could not choose a Checkr because none of the conditions evaluated to true.

OneOfWhen needs at least one available option whenever it runs.
Check the conditions passed to Checkr.Option, or provide a fallback option whose condition evaluates to true.";
}
