# Deliberation Shrinking
[On Shrinking][OnShrinking] | [Previous][InputReduction]  
  
This next step, which is disabled by default, allows the shrinker to find *global* minimal failcases instead of *local* ones.  

A first example, as well as how to enable this feature,
can be found in the entry on Deliberation Policies in the [Configuration][Configuration] chapter.

But here are some more use cases.  
## An Eager Shrinker
Under some rare circumstances, the Shrinker gets *greedy*.  
One currently identified case where this happens is if more than one input triggers the same failure.  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(0, 5).Many(3))
from trace in Checkr.Trace("List", () => input)
from expect in Checkr.ExpectWhen("List count > 2 and does not contain 1",
    () => input.Count() > 2,
    () => !input.Contains(1))
select Case.Closed;
```
Now when the list contains multiple elements `1`, we might get:  
```text
------------------------------------------------------------
 Test:                    RunTest_DeliberatedGreedy
 Location:                Docs\H_OnShrinking\F_DeliberationShrinking\DeliberationShrinking.cs:45:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 3 shrinks)
 Seed:                    227109555
 ------------------------------------------------------------
  Executed:
   - Input: Input = [ _, _, _ ]
   - Trace: List  = [ 1, 2, 1 ]
 ================================================================
  !! Expectation Failed: List count > 2 and does not contain 1
 ================================================================
```
It seems QuickCheckr thinks that the minimal fail-case is a list with atleast 3 elements (*correct*),
but the element values are considered irrelevant (*wrong*).  
For this rare case we  add a DeliberationPolicy, allowing a more explanatory fail-case to surface:  
```csharp
private static CheckrConfig ConfigureShrinkCount(CheckrConfig config)
    => config with { DeliberationPolicy = DeliberationPolicy.MinimumShrinkCount };
```
**Report:**  
```text
------------------------------------------------------------
 Test:                    RunTest_DeliberatedGreedyFixed
 Location:                Docs\H_OnShrinking\F_DeliberationShrinking\DeliberationShrinking.cs:82:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1133636239
 ------------------------------------------------------------
  Executed:
   - Input: Input = [ _, _, 1 ]
   - Trace: List  = [ 0, 4, 1 ]
 =============================================
  !! Expectation Failed: Does not contain 1
 =============================================
```
A similar thing can happen when shrinking objects, and the solution is exactly the same  
```csharp
from obj in Checkr.Input("BaggyInters",
    from cfg in Configr.Primitive(Fuzzr.Int(1, 5))
    from result in Fuzzr.One<BaggyInters>()
    select result)
from trace in Checkr.Trace("Original", () => obj)
from expect in Checkr.Expect("obj.IntOne == 1 || obj.IntThree == 1",
    () => obj.IntOne != 1 && obj.IntThree != 1)
select Case.Closed;
```
**With Deliberation Policy:**  
```text
------------------------------------------------------------
 Test:                    RunTest_DeliberatedGreedyObjects
 Location:                Docs\H_OnShrinking\F_DeliberationShrinking\DeliberationShrinking.cs:105:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    149961996
 ------------------------------------------------------------
  Executed:
   - Input: BaggyInters = { IntThree: 1 }
   - Trace: Original    = { IntOne: 3, IntTwo: 2, IntThree: 1 }
 ===============================================================
  !! Expectation Failed: obj.IntOne == 1 || obj.IntThree == 1
 ===============================================================
```

[Configuration]: ../../../Docs/B_PuttingACaseOnTheBoard/C_Configuration/Configuration.md

[OnShrinking]: ../OnShrinking.md

[InputReduction]: ../D_InputReduction/InputReduction.md
