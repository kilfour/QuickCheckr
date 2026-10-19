using QuickCheckr.Authoring.ThePress;
using QuickFuzzr;

namespace QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.N_Showr;

public class ClassificationRegistrationTests
{
    [Fact]
    public async Task SameTypeCanBeConfiguredAcrossExecutionsAndRuns()
    {
        var executions = 0;
        var check =
            from show in Showr.ForClassification(a => a.For<Query>()
                .Property(q => q.Name)
                .Bounds(Environment.NewLine, "")
                .Delimiter($",{Environment.NewLine}"))
            from query in Checkr.Input("Query", Fuzzr.Constant(new Query { Name = "123" }))
            from count in Checkr.Act("Count execution", () => executions++)
            from valid in Checkr.Expect("Query retained", () => query.Name == "123")
            select Case.Closed;

        await check.RunAsync(3.Runs(), 3.ExecutionsPerRun(), TestContext.Current.CancellationToken);

        Assert.Equal(9, executions);
    }

    [Fact]
    public void RepeatedConfigurationComposesWithoutDuplicatePropertiesInReport()
    {
        var check =
            from bounds in Showr.ForClassification(a => a.For<Query>()
                .Bounds("<", ">")
                .Property(q => q.Name))
            from delimiter in Showr.ForClassification(a => a.For<Query>()
                .Delimiter(" | ")
                .Property(q => q.Name))
            from query in Checkr.Input("Query", Fuzzr.Constant(new Query { Name = "123" }))
            from fail in Checkr.Expect("Name is not 123", () => query.Name != "123")
            select Case.Closed;

        var article = Journalist.Publish(check, a => a.Run(123));

        Assert.Equal("Name is not 123", article.FailureDescription());
        Assert.Equal("<Name: \"123\">", article.Execution(1).Input(1).Read().Value);
    }

    public sealed class Query
    {
        public string Name { get; init; } = "";
    }
}
