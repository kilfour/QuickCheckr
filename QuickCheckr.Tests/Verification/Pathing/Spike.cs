
using System.Linq.Expressions;
using QuickCheckr.UnderTheHood;

namespace QuickCheckr.Tests.Verification.Pathing;

public class Spike
{
    [Fact]
    public void Construction() =>
        Assert.Equal("Everything", GetPath.Of<Everything>().ToString());

    [Fact]
    public void Specific_Construction() =>
        Assert.Equal("the-root", GetPath.Of<Everything>("the-root").ToString());

    [Fact]
    public void ObjectProperty() =>
        Assert.Equal("Everything.TheUniverse",
            GetPath.Of<Everything>().Member(a => a.TheUniverse).ToString());

    [Fact]
    public void ObjectField() =>
        Assert.Equal("Universe.Answer",
            GetPath.Of<Universe>().Member(a => a.Answer).ToString());

    [Fact]
    public void ListElement() =>
        Assert.Equal("Everything.TheMultiverse[0]",
            GetPath.Of<Everything>()
            .Member(a => a.TheMultiverse)
            .Index(0)
            .ToString());

    [Fact]
    public void ListElementField() =>
        Assert.Equal("Everything.TheMultiverse[0].Answer",
            GetPath.Of<Everything>()
                .Member(a => a.TheMultiverse)
                .Index(0)
                .Member(a => a.Answer)
                .ToString());

    public class Everything
    {
        public Universe? TheUniverse { get; set; }
        public List<Universe> TheMultiverse { get; set; } = [];
    }

    public class Universe { public int Answer = 42; }
}

public class PathOf<T>(string? value = null)
{
    public override string ToString() => value ?? typeof(T).Name;

    public PathOf<TProp> Member<TProp>(Expression<Func<T, TProp>> expr)
        => new(ToString() + "." + expr.AsMemberInfo().Name);
}

public static class GetPath
{
    public static PathOf<T> Of<T>(string? value = null) => new(value);

    public static PathOf<TElement> Index<TElement>(this PathOf<List<TElement>> path, int index)
       => new(path.ToString() + $"[{index}]");
}