using QuickCheckr.Authoring;
using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.P_WhenThingsGoWrong.NoOptionsAvailable;

[DocFile]
[DocFileHeader($"{nameof(Poolr)}.{nameof(Poolr.OneOf)}")]
[DocContent($"Throws an `{nameof(EmptyPoolException)}` when the pool is empty.")]
[DocBoldHeader("Message:")]
[DocExample(typeof(C_PoolrOneOf), nameof(Message))]
public class C_PoolrOneOf : DocAttributeBaseTest
{
    [Fact]
    public void NoOptionsThrows()
    {
        var ex = Assert.Throws<EmptyPoolException>(() => Poolr.OneOf<int>("pool").Select(_ => Case.Closed).Run());
        Assert.Equal(Message(), ex.Message);
    }

    [CodeSnippet]
    private static string Message() =>
@"Trackr.OneOfPool<T> could not choose a 'Int32' from pool 'pool' because the pool is empty.

Trackr.OneOfPool<T> needs at least one stored value to choose from.
Add a value to the pool before calling Trackr.OneOfPool<T>, or use a conditional pool operation when the pool may be empty.";
}
