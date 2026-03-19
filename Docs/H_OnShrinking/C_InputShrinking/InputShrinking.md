# Input Shrinking
[On Shrinking][OnShrinking] | [Previous][ExecutionShrinking] | [Next][InputReduction]  
  
This is the next step in QuickCheckr shrinking and the most complex one.  

In QuickCheckr, **input shrinking is a two-step process**:

1. **Classification**: decide *what is relevant* for reproducing the failure.
2. **Reduction (Redux)**: simplify the *relevant* parts to improve human understanding, while still reproducing the failure.

A key principle: the engine always keeps the **original failing input**.
Reduction produces a **Redux view** alongside the original.
The report can therefore show both **Original** and **Redux** without pretending the original counterexample was replaced.  
Reduction is (by default) an opt-in feature. In this chapter we are mainly demonstrating Classification.
Reduction will be elaborated on later.
  
The following simple example has one input but always fails.  
```csharp
from irrelevant in Checkr.Input("Irrelevant Input", Fuzzr.Int())
from expect in Checkr.Expect("Always Fails", () => false)
select Case.Closed;
```
Input shrinking reruns the failing execution and prunes inputs whose values don't affect the failure.

So, in the previous example, after *Input Shrinking*, the result is:
  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Docs\H_OnShrinking\C_InputShrinking\InputShrinking.cs:39:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    165641598
 ------------------------------------------------------------
  Executed:
   - WARNING: All inputs were considered irrelevant.
 =======================================
  !! Expectation Failed: Always Fails
 =======================================
```
Another example, slightly trickier.  
```csharp
from one in Checkr.Input("Input One", Fuzzr.Int())
from two in Checkr.Input("Input Two", Fuzzr.Int())
from three in Checkr.Input("Input Three", Fuzzr.Int())
from result in Checkr.Act("Result", () => one + three)
from expect in Checkr.Expect("Should not be 42", () => result != 42)
select Case.Closed;
```
As you can see from the test definition, two inputs are involved in a possible failure.

QuickCheckr correctly identifies the relevant inputs.

Result:
  
```text
------------------------------------------------------------
 Test:                    Example_Two
 Location:                Docs\H_OnShrinking\C_InputShrinking\InputShrinking.cs:81:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    231122398
 ------------------------------------------------------------
  Executed: Result
   - Input: Input One   = 12
   - Input: Input Three = 30
 ===========================================
  !! Expectation Failed: Should not be 42
 ===========================================
```
Many more examples focused on shrinking behaviour can be found [here][HoneyIShrunkTheInputs].  

[HoneyIShrunkTheInputs]: ../../../Drafts/M_HoneyIShrunkTheInputs/HoneyIShrunkTheInputs.md

[OnShrinking]: ../OnShrinking.md

[ExecutionShrinking]: ../A_ExecutionShrinking/ExecutionShrinking.md

[InputReduction]: ../D_InputReduction/InputReduction.md
