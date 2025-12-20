# Deliberation Shrinking
This next step, which is disabled by default, allows the shrinker to find *global* minimal failcases instead of *local* ones.

We already saw an example in the entry on 'Deliberation Policies' in the Configuration chapter.

But here are some more use cases.  
## An Eager Shrinker
Under some rare circumstances, the Shrinker gets *greedy*.  
Currently the only identified cases where this happens is if more than one input triggers the same failure.  
```csharp
    from input in Checkr.Input("Input", Fuzzr.Int(0, 5).Many(3))
    from trace in Checkr.Trace("List", () => input)
    from expect in Checkr.ExpectWhen("Does not contain 1",
        () => input.Count() > 2,
        () => !input.Contains(1))
    select Case.Closed;
```
Now when the list contains multiple elements `1`, we might get:  
```text
------------------------------------------------------------
 Test:                    RunTest_DeliberatedGreedy
 Location:                Docs\H_OnShrinking\F_DeliberationShrinking\DeliberationShrinking.cs:38:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 3 shrinks)
 Seed:                    227109555
 ------------------------------------------------------------
  Executed:
   - Input: Input = [ _, _, _ ]
   - Trace: List = [ 1, 2, 1 ]
 =============================================
  !! Expectation Failed: Does not contain 1
 =============================================
```
It seems QuickCheckr thinks that the minimal fail-case is a list with atleast 3 elements (*correct*),
but the element values are considered irrelevant (*wrong*).  
For this rare case we  add a DeliberationPolicy, allowing QuickFuzzr to surface a more explanatory fail-case:  
```csharp
private static CheckrConfig ConfigureShrinkCount(CheckrConfig config)
    => config with { DeliberationPolicy = Protocol.DeliberationPolicy.MinimumShrinkCount };
```
**Report:**  
```text
------------------------------------------------------------
 Test:                    RunTest_DeliberatedGreedyFixed
 Location:                Docs\H_OnShrinking\F_DeliberationShrinking\DeliberationShrinking.cs:79:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    221913277
 ------------------------------------------------------------
  Executed:
   - Input: Input = [ _, _, 1 ]
   - Trace: List = [ 0, 3, 1 ]
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
**With DeliberationPolicy:**  
```text
------------------------------------------------------------
 Test:                    RunTest_DeliberatedGreedyObjects
 Location:                Docs\H_OnShrinking\F_DeliberationShrinking\DeliberationShrinking.cs:102:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1561553014
 ------------------------------------------------------------
  Executed:
   - Input: BaggyInters = { IntThree: 1 }
   - Trace: Original = { IntOne: 3, IntTwo: 2, IntThree: 1 }
 ===============================================================
  !! Expectation Failed: obj.IntOne == 1 || obj.IntThree == 1
 ===============================================================
```
