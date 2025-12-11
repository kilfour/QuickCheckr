using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Checkr
{
    /// <summary>
    /// Performs a side-effecting operation without recording it as an action.  
    /// Use when the behaviour is required for the test but should not appear in the final report.
    /// </summary>
    public static CheckrOf<Case> Perform(Action action) =>
        state => { action(); return CheckrResult.CaseOnly(state); };

    /// <summary>
    /// Performs an asynchronous side-effecting operation without recording it as an action.  
    /// Use when you need to await a task as setup or helper logic that should remain invisible in the report.
    /// </summary>
    public static CheckrOf<Case> Perform(Task action) =>
        state => { action.GetAwaiter().GetResult(); return CheckrResult.CaseOnly(state); };

}