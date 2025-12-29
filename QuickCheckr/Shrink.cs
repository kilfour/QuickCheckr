using QuickCheckr.UnderTheHood;
using QuickFuzzr;

namespace QuickCheckr;


public static partial class Shrinkr
{
    public static CheckrOf<Case> For<T>(FuzzrOf<T> fuzzr, int times = 10)
        => For<T>([Shrink.Fuzzr(fuzzr, times)]);

    public static CheckrOf<Case> For<T>(params ShrinkOption[] options)
        => state =>
        {
            state.Memory.StoreShrinkerr<T>(typeof(T).FullName!, options);
            return CheckrResult.CaseOnly(state);
        };
}

public static class Shrink
{
    public static ShrinkOption Fuzzr<T>(FuzzrOf<T> fuzzr, int times = 10)
        => new ShrinkOptionFuzzr<T>(fuzzr, times);
}

public abstract record ShrinkOption;
public interface IShrinkOptionFuzzr
{
    public FuzzrOf<object> GetFuzzr();
    public int Times { get; }
}

public record ShrinkOptionFuzzr<T>(FuzzrOf<T> Fuzzr, int Times) : ShrinkOption, IShrinkOptionFuzzr
{
    public FuzzrOf<object> GetFuzzr()
        => Fuzzr.AsObject();
}

public record ShrinkOptionCustom<T>(Func<T, IEnumerable<T>>? Shrinker) : ShrinkOption;
public record ShrinkOptionDefault<T>() : ShrinkOption;