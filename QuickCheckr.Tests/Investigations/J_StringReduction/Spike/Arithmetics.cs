using QuickCheckr.StringReduction.Nuts;

namespace QuickCheckr.Tests.Investigations.J_StringReduction.Spike;

public class Arithmetics
{
    [Fact(Skip = "wip")]
    public void Atomic()
    {
        var selectr = Selectr.OneOf('a', 'b');
        var result = selectr(new SelectrState("abc"));
        Assert.Equal("bc", result.Value.Remove("abc"));
    }
}