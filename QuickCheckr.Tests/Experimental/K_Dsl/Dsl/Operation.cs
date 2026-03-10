namespace QuickCheckr.Tests.Experimental.K_Dsl.Dsl;

public abstract class Operation
{
    public abstract CheckrOf<(Func<bool>, CheckrOf<Case>)> Definition(SystemUnderTest api);
}
