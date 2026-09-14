namespace QuickTestr.Bolts;

/// <summary>
/// Configures how failing inputs are grouped and stored while filling the vault.
/// Use to control which failures are considered distinct, how many are kept,
/// or which inputs should be skipped.
/// </summary>
public record VaultPolicy<TInput>(
    Func<TInput, object> ClassifyBy,
    int? MaxStoredFailures = 10,
    Func<TInput, bool>? SkipWhen = null)
{
    public static VaultPolicy<TInput> Default => new(a => a!);
}
