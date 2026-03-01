using QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.C_Configuration;
using QuickCheckr.Tests.Docs.J_YouCanGoYourOwnWay.A_Showr;
using QuickCheckr.Tests.Docs.J_YouCanGoYourOwnWay.B_CustomShrinkersOld;
using QuickCheckr.Tests.Docs.J_YouCanGoYourOwnWay.C_ReducersOld;
using QuickCheckr.Tests.Tools;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.J_YouCanGoYourOwnWay;

[DocFile]
[DocLink(typeof(Configuration))]
[DocLink(typeof(A_TheShowrClass))]
[DocLink(typeof(CustomReducersOld))]
[DocExplanationInProgress]
[DocContent(
@"We already saw in the chapter on [Configuration][Configuration] how you can turn some knobs to customize QuickCheckr's behaviour
and/or reporting globally.  
But there are ways to obtain more fine-grained control. That is what this chapter is about.")]
[DocHeader("Contents")]
[DocContent(
@"
- [Showr][A_TheShowrClass]
- [Custom Shrinkers][CustomShrinkers]
- [Reducers][Reducers]
")]

public class YouCanGoYourOwnWay { }