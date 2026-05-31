namespace QuickCheckr;

/// <summary>
/// Represents how many executions occur within a single run.  
/// Use to model stateful behaviour spanning several sequential steps.
/// </summary>
public record ExecutionCount(int NumberOfExecutions);