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
            from a_b in Selectr.Once('a', 'b').Remove()
            select Selection.Defined;
        Assert.Equal("bc", selectr.Run("abc").Next());
    }

    [Fact]
    public void FirstCharIsNoMatch()
    {
        var selectr =
            from a_b in Selectr.Once('a', 'b').Remove()
            select Selection.Defined;
        Assert.Equal("ca", selectr.Run("cba").Next());
    }

    [Fact]
    public void CallingNextMultipleTimes_TrackEnd()
    {
        var selectr =
            from a_b in Selectr.Once('a', 'b').Remove()
            select Selection.Defined;
        var runner = selectr.Run("abc");
        Assert.Equal("bc", runner.Next());
        Assert.False(runner.EndOfText);
        Assert.Equal("c", runner.Next());
        Assert.False(runner.EndOfText);
        Assert.Equal("c", runner.Next());
        //Assert.True(runner.EndOfText);
    }

    [Fact]
    public void Chained()
    {
        var selectr =
            from first in Selectr.Once('a', 'b').Remove()
            from second in Selectr.Once('a', 'b').Remove()
            select Selection.Defined;
        var runner = selectr.Run("abc");
        Assert.Equal("c", runner.Next());
    }

    [Fact]
    public void ChainedFirstSetIsNoMatch()
    {
        var selectr =
            from first in Selectr.Once('a', 'b').Remove()
            from second in Selectr.Once('a', 'b').Remove()
            select Selection.Defined;
        var runner = selectr.Run("dabc");
        Assert.Equal("dc", runner.Next());
    }

    [Fact]
    public void Chained_Tricky()
    {
        var selectr =
            from first in Selectr.Once('a', 'b').Remove()
            from second in Selectr.Once('a', 'b').Remove()
            select Selection.Defined;
        var runner = selectr.Run("azabc");
        Assert.Equal("azc", runner.Next());
    }

    [Fact]
    public void ReplaceUsingSelectionText()
    {
        var selectr =
            from a in Selectr.Once('a', 'c').Replace(t => t.ToUpper())
            select Selection.Defined;
        var runner = selectr.Run("abc");
        Assert.Equal("Abc", runner.Next());
        Assert.Equal("AbC", runner.Next());
        Assert.True(runner.EndOfText);
    }

    [Fact]
    public void UsingRange()
    {
        var selectr =
            from a in Selectr.Once('a').Remove()
            from replace in Selectr.Once('b').Replace(t => a.ToUpper())
            select Selection.Defined;
        var runner = selectr.Run("abc");
        Assert.Equal("Ac", runner.Next());
    }

    [Fact]
    public void UsingRange_NoMatch()
    {
        var selectr =
             from a_b in Selectr.Once('a', 'b').Remove()
             from replace in Selectr.Once('a', 'b').Remove()
             select Selection.Defined;
        var runner = selectr.Run("ac");
        Assert.Equal("ac", runner.Next());
    }

    [Fact]
    public void UsingRange_Twice()
    {
        var selectr =
            from a_b in Selectr.Once('a', 'b').Remove()
            from c_d in Selectr.Once('c', 'd').Remove()
            select Selection.Defined;
        var runner = selectr.Run("acbdee");
        Assert.Equal("bdee", runner.Next());
        Assert.Equal("ee", runner.Next());
    }

    [Fact]
    public void UsingRange_Twice_with_other_stuff()
    {
        var selectr =
            from a_b in Selectr.Once('a', 'b').Remove()
            from replace in Selectr.Once('c', 'd').Replace(t => a_b.ToUpper())
            select Selection.Defined;
        var runner = selectr.Run("--ac--bd--");
        Assert.Equal("--A--bd--", runner.Next());
        Assert.Equal("--A--B--", runner.Next());
    }

    [Fact]
    public void UsingRange_WithoutRemove()
    {
        var selectr =
            from a_b in Selectr.Once('a', 'b').Text()
            from replace in Selectr.Once('c', 'd').Replace(t => a_b.ToUpper())
            select Selection.Defined;
        var runner = SelectrRunner.For(selectr, "--ac--bd--");
        Assert.Equal("--aA--bd--", runner.Next());
        Assert.Equal("--aA--bB--", runner.Next());
    }
}


