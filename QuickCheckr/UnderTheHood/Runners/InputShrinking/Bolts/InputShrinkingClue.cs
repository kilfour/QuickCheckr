namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

public class InputShrinkingClue
{
    public int ExecutionId { get; set; } = 0;
    public string Key { get; set; } = string.Empty;
    public object? Original { get; set; } = null;
    public object? Result { get; set; } = null;
    public string? Report { get; set; } = null;
    public ShrinkingIntent Intent { get; set; } = ShrinkingIntent.Ignore;
    public object? Reason { get; set; } = null;
    public string Strategy { get; set; } = string.Empty;

    public static InputShrinkingClue Ignore<T>(int executionId, string key, object? value)
        => new()
        {
            ExecutionId = executionId,
            Key = key,
            Original = value,
            Result = null,
            Report = string.Empty,
            Intent = ShrinkingIntent.Ignore,
            Reason = null,
            Strategy = typeof(T).Name
        };
}
