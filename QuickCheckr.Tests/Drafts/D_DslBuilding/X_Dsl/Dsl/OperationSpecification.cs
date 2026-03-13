namespace QuickCheckr.Tests.Drafts.D_DslBuilding.X_Dsl.Dsl;

public record OperationSpecification(
    CheckrOf<(Func<bool>, CheckrOf<Case>)> Checkr)
{

}
