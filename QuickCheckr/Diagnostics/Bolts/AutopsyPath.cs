namespace QuickCheckr.Diagnostics.Bolts;

public static class AutopsyPath
{
    // -------------------------------------------------------------------------------------------
    // ExecutionShrinking
    public const string ExecutionShrinking = "ExecutionShrinking";
    public static string Pass(int passNumber) => $"Pass-{passNumber}";
    public static string Checking(int executionId) => $"Checking-{executionId}";
    public static string Executing(int executionId) => $"Executing-{executionId}";
    public const string FailedAndRemoved = "FailedAndRemoved";


    // -------------------------------------------------------------------------------------------
    // ActionShrinking
    public const string ActionShrinking = "ActionShrinking";


    // -------------------------------------------------------------------------------------------
    // InputShrinking
    public const string InputShrinking = "InputShrinking";
    public static string Input(string key) => $"Input-{key}";

    // -------------------------------------------------------------------------------------------
    // InputReduction
    public const string InputReduction = "InputReduction";
}



