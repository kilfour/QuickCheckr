using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.A_Guide.A_CheckringThingsOut;

[CodeExample]
public class MinimalCheckr
{
    [Fact]
    public void RunTest()
        => Checkr.Spec("Always true", () => true).Run();
}