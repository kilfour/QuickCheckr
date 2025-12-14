using QuickCheckr.UnderTheHood.WhenThingsGoWrong.Abstractions;

namespace QuickCheckr.UnderTheHood.WhenThingsGoWrong;

public class DuplicateKeyWriteException(string key, IEnumerable<string> checkrs)
    : QuickCheckrException(BuildMessage(key, checkrs))
{
    private static string BuildMessage(string key, IEnumerable<string> checkrs)
    {
        var checkrList = string.Join(Environment.NewLine, checkrs.Select(a => $"- '{a}'"));
        return @$"Key '{key}' is written more than once to the same memory location.

QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.

The following Checkrs write to the this bank using the same key:

{checkrList}
";
    }
}
