# Candidate-based Classifiers
Here is an overview of all the different ways you can define *candidates* for classification.  
Using a more relevant example this time (where possible), that allows you to deal with [this][E_CheckingNulls] relatively common issue.  
## Values

**The Checkr:**  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from thing in Checkr.Input("Null Thing",
    Fuzzr.Constant<Thing>(null!),
    Try.Values(new Thing()))
from expect in Checkr.Expect("counter != 2", () => counter != 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_TryValues.cs:73:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    161541321
 ------------------------------------------------------------
  Executed:
   - Input: counter = 2
 =======================================
  !! Expectation Failed: counter != 2
 =======================================
 Passed Expectations
 - counter != 2: 1x
 ------------------------------------------------------------
```
## Collection
This is very similar to `Try.Values`.  
It only exists to avoid generic type soup and easily made mistakes when typing candidates supply for collections.  

**The Checkr:**  
```csharp
from answers in Checkr.Input("answers", Fuzzr.Int().Many(2, 5), Try.Collection([0], [1], [42]))
from expect in Checkr.Expect("all answers equal 42", () => answers.All(a => a == 42))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_TryCollection.cs:56:1
 Original failing run:    1 execution
 Seed:                    899182715
 ------------------------------------------------------------
  Executed:
   - Input: answers = [ 2, 80 ]
 ===============================================
  !! Expectation Failed: all answers equal 42
 ===============================================
```
## Function
This method allows you to supply dynamic candidates using a function.  
The lambda argument of the supplied function, is the original Input value.
  

**The Checkr:**  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from thing in Checkr.Input("Null Thing",
    Fuzzr.Constant<Thing>(null!),
    Try.Function((Thing a) => [new Thing()]))
from expect in Checkr.Expect("counter != 2", () => counter != 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                C_TryFunction.cs:73:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    161541321
 ------------------------------------------------------------
  Executed:
   - Input: counter = 2
 =======================================
  !! Expectation Failed: counter != 2
 =======================================
 Passed Expectations
 - counter != 2: 1x
 ------------------------------------------------------------
```
## Fuzzr

**The Checkr:**  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from thing in Checkr.Input("Null Thing", Fuzzr.Constant<Thing>(null!), Try.Fuzzr(Fuzzr.One<Thing>(), 2))
from expect in Checkr.Expect("counter != 2", () => counter != 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                D_TryFuzzr.cs:73:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    161541321
 ------------------------------------------------------------
  Executed:
   - Input: counter = 2
 =======================================
  !! Expectation Failed: counter != 2
 =======================================
 Passed Expectations
 - counter != 2: 1x
 ------------------------------------------------------------
```

[E_CheckingNulls]: ../../../../../Docs/M_HoneyIShrunkTheInputs/C_Objects/TheExamples/E_CheckingNulls.md
