using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.P_WhenThingsGoWrong.NoOptionsAvailable;

[DocFile]
[DocFileHeader("OneOf")]
[DocContent("Throws an `ArgumentException` when no Checkrs are provided.")]
public class A_OneOf
{
    [Fact]
    public void NoOptionsThrows()
    {
        var ex = Assert.Throws<ArgumentException>(() => Checkr.OneOf().Run());
        Assert.Equal("You need to provide atleast one Checkr. (Parameter 'scripts')", ex.Message);
    }
}
