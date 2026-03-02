using QuickPulse.Arteries;

namespace QuickCheckr.Protocol;
/// <summary>
/// Predefined InternalAffairs modes that enable additional diagnostic output during a run.  
/// Use these to gain insight into events, failures, or shrinking behaviour while debugging a Checkr.
/// </summary>
public static class InternalAffairs
{
    /// <summary>
    /// Records internal events emitted during execution.  
    /// Use when you want a high-level view of what the engine is doing.
    /// </summary>
    public static readonly InternalAffair Events =
        new(() => FileLog.Write(), Procedure.Events);

    /// <summary>
    /// Records detailed information whenever an exception is recorded.  
    /// Use to investigate what exceptions are being thrown during a run.
    /// </summary>
    public static readonly InternalAffair EvidenceFailures =
        new(() => FileLog.Write(), Procedure.EvidenceFailures);

    /// <summary>
    /// Records clues produced during input shrinking.  
    /// Use when debugging why a particular minimal case was chosen.
    /// </summary>
    public static readonly InternalAffair InputShrinking =
        new(() => FileLog.Write(), Procedure.InputShrinking);

    public static readonly InternalAffair ExecutionShrinking =
        new(() => FileLog.Write(), Procedure.ExecutionShrinking);

    /// <summary>
    /// Highlights cases where null values interfere with shrinking.  
    /// Use to identify problematic null structures that prevent full minimisation.
    /// </summary>
    public static readonly InternalAffair NullIssues =
        new(() => FileLog.Write(), Procedure.NullIssues);

    /// <summary>
    /// A convenience collection of commonly useful InternalAffairs.  
    /// Use to quickly enable broad diagnostics during development.
    /// </summary>
    public static readonly InternalAffair[] All =
        [Events, EvidenceFailures, NullIssues, InputShrinking];
}
