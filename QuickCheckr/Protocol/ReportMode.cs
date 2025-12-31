namespace QuickCheckr.Protocol;

[Flags]
public enum ReportMode
{
    None = 0,
    Trace = 1,
    Warning = 2,
    StackTrace = 4,
    Labels = 8,
    Default = Trace | Warning | StackTrace | Labels
}
