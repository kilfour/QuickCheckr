namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Collapsing;

internal interface ICollapseOption
{
    object? Select(object value);
}

internal sealed record CollapseOption<T>(Func<T, T?> Selector) : ICollapseOption
    where T : class
{
    public object? Select(object value) => Selector((T)value);
}
