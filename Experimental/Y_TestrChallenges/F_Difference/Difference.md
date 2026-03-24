# Difference

**The Fuzzr:**  
```csharp
from _ in Configr<Pair>.Construct(Fuzzr.Int(), Fuzzr.Int())
from pair in Fuzzr.One<Pair>()
select pair
```

**The Shrinkr:**  
```csharp
public class The
{
    public static Shrinker Shrinkr =
        Shrink.OnType<Pair>(
            s => s.Simplify(
                Reduce.Function<Pair>(a => TowardsZero(a))));
    private static IEnumerable<Pair> TowardsZero(Pair input)
    {
        var local = input;
        while (local.A >= 0)
        {
            local = local with { A = local.A - 1, B = local.B - 1 };
            yield return local;
        }
    }
}
```
