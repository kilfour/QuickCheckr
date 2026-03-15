using QuickCheckr.UnderTheHood.Dispatching;
using QuickCheckr.UnderTheHood.Docketing;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Defaults;

public class PrimitiveClassifier : Classifier
{
    public override HandlerResult<InputClassification> TryHandle(ShrinkingInfo info, ShrinkingContext context)
    {
        if (!CanHandle(info)) return Unhandled;
        return HandlerResult.Handled(Handle(info, context));
    }

    private static bool CanHandle(ShrinkingInfo info) =>
        PrimitiveValues.Keys.Any(k => k.IsAssignableFrom(info.NormalizedType));

    private static InputClassification Handle(ShrinkingInfo info, ShrinkingContext context)
    {
        var verifier = new Verifier(context.State, context.Check);
        var primitiveKey = PrimitiveValues.Keys.First(k => k.IsAssignableFrom(info.NormalizedType));
        var primitiveVals = PrimitiveValues[primitiveKey];
        foreach (var candidate in primitiveVals.Where(x => !Equals(x, info.Value)))
        {
            if (verifier.RunPassed(info.ExecutionId, info.Key, candidate))
            {
                // Mark input as relevant, because we found a candidate that makes the run pass.
                var clue = InputShrinkingClue.Keep<PrimitiveClassifier>(
                    info.ExecutionId, info.Path, info.Value, context.PrintInput(info.Value));
                context.State.Docket.Record(new InputShrinkingClueFound(clue));
                return new InputRelevant(context.PrintInput(info.Value));
            }
        }

        foreach (var candidate in primitiveVals.Where(x => !Equals(x, info.Value)))
        {
            //var witness = primitiveVals.First();
            if (verifier.TestimonyPassed(info.ExecutionId, info.Key, candidate))
            {
                // Mark input as relevant, because we found a candidate that makes the run pass.
                var clue = InputShrinkingClue.Keep<PrimitiveClassifier>(
                    info.ExecutionId, info.Path, info.Value, context.PrintInput(info.Value));
                context.State.Docket.Record(new InputShrinkingClueFound(clue));
                return new InputRelevant(context.PrintInput(info.Value));
            }
        }

        var ignoreClue = InputShrinkingClue.Ignore<PrimitiveClassifier>(info.ExecutionId, info.Path, info.Value);
        context.State.Docket.Record(new InputShrinkingClueFound(ignoreClue));
        // -----------------------------------------------
        // TESTIFY 
        // - try [0, 1] => fails => irrelevant, report [_, 1], store [0, 1]
        // - restore original value [1, 1]
        // - try [1, 0] => fails => 
        //     - restore [0, 1]
        //         try [0, 0] => passes => relevant
        context.State.Memory.Testify(info.ExecutionId, info.Key, primitiveVals.First());
        // -----------------------------------------------
        return new InputAssumedIrrelevant();
    }

    public static readonly Dictionary<Type, object[]> PrimitiveValues =
        new()
        {
            { typeof(bool),     new object[] { true, false } },
            { typeof(byte),     new object[] { (byte)0, (byte)1, byte.MaxValue } },
            { typeof(sbyte),    new object[] { sbyte.MinValue, -1, 0, 1, sbyte.MaxValue } },
            { typeof(short),    new object[] { (short)0, short.MinValue, (short)-1, (short)1, short.MaxValue } },
            { typeof(ushort),   new object[] { (ushort)0, (ushort)1, ushort.MaxValue } },
            { typeof(int),      new object[] { -10000, -1000, -1, 0, 1, 1000, 10000 } },
            { typeof(uint),     new object[] { 0u, 1u, 1000u, uint.MaxValue } },
            { typeof(long),     new object[] { long.MinValue, -1000L, -1L, 0L, 1L, 1000L, long.MaxValue } },
            { typeof(ulong),    new object[] { 0UL, 1UL, 1000UL, ulong.MaxValue } },
            { typeof(float),    new object[] { float.MinValue, -1f, 0f, 1f, float.MaxValue, float.NaN, float.NegativeInfinity, float.PositiveInfinity } },
            { typeof(double),   new object[] { double.MinValue, -1.0, 0.0, 1.0, double.MaxValue, double.NaN, double.NegativeInfinity, double.PositiveInfinity } },
            { typeof(decimal),  new object[] { decimal.MinValue, -1m, 0m, 1m, decimal.MaxValue } },
            { typeof(char),     new object[] { '\0', 'a', 'Z', ' ', '\n', '\uFFFF' } },
            { typeof(string),   new object[] { null!, "", "a", "the quick brown fox", new string('-', 256), new string('-', 1024) } },
            { typeof(DateTime), new object[] { DateTime.MinValue, DateTime.MaxValue, DateTime.UnixEpoch, DateTime.UtcNow, DateTime.Today } },
            { typeof(DateOnly), new object[] { DateOnly.FromDateTime(DateTime.MinValue), DateOnly.FromDateTime(DateTime.MaxValue), DateOnly.FromDateTime(DateTime.UnixEpoch), DateOnly.FromDateTime(DateTime.UtcNow), DateOnly.FromDateTime(DateTime.Today) } },
            { typeof(DateTimeOffset), new object[] { DateTimeOffset.MinValue, DateTimeOffset.MaxValue, DateTimeOffset.UtcNow, DateTimeOffset.Now } },
            { typeof(TimeOnly), new object[] { new TimeOnly(0), new TimeOnly(0, 1), new TimeOnly(10, 2), new TimeOnly(20, 3), new TimeOnly(23, 4), new TimeOnly(23, 59, 59, 999) } },
            { typeof(TimeSpan), new object[] { TimeSpan.Zero, TimeSpan.FromSeconds(1), TimeSpan.FromDays(1), TimeSpan.MinValue, TimeSpan.MaxValue } },
            { typeof(Guid),     new object[] { Guid.Empty, Guid.NewGuid() } },
            { typeof(Uri),      new object[] { null!, new Uri("http://localhost"), new Uri("https://example.com/resource?query=1") } }
        };
}