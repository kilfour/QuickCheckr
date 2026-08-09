using QuickCheckr.Authoring;
using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.P_WhenThingsGoWrong.NoOptionsAvailable;

[DocFile]
[DocFileHeader("OneOf")]
[DocContent("Throws a `NoCheckrsProvidedException` when no Checkrs are provided.")]
[DocBoldHeader("Message:")]
[DocExample(typeof(A_OneOf), nameof(Message))]
public class A_OneOf : DocAttributeBaseTest
{
    [Fact]
    public void NoOptionsThrows()
    {
        var ex = Assert.Throws<NoCheckrsProvidedException>(() => Checkr.OneOf().Run());
        Assert.Equal(Message(), ex.Message);
    }

    [CodeSnippet]
    private static string Message() =>
@"Checkr.OneOf could not choose a Checkr because no options were provided.

OneOf needs at least one Checkr to choose from.
Pass one or more Checkrs to OneOf.";
}
