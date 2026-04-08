using QuickCheckr;
using QuickCheckr.StringReduction;

namespace QuickCheckr.Tests.Investigations.J_StringReduction;

public class Arithmetics
{
    private static string RunAll(SelectrOf<Selection> selector, string input)
    {
        var result = string.Empty;
        var runner = selector.Run(input);
        while (!runner.EndOfText) result = runner.Next();
        return result;
    }

    [Fact]
    public void RemoveOpAndRhs()
    {
        var selectr =
            from lhs in Stringr.AtleastOnce(char.IsDigit)
            from op in Stringr.Once('+', '-').Remove()
            from rhs in Stringr.AtleastOnce(char.IsDigit).Remove()
            select Selection.Defined;
        Assert.Equal("39", RunAll(selectr, "39+2"));
    }

    [Fact]
    public void RemoveOpAndRhsWhitespaced()
    {
        var selectr =
            from lhs in Stringr.AtleastOnce(char.IsDigit)
            from wsl in Stringr.Optional(char.IsWhiteSpace).Remove()
            from op in Stringr.Once('+', '-').Remove()
            from wsr in Stringr.Optional(char.IsWhiteSpace).Remove()
            from rhs in Stringr.AtleastOnce(char.IsDigit).Remove()
            select Selection.Defined;
        Assert.Equal(" 39 ", RunAll(selectr, " 39  +2 "));
    }

    [Fact]
    public void RemoveLhsAndOp()
    {
        var selectr =
            from lhs in Stringr.AtleastOnce(char.IsDigit).Remove()
            from op in Stringr.Once('+', '-').Remove()
            from rhs in Stringr.AtleastOnce(char.IsDigit)
            select Selection.Defined;
        Assert.Equal("2", RunAll(selectr, "39+2"));
    }

    [Fact]
    public void ReplaceRhsWithLhs()
    {
        var selectr =
            from lhs in Stringr.AtleastOnce(char.IsDigit)
            from op in Stringr.Once('+', '-')
            from rhs in Stringr.AtleastOnce(char.IsDigit).Replace(t => lhs.Text)
            select Selection.Defined;
        Assert.Equal("39+39", RunAll(selectr, "39+2"));
    }

    [Fact]
    public void ReplaceLhsWithRhs()
    {
        var selectr =
            from lhs in Stringr.AtleastOnce(char.IsDigit).Editable()
            from op in Stringr.Once('+', '-')
            from rhs in Stringr.AtleastOnce(char.IsDigit)
            from replaceLhs in lhs.Replace(t => rhs.Text)
            select Selection.Defined;
        Assert.Equal("2+2", RunAll(selectr, "39+2"));
    }

    [Fact]
    public void Swap()
    {
        var selectr =
            from lhs in Stringr.AtleastOnce(char.IsDigit).Editable()
            from op in Stringr.Once('+', '-')
            from rhs in Stringr.AtleastOnce(char.IsDigit).Editable()
            from replaceLhs in lhs.Replace(t => rhs.Text)
            from replaceRhs in rhs.Replace(t => lhs.Text)
            select Selection.Defined;
        Assert.Equal("2+39", RunAll(selectr, "39+2"));
    }

    [Fact]
    public void All()
    {
        var selectr =
            from lhs in Stringr.AtleastOnce(char.IsDigit).Editable()
            from op in Stringr.Once('+', '-').Editable()
            from rhs in Stringr.AtleastOnce(char.IsDigit).Editable()
            from replaceLhs in lhs.Replace(t => rhs.Text)
            from replaceOp in op.Replace(t => "-")
            from replaceRhs in rhs.Replace(t => lhs.Text)
            select Selection.Defined;
        Assert.Equal("2-39", RunAll(selectr, "39+2"));
    }

    [Fact]
    public void All_LengthChange()
    {
        var selectr =
            from lhs in Stringr.AtleastOnce(char.IsDigit).Editable()
            from op in Stringr.Once('+', '-').Editable()
            from rhs in Stringr.AtleastOnce(char.IsDigit).Editable()
            from replaceLhs in lhs.Replace(t => "1234")
            from replaceOp in op.Replace(t => "---")
            from replaceRhs in rhs.Replace(t => "12345")
            select Selection.Defined;
        Assert.Equal("1234---12345", RunAll(selectr, "39+2"));
    }

    [Fact]
    public void Parenthesis()
    {
        var selectr =
            from lb in Stringr.Once('(').Remove()
            from between in Stringr.Until(')')
            from rb in Stringr.Once(')').Remove()
            select Selection.Defined;
        var runner = selectr.Run("1+(2+(3*4))");
        Assert.Equal("1+2+(3*4)", runner.Next());
        Assert.Equal("1+2+3*4", runner.Next());
        Assert.True(runner.EndOfText);
    }

    [Fact]
    public void Balanced()
    {
        var selectr =
            from lb in Stringr.Balanced('(', ')').Replace(t => t[1..^1])
            select Selection.Defined;
        var runner = selectr.Run("1+(2+(3*4)-5)");
        Assert.Equal("1+2+(3*4)-5", runner.Next());
        Assert.Equal("1+2+3*4-5", runner.Next());
        Assert.Equal("1+2+3*4-5", runner.Next());
        Assert.True(runner.EndOfText);
    }

    [Fact(Skip = "Fails with 32-9")]
    public void AllChaotic()
    {
        var selectr =
            from lhs in Stringr.AtleastOnce(char.IsDigit).Editable()
            from op in Stringr.Once('+', '-').Editable()
            from rhs in Stringr.AtleastOnce(char.IsDigit).Editable()
            from replaceRhs in rhs.Replace(t => lhs.Text)
            from replaceLhs in lhs.Replace(t => rhs.Text)
            from replaceOp in op.Replace(t => "-")
            select Selection.Defined;
        Assert.Equal("39-2", RunAll(selectr, "39+2"));
    }
}