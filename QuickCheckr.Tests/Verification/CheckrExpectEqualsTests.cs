using QuickCheckr.Authoring.ThePress;
using QuickPulse.Show;

namespace QuickCheckr.Tests.Verification;

public class CheckrExpectEqualsTests
{
    [Fact]
    public void Equal_values_pass()
    {
        var article = Journalist.Publish(
            Checkr.ExpectEquals("answer", 42, 42),
            checkr => checkr.Run());

        Assert.Equal(string.Empty, article.FailureDescription());
        Assert.Equal(["answer"], article.PassedExpectationLabels());
    }

    [Fact]
    public void Different_values_are_reported_using_the_configured_formatter()
    {
        var checkr =
            from showr in Showr.ForTrace(() => Please.AllowMe().ToReplace<int>(value => $"<{value}>"))
            from expectation in Checkr.ExpectEquals("answer", 41, 42)
            select Case.Closed;

        var article = Journalist.Publish(checkr, configured => configured.Run());

        Assert.Equal("answer", article.FailureDescription());
        Assert.Equal(["Expected: <41>", "Actual: <42>"], article.FailingExpectationMessages());
    }

    [Fact]
    public void Null_values_are_compared()
    {
        string? expected = null;
        string? actual = null;

        var article = Journalist.Publish(
            Checkr.ExpectEquals("nullable", expected, actual),
            checkr => checkr.Run());

        Assert.Equal(string.Empty, article.FailureDescription());
        Assert.Equal(["nullable"], article.PassedExpectationLabels());
    }
}
