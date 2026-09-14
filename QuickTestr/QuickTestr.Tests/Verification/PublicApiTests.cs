using System.Reflection;
using QuickTestr.Bolts.Builders;
using QuickTestr.Bolts.Builders.ModelBased;
using QuickTestr.Bolts.Builders.Oracle;
using QuickTestr.Bolts.Builders.Property;

namespace QuickTestr.Tests.Verification;

public class PublicApiTests
{
    [Fact]
    public void PublicConcreteTypesAreSealed()
    {
        var unsealedTypes = typeof(Testr).Assembly
            .GetExportedTypes()
            .Where(type => type.IsClass && !type.IsAbstract && !type.IsSealed)
            .Select(type => type.FullName)
            .ToList();

        Assert.Empty(unsealedTypes);
    }

    [Fact]
    public void FluentTypesCannotBeConstructedDirectly()
    {
        Type[] fluentTypes =
        [
            typeof(NamedTestr),
            typeof(InputTestr<>),
            typeof(InputTestrT2<,>),
            typeof(DeliberatedTestr<>),
            typeof(DeliberatedTestrT2<,>),
            typeof(OracleTestr<,>),
            typeof(OracleTestrT2<,,>),
            typeof(TestrPropertyRunner<>),
            typeof(TestrPropertyRunnerT2<,>),
            typeof(TestrOracleRunner<,>),
            typeof(TestrOracleRunnerT2<,,>),
            typeof(WithModel<>),
            typeof(WithSut<,>),
            typeof(WithOperations<,>),
            typeof(Tracer<,>),
            typeof(TestrRunner<>)
        ];

        Assert.All(
            fluentTypes,
            type => Assert.Empty(type.GetConstructors(BindingFlags.Public | BindingFlags.Instance)));
    }
}
