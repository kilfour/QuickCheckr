using QuickFuzzr;

namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Custom;

public interface IClassificationOption
{
    public Type TargetType { get; }
}

public abstract record ClassificationOption<T> : IClassificationOption
{
    public Type TargetType => typeof(T);
}

public interface IClassificationOptionFuzzr
{
    public FuzzrOf<object> GetFuzzr();
    public int Times { get; }
}

public record ClassificationOptionFuzzr<T>(FuzzrOf<T> Fuzzr, int Times) : ClassificationOption<T>, IClassificationOptionFuzzr
{
    public FuzzrOf<object> GetFuzzr()
        => Fuzzr.AsObject();
}

public interface IClassificationOptionFunction
{
    public Func<object, IEnumerable<object>> GetFunction();
};

public record ClassificationOptionFunction<T>(Func<T, IEnumerable<T>> Shrinker) : ClassificationOption<T>, IClassificationOptionFunction
{
    public Func<object, IEnumerable<object>> GetFunction()
        => a => Shrinker((T)a).Cast<object>();
}

public interface IClassificationOptionDefault;
public record ClassificationOptionDefault<T>() : ClassificationOption<T>, IClassificationOptionDefault;

public interface IClassificationOptionNone;
public record ClassificationOptionNone<T>() : ClassificationOption<T>, IClassificationOptionNone;

public interface IClassificationOptionKeep;
public record ClassificationOptionKeep<T>() : ClassificationOption<T>, IClassificationOptionKeep;