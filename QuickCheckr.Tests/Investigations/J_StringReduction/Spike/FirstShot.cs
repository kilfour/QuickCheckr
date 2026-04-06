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
            from a_b in Selectr.OneOf('a', 'b')
            select a_b.Remove();
        Assert.Equal("bc", selectr.Run("abc").Next());
    }

    [Fact]
    public void Chained()
    {
        var selectr =
            from first in Selectr.OneOf('a', 'b')
            from second in Selectr.OneOf('a', 'b')
            select first.Remove().After(second.Remove());
        var runner = selectr.Run("abc");
        Assert.Equal("c", runner.Next());
    }

    [Fact]
    public void UsingRange()
    {
        var selectr =
            from a_b in Selectr.OneOf('a', 'b')
            from replace in Selectr.OneOf('a', 'b')
            select a_b.Remove().After(replace.Replace(a_b.Text.ToUpper()));
        var runner = selectr.Run("abc");
        Assert.Equal("Ac", runner.Next());
    }

    [Fact]
    public void UsingRange_NoMatch()
    {
        var selectr =
             from a_b in Selectr.OneOf('a', 'b')
             from replace in Selectr.OneOf('a', 'b')
             select a_b.Remove().After(replace.Replace(a_b.Text.ToUpper()));
        var runner = selectr.Run("ac");
        Assert.Equal("ac", runner.Next());
    }

    [Fact(Skip = "wip")]
    public void UsingRange_Twice()
    {
        var selectr =
            from a_b in Selectr.OneOf('a', 'b')
            from c_d in Selectr.OneOf('c', 'd')
            select a_b.Remove().After(c_d.Remove());
        var runner = selectr.Run("acbdee");
        Assert.Equal("bdee", runner.Next());
        Assert.Equal("ee", runner.Next());
    }

    [Fact]
    public void UsingRange_Twice_with_other_stuff()
    {
        var selectr =
            from a_b in SelectrOld.OneOf('a', 'b').Remove()
            from replace in SelectrOld.OneOf('c', 'd').Replace(a_b.ToUpper())
            select Selection.Defined;
        var runner = SelectrRunnerOld.For(selectr, "--ac--bd--");
        Assert.Equal("--A--bd--", runner.Next());
        Assert.Equal("--A--B--", runner.Next());
    }

    [Fact]
    public void Strategies()
    {
        SelectrOf<Func<string, string>> selectr =
            from a_b in Selectr.OneOf('a', 'b')
            from replace in Selectr.OneOf('c', 'd')
            select replace.Replace(a_b.Text.ToUpper());

        var runner = selectr.Run("--ac--bd--");
        Assert.Equal("--aA--bd--", runner.Next());
        Assert.Equal("--aA--bB--", runner.Next());
    }

    // [Fact]
    // public void UsingRange_WithoutRemove()
    // {
    //     var selectr =
    //         from a_b in Selectr.OneOf('a', 'b').AsText()
    //         from replace in Selectr.OneOf('c', 'd').Replace(a_b.ToUpper())
    //         select Selection.Defined;
    //     var runner = SelectrRunner.For(selectr, "--ac--bd--");
    //     Assert.Equal("--A--bd--", runner.Next());
    //     Assert.Equal("--A--B--", runner.Next());
    // }
}


