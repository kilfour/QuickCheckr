using QuickFuzzr;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;

/// <summary>
/// Identifies a classification strategy and the input type it targets.
/// </summary>
public interface IClassificationOption
{
    public Type TargetType { get; }
}

/// <summary>
/// Represents a classification strategy for inputs of type <typeparamref name="T"/>.
/// </summary>
public abstract record ClassificationOption<T> : IClassificationOption
{
    public Type TargetType => typeof(T);
}

internal interface IClassificationOptionFuzzr
{
    public FuzzrOf<object> GetFuzzr();
    public int Times { get; }
}

internal sealed record ClassificationOptionFuzzr<T>(FuzzrOf<T> Fuzzr, int Times) : ClassificationOption<T>, IClassificationOptionFuzzr
{
    public FuzzrOf<object> GetFuzzr()
        => Fuzzr.AsObject();
}

internal interface IClassificationOptionFunction
{
    public Func<object, IEnumerable<object>> GetFunction();
};

internal sealed record ClassificationOptionFunction<T>(Func<T, IEnumerable<T>> Shrinker) : ClassificationOption<T>, IClassificationOptionFunction
{
    public Func<object, IEnumerable<object>> GetFunction()
        => a => Shrinker((T)a).Cast<object>();
}

internal interface IClassificationOptionDefault;
internal sealed record ClassificationOptionDefault<T>() : ClassificationOption<T>, IClassificationOptionDefault;

internal interface IClassificationOptionNone;
internal sealed record ClassificationOptionNone<T>() : ClassificationOption<T>, IClassificationOptionNone;

internal interface IClassificationOptionKeep;
internal sealed record ClassificationOptionKeep<T>() : ClassificationOption<T>, IClassificationOptionKeep;
