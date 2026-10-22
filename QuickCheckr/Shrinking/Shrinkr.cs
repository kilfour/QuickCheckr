namespace QuickCheckr.Shrinking;

public static class Shrinkr
{
    public static Shrinker<T> For<T>() => new();
}


