using QuickCheckr.Tests.Docs.A_Guide.B_TheStashRunsAndExecutionsChapter;
using QuickCheckr.Tests.Docs.A_Guide.D_InPoolsWeTrust;
using QuickCheckr.Tests.Tools;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.A_Guide.C_ChoosingTheNextMoveChapter;

[DocFile]
[DocFileHeader("Choosing the Next Move")]
[DocNavigation(typeof(Guide), typeof(A_TheStashRunsAndExecutions), typeof(InPoolsWeTrust))]
[DocContent(
@$"In the previous chapter we mentioned *Pools* and promised to explain how they work.  
We'll get there soon, but first we need to talk about `{nameof(Checkr)}.{nameof(Checkr.OneOf)}`, the tool that lets QuickCheckr explore branching behaviour.

`{nameof(Checkr.OneOf)}` is QuickCheckr's way of saying:

> **""At this step, choose *exactly one* of these possible behaviours and execute it.""**

It is the foundational building block for introducing randomness into *what happens next*.
Instead of describing one action, you describe a *set* of alternative next moves,
and QuickCheckr picks one at random during each execution.

`OneOf` turns your Checkr into a **state machine explorer**.")]
[DocInclude(typeof(B_ChoosingTheNextMoveNoShrinking), true)]
[DocInclude(typeof(C_ChoosingTheNextMoveShrunk))]
[DocInclude(typeof(D_ChoosingTheNextMoveConditional))]
public class A_ChoosingTheNextMove;