using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.P_WhenThingsGoWrong.D_WhatHappensWhenAnExceptionIsThrown;

[DocFile]
[DocContent(
"""
- Act/Capture/Perform exception   => failure
- ActCarefully                    => inspect/assert exception behaviour
- Anything else                   => throws or rethrows
- Shrinking replay                => never throws
"""
)]
public class WhatHappensWhenAnExceptionIsThrown;