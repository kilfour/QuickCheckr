using QuickCheckr.StringReduction;
using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.Tests.Verification.StringReduction;

public class ResultTests
{
    [Fact]
    public void SomeContainsTheStateAndValue()
    {
        var state = new SelectrState("abc");

        var result = SelectrResult.Some(state, "value");

        Assert.Same(state, result.State);
        Assert.Equal("value", result.Value);
        Assert.True(result.Succeeded);
    }

    [Fact]
    public void DefaultContainsTheStateAndDefaultValue()
    {
        var state = new SelectrState("abc");

        var result = SelectrResult.Default<string>(state);

        Assert.Same(state, result.State);
        Assert.Null(result.Value);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public void SelectionOnlyProducesTheDefinedMarker()
    {
        var state = new SelectrState("abc");

        var result = SelectrResult.SelectionOnly(state);

        Assert.Same(state, result.State);
        Assert.Equal(Selection.Defined, result.Value);
        Assert.True(result.Succeeded);
    }
}
