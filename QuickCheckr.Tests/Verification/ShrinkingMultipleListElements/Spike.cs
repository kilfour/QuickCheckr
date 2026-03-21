namespace QuickCheckr.Tests.Verification.ShrinkingMultipleListElements;

public class Spike
{
    [Fact]
    public void Go()
    {
        var list = new List<int> { 10, 20 };
        Func<List<int>, bool> predicate = a => a.Sum() != 30;
    }
}