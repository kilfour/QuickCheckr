using QuickCheckr.UnderTheHood.WhenThingsGoWrong.Abstractions;

namespace QuickCheckr.UnderTheHood.WhenThingsGoWrong;

public class DuplicateKeyWriteException(string key, string operation, string scope)
    : QuickCheckrException(BuildMessage(key, operation, scope))
{
    private static string BuildMessage(string key, string operation, string scope) =>
    $@"Key '{key}' is written more than once in a single '{operation}' {scope}.

QuickCheckr treats keys as stable identifiers. 
Reusing a key for multiple write operations makes reports and shrinking ambiguous.

Possible solutions:
- Ensure each '{operation}' uses a unique key.
";
}
// - If you need the same value in multiple places, call Checkr.Input once and pass the value around instead of reading it again under the same key.
// - When using helper methods that declare Checkr steps, let the caller provide a key (or key prefix) so each call site gets a distinct label.
// - If two steps really represent the same concept, merge them into a single Checkr step instead of duplicating it under the same key.