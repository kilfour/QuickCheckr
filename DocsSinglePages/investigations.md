## Expectation Exceptions
### Expectation Throws
In case an expectation throws an exception during the initial Checkr run, 
It is wrapped in an ExpectationThrowsException through InnerException and the expectation key is reported.  

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int())
from act in Checkr.Act("Act", () => (int?)null)
from expect in Checkr.Expect("Expect Throws", () => act.Value == 5)
select Case.Closed;
```

**Message::**  
```csharp
"The expectation 'Expect Throws' threw a 'InvalidOperationException'.";
```
### Caused By Delayed Result

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int())
from act in Checkr.ActCarefully("Act", () => (int?)null)
from expect in Checkr.Expect("Expect Throws", () => act.Value!.Value == 5)
select Case.Closed;
```

**Message::**  
```csharp
"The expectation 'Expect Throws' threw a 'DelayedResultNullValueAccessedException'.";
```
## Greed Is Not Good

**The Checkr:**  
```csharp
from input in Checkr.Input("List", Fuzzr.Constant(new List<int>() { 1, 1 }))
from length in Checkr.Expect("input.Count == 2", () => input.Count == 2)
from expect in Checkr.ExpectWhen("does not contain 1", () => input.Count == 2, () => !input.Contains(1))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_GreedIsNotGood.cs:40:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    2130064221
 ------------------------------------------------------------
  Executed:
   - Input: List = [ _, 1 ]
 =============================================
  !! Expectation Failed: does not contain 1
 =============================================
 Passed Expectations
 - input.Count == 2: 1x
 ------------------------------------------------------------
```
## Nested Collections Inner Count
Not necessarily a bug, but not a helpful report either. 

There is a real bug in the shrink count though.  
Only the outer list shrink (being deemed irrelevant) is counted, minor issue.
  

**The Checkr:**  
```csharp
from input in Checkr.Input("List", Fuzzr.Int().Many(1, 5).Many(3, 10))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect("List count >= 4", () => input.Count() >= 4)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_NestedCollectionsInnerCount.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1427464097
 ------------------------------------------------------------
  Executed:
   - Trace: Original = [ [ 41, 73, 3, 5, 14 ], [ 2, 88, 10 ], [ 59, 61, 21, 32, 34 ] ]
   - WARNING: All inputs were considered irrelevant.
   - WARNING: Empty collection deemed irrelevant at input 'List'.
 ==========================================
  !! Expectation Failed: List count >= 4
 ==========================================
```
## Nested Collections Inner Count
Adding the custom shrinker solves the problem in the report,
but really coming up with that solution is not something you want the user to do.
  

**The Checkr:**  
```csharp
from input in Checkr.Input("List", Fuzzr.Int().Many(1, 5).Many(3, 10),
    Shrink.CollectionFromState<IEnumerable<int>>([[], [], [], [], []], []))
from trace in Checkr.Trace("Original", () => input)
from expect in Checkr.Expect("List count >= 4", () => input.Count() >= 4)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B1_NestedCollectionsInnerCount.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1134566081
 ------------------------------------------------------------
  Executed:
   - Input: List     = [ [ 79, 83, 60, 28, 36 ], [ 92, 99 ], [ 91 ] ]
            Redux      [ ]
   - Trace: Original = [ [ 79, 83, 60, 28, 36 ], [ 92, 99 ], [ 91 ] ]
 ==========================================
  !! Expectation Failed: List count >= 4
 ==========================================
```
