namespace QuickCheckr.Diagnostics.Bolts;

public class AutopsyPath
{
    public const string ExecutionShrinking = "ExecutionShrinking";
    public static string Pass(int passNumber) => $"Pass-{passNumber}";
    public static string Checking(int executionId) => $"Checking-{executionId}";
    public static string Executing(int executionId) => $"Executing-{executionId}";


    public const string FailedAndRemoved = "FailedAndRemoved";
}



