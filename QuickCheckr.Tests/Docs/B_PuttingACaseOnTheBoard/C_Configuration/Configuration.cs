using QuickCheckr.Protocol;
using QuickPulse.Explains;
using QuickPulse.Show.Bolts;


namespace QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.C_Configuration;

[DocFile]
[DocContent(
@"QuickCheckr runs are configured through an immutable `CheckrConfig` record. 
You typically start from `CheckrConfig.Default()` and then tweak the parts you care about.

All configuration options live in the `QuickCheckr.Protocol` namespace.")]
public class Configuration
{
    [CodeSnippet]
    [CodeRemove("return new CheckrConfig()")]
    [CodeRemove(";")]
    private static CheckrConfig DefaultSettings()
    {
        return new CheckrConfig()
        {
            ShrinkMode = ShrinkMode.Default,
            DeliberationPolicy = null,
            FileAs = string.Empty,
            Custodian = Custodian.Default,
            Investigations = [],
        };
    }

    [Fact]
    [DocContent("**Default Configuration:**")]
    [DocExample(typeof(Configuration), nameof(DefaultSettings))]
    public void DefaultConfig()
    {
        var expected = DefaultSettings();
        var actual = CheckrConfig.Default();
        Assert.Equal(expected.ShrinkMode, actual.ShrinkMode);
        Assert.Equal(expected.DeliberationPolicy, actual.DeliberationPolicy);
        Assert.Equal(expected.FileAs, actual.FileAs);
        // Assert below touches the file system
        // Assert.IsType<Custodian.FileRecorder>(actual.Custodian.Recorder("test"));
        Assert.Equal(expected.Investigations, actual.Investigations);
        // more ?
    }
}