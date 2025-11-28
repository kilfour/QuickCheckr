using QuickPulse.Show;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

public class InputShrinkingClue
{
    public int ExecutionId { get; private set; } = 0;
    public string Key { get; set; } = string.Empty;
    public object? Original { get; private set; } = null;
    public object? Result { get; private set; } = null;
    public string? Report { get; private set; } = null;
    public ShrinkingIntent Intent { get; private set; } = ShrinkingIntent.Ignore;
    public string Strategy { get; private set; } = string.Empty;
    public int Count { get; private set; } = 1;
    public int AddToCount(int number) => Count += number;

    public static InputShrinkingClue Ignore<T>(int executionId, string key, object? value)
        => new()
        {
            ExecutionId = executionId,
            Key = key,
            Original = value,
            Result = null,
            Report = string.Empty,
            Intent = ShrinkingIntent.Ignore,
            Strategy = typeof(T).Name
        };
    public static InputShrinkingClue Keep<T>(int executionId, string key, object? value)
        => Keep<T>(executionId, key, value, Introduce.This(value!, false));

    // this should be a replace one i reckon
    public static InputShrinkingClue Keep<T>(int executionId, string key, object? value, object? result)
       => new()
       {
           ExecutionId = executionId,
           Key = key,
           Original = value,
           Result = result,
           Report = Introduce.This(result!, false),
           Intent = ShrinkingIntent.Keep,
           Strategy = typeof(T).Name
       };

    public static InputShrinkingClue Keep<T>(int executionId, string key, object? value, string report)
        => new()
        {
            ExecutionId = executionId,
            Key = key,
            Original = value,
            Result = value,
            Report = report,
            Intent = ShrinkingIntent.Keep,
            Strategy = typeof(T).Name
        };

    public static InputShrinkingClue Replace<T>(
            int executionId,
            string key,
            object? value,
            object? result)
        => Replace<T>(executionId, key, value, result, Introduce.This(result!, false));

    public static InputShrinkingClue Replace<T>(
            int executionId,
            string key,
            object? value,
            object? result,
            string report)
        => new()
        {
            ExecutionId = executionId,
            Key = key,
            Original = value,
            Result = result,
            Report = report,
            Intent = ShrinkingIntent.Replace,
            Strategy = typeof(T).Name
        };

    public static InputShrinkingClue Remove<T>(
            int executionId,
            string key,
            object? value,
            object? result)
        => new()
        {
            ExecutionId = executionId,
            Key = key,
            Original = value,
            Result = result,
            Report = null,
            Intent = ShrinkingIntent.Remove,
            Strategy = typeof(T).Name
        };
}
