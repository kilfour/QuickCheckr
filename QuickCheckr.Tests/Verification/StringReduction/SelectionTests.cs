using QuickCheckr.StringReduction;
using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.Tests.Verification.StringReduction;

public class SelectionTests
{
    [Fact]
    public void StringSliceRetainsItsLocationAndSelectedText()
    {
        var selection = StringSlice.From(2, 3, "abcdef");

        var slice = Assert.IsType<StringSlice>(selection);
        Assert.Equal(2, slice.Start);
        Assert.Equal(3, slice.Length);
        Assert.Equal("cde", slice.Text);
    }

    [Fact]
    public void TextSelectionCanBeViewedAsItsConcreteSlice()
    {
        ITextSelection selection = new StringSlice(0, 1, "a");

        var slice = selection.AsSlice();

        Assert.Equal("a", slice.Text);
    }

    [Fact]
    public void EditableSelectionCanBeViewedAsEditable()
    {
        ITextSelection selection = new StringSlice(0, 1, "a");

        var editable = selection.AsEditable();

        Assert.Equal("a", editable.Text);
    }

    [Fact]
    public void NonSliceTextSelectionCannotBeUsedByAnEditingOperation()
    {
        ITextSelection selection = new OtherSelection("a");

        Assert.Throws<InvalidCastException>(() => selection.AsSlice());
        Assert.Throws<InvalidCastException>(() => selection.AsEditable());
    }

    private sealed record OtherSelection(string Text) : ITextSelection;
}
