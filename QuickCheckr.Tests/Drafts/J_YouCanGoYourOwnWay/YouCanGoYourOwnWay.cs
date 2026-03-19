using QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.C_Configuration;
using QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.D_CustomShrinkers;
using QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.N_Showr;
using QuickCheckr.Tests.Tools;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay;

[DocFile]
[DocLink(typeof(Configuration))]
[DocLink(typeof(A_TheShowrClass))]
[DocLink(typeof(CustomShrinkers))]
[DocContent(
@"We already saw in the chapter on [Configuration][Configuration] how you can turn some knobs to customize QuickCheckr's behaviour
and/or reporting globally.  
But there are ways to obtain more fine-grained control. That is what this chapter is about.")]
[DocHeader("Contents")]
[DocContent(
@"
- [Custom Shrinkers][CustomShrinkers]
- [Showr][A_TheShowrClass]
")]

public class YouCanGoYourOwnWay : BaseTest
{
    [Fact(Skip = "Doc")]
    public void Doc() => ExplainMe<YouCanGoYourOwnWay>(true);
}