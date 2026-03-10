namespace QuickCheckr.Tests.Drafts.D_DslBuilding.K_Dsl.Dsl;

public abstract class Operation
{
    public abstract CheckrOf<(Func<bool>, CheckrOf<Case>)> Definition(SystemUnderTest api);
}
