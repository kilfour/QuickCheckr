using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.D_DslBuilding;

[DocFile]
[DocFileHeader("Building a Testing DSL with QuickCheckr")]
public class DslBuilding
{
    [Fact(Skip = "Doc")]
    public void Doc() => Explain.This<DslBuilding>("temp-dsl-building.md");
}