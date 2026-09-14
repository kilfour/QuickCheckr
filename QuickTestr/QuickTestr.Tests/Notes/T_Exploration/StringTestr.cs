using QuickTestr.Bolts.Builders.Strings;

namespace QuickTestr;

public static class StringTestr
{
    public static StringTestrFor Named(string name) => new(name);
}
