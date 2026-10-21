using QuickCheckr.UnderTheHood.WhenThingsGoWrong;
using QuickFuzzr;

namespace QuickCheckr.Tests.Drafts.F_TheDeepEnd;

public class PoolrForEachWithActs
{
    [Fact]
    public void Example() =>
        Assert.Throws<DuplicateKeyWriteException>(() => (
            from pool1 in Poolr.Add("one", Checkr.Input("input one", Fuzzr.Constant(1)))
            from pool2 in Poolr.Add("two", Checkr.Input("input two", Fuzzr.Constant(2)))
            from _ in Poolr.ForEach<int>(a => Checkr.Act("label", () => { }))
            select Case.Closed).Run());
}