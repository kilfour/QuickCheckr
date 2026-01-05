using QuickFuzzr;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Custom;

public interface IShrinkOption
{
    public Type TargetType { get; }
}

public abstract record ShrinkOption<T> : IShrinkOption
{
    public Type TargetType => typeof(T);
}

public interface IShrinkOptionFuzzr
{
    public FuzzrOf<object> GetFuzzr();
    public int Times { get; }
}

public record ShrinkOptionFuzzr<T>(FuzzrOf<T> Fuzzr, int Times) : ShrinkOption<T>, IShrinkOptionFuzzr
{
    public FuzzrOf<object> GetFuzzr()
        => Fuzzr.AsObject();
}

public interface IShrinkOptionFunction
{
    public Func<object, IEnumerable<object>> GetFunction();
};

public record ShrinkOptionFunction<T>(Func<T, IEnumerable<T>> Shrinker) : ShrinkOption<T>, IShrinkOptionFunction
{
    public Func<object, IEnumerable<object>> GetFunction()
        => a => Shrinker((T)a).Cast<object>();
}

public interface IShrinkOptionDefault;
public record ShrinkOptionDefault<T>() : ShrinkOption<T>, IShrinkOptionDefault;