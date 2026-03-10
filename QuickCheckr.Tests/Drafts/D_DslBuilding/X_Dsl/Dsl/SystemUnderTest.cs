namespace QuickCheckr.Tests.Drafts.D_DslBuilding.X_Dsl.Dsl;

public class SystemUnderTest
{
    public CheckrOf<Case> Operations(params Operation[] operations) =>
        Checkr.OneOfWhen([.. operations.Select(a => a.Definition(this))]);

    public CreateBuilder<TPoolElement> Create<TPoolElement>() => new($"Create {typeof(TPoolElement).Name}");

    public UpdateBuilder<TPoolElement> Update<TPoolElement>(string methodLabel) => new($"Update {typeof(TPoolElement).Name} {methodLabel}");

    public CombinedUpdateBuilder<TPoolElement1, TPoolElement2> Update<TPoolElement1, TPoolElement2>(string methodLabel)
        => new($"Update ({typeof(TPoolElement1).Name}, {typeof(TPoolElement2).Name}) {methodLabel}");
}
