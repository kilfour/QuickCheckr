# Policy-based Classifiers
  
## Keep
`Try.Keep` explicitly marks an execution as relevant and worth preserving during shrinking.
In most scenarios this is not strictly necessary, as QuickCheckr will naturally retain behaviour that contributes to failure.
`Try.Keep` mainly exists for completeness and for rare performance-tuning cases where you want to prevent the shrinker from exploring removals you already know are pointless.
It also serves as an explicit signal of intent: this step is considered essential evidence.  

**The Checkr:**  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from thing in Checkr.Input("Null Thing",
    Fuzzr.Constant<Thing>(null!),
    Shrink.OnType<Thing>(s => s.Classify(Try.Keep<Thing>())))
from expect in Checkr.Expect("counter != 2", () => counter != 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_TryKeep.cs:46:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    161541321
 ------------------------------------------------------------
  Executed:
   - Input: counter    = 2
   - Input: Null Thing = null
 =======================================
  !! Expectation Failed: counter != 2
 =======================================
 Passed Expectations
 - counter != 2: 1x
 ------------------------------------------------------------
```
## Default
`Try.Keep` applies QuickCheckr's normal relevance detection as one strategy within a classifier chain.
Classifiers are executed in succession, so Try.Default can be placed anywhere in the sequence.
If it finds a relevant candidate, the chain stops. Otherwise, the next classifier is attempted.
This makes it possible to combine engine-driven relevance detection with more opinionated strategies such as Try.Values(...)
without hard-coding one as the global default.  

**The Checkr:**  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from thing in Checkr.Input("Null Thing",
    Fuzzr.Constant<Thing>(null!),
    Shrink.OnType<Thing>(s => s.Classify(Try.Default<Thing>())))
from expect in Checkr.Expect("counter != 2", () => counter != 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                C_TryDefault.cs:46:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    161541321
 ------------------------------------------------------------
  Executed:
   - Input: counter    = 2
   - Input: Null Thing = null
   - WARNING: Null value for type 'Thing' at input 'Null Thing'.
 =======================================
  !! Expectation Failed: counter != 2
 =======================================
 Passed Expectations
 - counter != 2: 1x
 ------------------------------------------------------------
```
