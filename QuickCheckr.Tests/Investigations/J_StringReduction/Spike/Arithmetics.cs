namespace QuickCheckr.Tests.Investigations.J_StringReduction.Spike;

public class Arithmetics
{
    [Fact(Skip = "wip")]
    public void Atomic()
    {
        var selectr = SelectrOld.OneOf('a', 'b');
        var result = selectr(new SelectrState("abc"));
        Assert.Equal("bc", result.Value.Remove("abc"));
    }
}