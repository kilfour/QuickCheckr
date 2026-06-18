namespace QuickCheckr.Protocol;

[Flags]
public enum ReportMode
{
    None = 0,
    Trace = 1,
    StackTrace = 2,
    FinalTrace = 4,
    Default = Trace | StackTrace
}
