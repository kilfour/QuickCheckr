namespace QuickCheckr.Protocol;

[Flags]
public enum ReportMode
{
    None = 0,
    Trace = 1,
    StackTrace = 2,
    Labels = 4,
    FinalTrace = 8,
    Default = Trace | StackTrace | Labels
}
