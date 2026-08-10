using QuickFuzzr;

namespace QuickCheckr.Supplier;

public interface ISupplrDefinition<TValue>
{
    public FuzzrOf<TValue> GetFuzzr();
    public Shrinker[] GetShrinkers();
    public IEnumerable<TValue> GetAlternatives(TValue original);
}
