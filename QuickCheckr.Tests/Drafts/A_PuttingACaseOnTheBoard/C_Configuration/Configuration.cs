using QuickPulse.Explains;


namespace QuickCheckr.Tests.Drafts.C_Configuration;

[DocFile]
[DocContent(
@"QuickCheckr runs are configured through an immutable `CheckrConfig` record. 
You typically start from `CheckrConfig.Default()` and then tweak the parts you care about")]
public class Configuration;