using QuickCheckr.Tests.Docs.H_OnShrinking.A_ExecutionShrinking;
using QuickCheckr.Tests.Docs.H_OnShrinking.C_InputShrinking;
using QuickCheckr.Tests.Docs.H_OnShrinking.D_InputReduction;
using QuickCheckr.Tests.Docs.H_OnShrinking.F_DeliberationShrinking;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.H_OnShrinking;

[DocFile]
[DocHeader("Philosophy")]
[DocContent(
@"
QuickCheckr keeps shrinking simple and predictable.  
It never tries to *outsmart* your domain with clever global algorithms.  
Shrinking is purely local and irrelevance-based: if removing a value (or execution) still makes the property fail,
QuickCheckr marks it as *irrelevant*.  
This avoids surprising or *magical* shrink results.")]
[DocLink(typeof(ExecutionShrinking))]
[DocLink(typeof(InputShrinking))]
[DocLink(typeof(InputReduction))]
[DocLink(typeof(DeliberationShrinking))]
[DocHeader("Contents")]
[DocContent(
@"
- [Execution Shrinking][ExecutionShrinking]
- [Input Shrinking][ExecutionShrinking]
- [Input Reduction][Input Reduction]
- [Deliberation Shrinking][DeliberationShrinking]
")]

public class OnShrinking { }