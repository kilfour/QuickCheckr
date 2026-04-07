using QuickCheckr.StringReduction;

namespace QuickCheckr.Tests.Investigations.J_StringReduction;

public class OrTests
{
    private static string RunAll(SelectrOf<Selection> selector, string input)
    {
        var result = string.Empty;
        var runner = selector.Run(input);
        while (!runner.EndOfText) result = runner.Next();
        return result;
    }

    [Fact]
    public void FirstShot()
    {
        var selectr =
            Selectr.Once('a').Or(Selectr.Once('b')).Remove().Defined();
        Assert.Equal("cdc", RunAll(selectr, "abcdcba"));
    }

    [Theory]
    [InlineData("39+2", "+")]
    [InlineData("1+(2 + 3)+4", "++")]
    public void RemoveTerms(string input, string expected)
    {
        var number = Selectr.AtleastOnce(char.IsDigit);
        var enclosed = Selectr.Balanced('(', ')');
        Assert.Equal(expected, RunAll(number.Or(enclosed).Remove().Defined(), input));
    }
}