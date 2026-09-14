namespace QuickTestr.Bolts.Builders.Strings;

public class StringTestrFor(string name)
{
    public StringTestrExplore<TResult> For<TResult>(Func<string, TResult> factory) => new(name, factory);
}
