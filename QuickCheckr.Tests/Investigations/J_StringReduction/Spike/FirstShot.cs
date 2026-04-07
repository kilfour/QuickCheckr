using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.Tests.Investigations.J_StringReduction.Spike;


public static class T
{
    public static Func<string, string> Then(this Func<string, string> a, Func<string, string> b) =>
        text => b(a(text));

    public static Func<string, string> After(this Func<string, string> a, Func<string, string> b) =>
        b.Then(a);
}

public class FirstShot
{
    [Fact]
    public void Linqed()
    {
        var selectr =
            from a_b in Selectr.OneOf('a', 'b').Remove()
            select Selection.Defined;
        Assert.Equal("bc", selectr.Run("abc").Next());
    }

    [Fact]
    public void Chained()
    {
        var selectr =
            from first in Selectr.OneOf('a', 'b').Remove()
            from second in Selectr.OneOf('a', 'b').Remove()
            select Selection.Defined;
        var runner = selectr.Run("abc");
        Assert.Equal("c", runner.Next());
    }

    [Fact]
    public void UsingRange()
    {
        var selectr =
            from a_b in Selectr.OneOf('a', 'b').Remove()
            from replace in Selectr.OneOf('a', 'b').Replace(a_b.ToUpper())
            select Selection.Defined;
        var runner = selectr.Run("abc");
        Assert.Equal("Ac", runner.Next());
    }

    [Fact]
    public void UsingRange_NoMatch()
    {
        var selectr =
             from a_b in Selectr.OneOf('a', 'b').Remove()
             from replace in Selectr.OneOf('a', 'b').Remove()
             select Selection.Defined;
        var runner = selectr.Run("ac");
        Assert.Equal("ac", runner.Next());
    }

    [Fact]
    public void UsingRange_Twice()
    {
        var selectr =
            from a_b in Selectr.OneOf('a', 'b').Remove()
            from c_d in Selectr.OneOf('c', 'd').Remove()
            select Selection.Defined;
        var runner = selectr.Run("acbdee");
        Assert.Equal("bdee", runner.Next());
        Assert.Equal("ee", runner.Next());
    }

    [Fact]
    public void UsingRange_Twice_with_other_stuff()
    {
        var selectr =
            from a_b in Selectr.OneOf('a', 'b').Remove()
            from replace in Selectr.OneOf('c', 'd').Replace(a_b.ToUpper())
            select Selection.Defined;
        var runner = selectr.Run("--ac--bd--");
        Assert.Equal("--A--bd--", runner.Next());
        Assert.Equal("--A--B--", runner.Next());
    }

    [Fact]
    public void UsingRange_WithoutRemove()
    {
        var selectr =
            from a_b in Selectr.OneOf('a', 'b').Text()
            from replace in Selectr.OneOf('c', 'd').Replace(a_b.ToUpper())
            select Selection.Defined;
        var runner = SelectrRunner.For(selectr, "--ac--bd--");
        Assert.Equal("--aA--bd--", runner.Next());
        Assert.Equal("--aA--bB--", runner.Next());
    }
}


