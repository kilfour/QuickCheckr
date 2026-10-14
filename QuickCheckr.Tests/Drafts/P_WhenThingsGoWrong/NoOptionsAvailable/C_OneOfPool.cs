using QuickCheckr.Authoring;
using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.P_WhenThingsGoWrong.NoOptionsAvailable;

[DocFile]
[DocFileHeader("OneOfPool")]
[DocContent("Throws an `EmptyPoolException` when the pool is empty.")]
[DocBoldHeader("Message:")]
[DocExample(typeof(C_OneOfPool), nameof(Message))]
public class C_OneOfPool : DocAttributeBaseTest
{
    [Fact]
    public void NoOptionsThrows()
    {
        var ex = Assert.Throws<EmptyPoolException>(() => Trackr.OneOfPool<int>("pool", a => s => CheckrResult.CaseOnly(s)).Run());
        Assert.Equal(Message(), ex.Message);
    }

    [CodeSnippet]
    private static string Message() =>
@"Trackr.OneOfPool<T> could not choose a 'Int32' from pool 'pool' because the pool is empty.

Trackr.OneOfPool<T> needs at least one stored value to choose from.
Add a value to the pool before calling Trackr.OneOfPool<T>, or use a conditional pool operation when the pool may be empty.";
}
