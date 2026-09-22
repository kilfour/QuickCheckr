using QuickCheckr.Authoring.ThePress;
using QuickPulse.Show;

namespace QuickCheckr.Tests.Verification;

public class CheckrExpectTests
{
    [Fact]
    public void Failure_messages_can_use_the_configured_formatter()
    {
        var checkr =
            from showr in Showr.ForTrace(() => Please.AllowMe().ToReplace<int>(value => $"<{value}>"))
            from expectation in Checkr.Expect(
                "answer", a => a.Fail($"Value: {a.Show(42)}"))
            select Case.Closed;

        var article = Journalist.Publish(checkr, configured => configured.Run());

        Assert.Equal(["Value: <42>"], article.FailingExpectationMessages());
    }
}
