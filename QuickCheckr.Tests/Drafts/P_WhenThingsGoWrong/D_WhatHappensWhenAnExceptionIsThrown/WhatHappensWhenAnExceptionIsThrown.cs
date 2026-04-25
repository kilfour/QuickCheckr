using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.P_WhenThingsGoWrong.D_WhatHappensWhenAnExceptionIsThrown;

[DocFile]
[DocContent(
""""
- Whenever an exception is thrown in an 'Act', 'Capture' or 'Perform' it is considered a failure.
- ActCarefully for asserting exceptions and exception behaviour
- everything else BOOM
- never BOOM during shrinking
""""
)]
public class WhatHappensWhenAnExceptionIsThrown;