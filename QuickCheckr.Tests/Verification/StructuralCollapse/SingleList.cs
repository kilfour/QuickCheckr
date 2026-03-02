using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

namespace QuickCheckr.Tests.Verification.StructuralCollapse;

public class SingleList
{
    [Fact]
    public void Simple_Empty_Result()
    {
        var list = new List<int> { 1, 2 };
        var info = new ShrinkingInfo(1, "input", list, "input", typeof(List<int>));
        var result = Collapser.Collapse(() => list.Count >= 0, info);
        Assert.Empty((IList<int>)result!);
    }

    [Fact]
    public void Simple_One_Element_Left()
    {
        var list = new List<int> { 1, 2 };
        var info = new ShrinkingInfo(1, "input", list, "input", typeof(List<int>));
        var result = Collapser.Collapse(() => list.Count >= 1, info);
        Assert.True(((IList<int>)result!).Count == 1);
    }
}
