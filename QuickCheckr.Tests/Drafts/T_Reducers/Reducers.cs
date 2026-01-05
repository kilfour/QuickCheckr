using QuickCheckr.Tests.Drafts.T_Reducers.A_Simple;
using QuickCheckr.Tests.Drafts.T_Reducers.B_Full;
using QuickCheckr.Tests.Tools;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.T_Reducers;

[DocFile]
[DocUnderConstruction]
[DocContent(
@"Reducers are a relatively new addition to the QuickCheckr arsenal.  
And you could, rightfully so, point out that they violate it's core philosophy
of not rewriting what actually happened.  

But once again, it is a feature born out of necessity.  
 
With that caveat out of the way, let's dig in.  

There exist **two different types** of reducers:
 - [Simple Reducers][SimpleReducers]
 - [Multi Step Reducers][MultiStepReducers]
")]
[DocInclude(typeof(SimpleReducers))]
[DocInclude(typeof(MultiStepReducers))]
public class Reducers;