using QuickFuzzr;

namespace QuickCheckr.Supplier;

public class IntRange(int min, int max) : ISupplrDefinition<int>
{
    public FuzzrOf<int> GetFuzzr() => Fuzzr.Int(min, max);

    public Shrinker[] GetShrinkers() =>
        [ Shrink.OnType<int>(a => a.Classify(Try.Function<int>(GetAlternatives)))
        ];

    public IEnumerable<int> GetAlternatives(int original)
    {
        var local = original;
        while (local > min)
        {
            local--;
            yield return local;
        }
        local = original;
        while (local < max - 1)
        {
            local++;
            yield return local;
        }
    }
}
