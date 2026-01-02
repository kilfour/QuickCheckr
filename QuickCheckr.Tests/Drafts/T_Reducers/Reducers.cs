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

Not sure if it counts as *real world* because it came about while teaching,
but it is very useful nonetheless.  

With this feature we're venturing closer too the QuickCheck style,
and I actually tried to use other existing libs first, but that did not work out all that well.  
Granted, I'm biased, I wrote this thing after all,
so ofcourse it's going to be easier for me to incorporate it into my own lib
than to struggle learning someone else's.  

With that caveat out of the way, let's dig in.  

There exist **two different types** of reducers:
")]
[DocInclude(typeof(SimpleReducers))]
[DocInclude(typeof(FullReducers))]
public class Reducers;