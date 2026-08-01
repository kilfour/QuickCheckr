using QuickCheckr.StringReduction;
using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.Tests.Verification.StringReduction;

public class TextProjectionTests
{
    [Fact]
    public void ProjectsAConcreteSliceToItsText()
    {
        SelectrOf<StringSlice> sliceSelector = state =>
            SelectrResult.Some(state, new StringSlice(0, 2, "ab"));
        var selector = sliceSelector.Text();
        var state = new SelectrState("abc");

        var result = selector(state);

        Assert.Equal("ab", result.Value);
        Assert.True(result.Succeeded);
    }

    [Fact]
    public void MissingSliceProducesTheDefaultText()
    {
        SelectrOf<StringSlice> sliceSelector = SelectrResult.Default<StringSlice>;
        var selector = sliceSelector.Text();
        var state = new SelectrState("abc");

        var result = selector(state);

        Assert.Null(result.Value);
        Assert.False(result.Succeeded);
    }
}
