using QuickCheckr.StringReduction;

namespace QuickCheckr.Tests.Verification.StringReduction;

public class EditableTests
{
    [Fact]
    public void EarlierSelectionCanBeReplacedAfterLaterSelectionsRun()
    {
        var selector =
            from left in Stringr.AtleastOnce(char.IsDigit).Editable()
            from op in Stringr.Once('+')
            from right in Stringr.AtleastOnce(char.IsDigit)
            from replacement in left.Replace(_ => right.Text)
            select Selection.Defined;

        var result = StringReductionTestTools.RunAll(selector, "39+2");

        Assert.Equal("2+2", result);
    }

    [Fact]
    public void TwoEditableSelectionsCanBeSwapped()
    {
        var selector =
            from left in Stringr.AtleastOnce(char.IsDigit).Editable()
            from op in Stringr.Once('+')
            from right in Stringr.AtleastOnce(char.IsDigit).Editable()
            from replaceLeft in left.Replace(_ => right.Text)
            from replaceRight in right.Replace(_ => left.Text)
            select Selection.Defined;

        var result = StringReductionTestTools.RunAll(selector, "39+2");

        Assert.Equal("2+39", result);
    }

    [Fact]
    public void LateReplacementsAccountForLengthChanges()
    {
        var selector =
            from left in Stringr.AtleastOnce(char.IsDigit).Editable()
            from op in Stringr.Once('+').Editable()
            from right in Stringr.AtleastOnce(char.IsDigit).Editable()
            from replaceLeft in left.Replace(_ => "1234")
            from replaceOp in op.Replace(_ => "---")
            from replaceRight in right.Replace(_ => "12345")
            select Selection.Defined;

        var result = StringReductionTestTools.RunAll(selector, "39+2");

        Assert.Equal("1234---12345", result);
    }

    [Fact]
    public void ReplacementOrderIsObservable()
    {
        var selector =
            from left in Stringr.AtleastOnce(char.IsDigit).Editable()
            from op in Stringr.Once('+').Editable()
            from right in Stringr.AtleastOnce(char.IsDigit).Editable()
            from replaceRight in right.Replace(_ => left.Text)
            from replaceLeft in left.Replace(_ => right.Text)
            from replaceOp in op.Replace(_ => "-")
            select Selection.Defined;

        var result = StringReductionTestTools.RunAll(selector, "39+2");

        Assert.Equal("32-9", result);
    }
}
