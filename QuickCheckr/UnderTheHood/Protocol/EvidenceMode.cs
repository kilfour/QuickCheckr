namespace QuickCheckr.UnderTheHood.Protocol;

/// <summary>
/// Controls how QuickCheckr interacts with the evidence vault,
/// which stores seeds of previously failing runs.
/// </summary>
public enum EvidenceMode
{
    /// <summary>
    /// Do not consult the evidence vault.
    /// No stored seeds are replayed.
    /// </summary>
    Ignore,

    /// <summary>
    /// Replay all stored seeds from the evidence vault,
    /// but leave the vault unchanged regardless of outcome.
    /// Use for regression testing, keeping all evidence intact.
    /// </summary>
    Review,

    /// <summary>
    /// Replay all stored seeds from the evidence vault,
    /// and remove any seed that now passes.
    /// Use to automatically purge obsolete failures
    /// while keeping genuinely failing seeds.
    /// </summary>
    ReviewAndPurge
}
