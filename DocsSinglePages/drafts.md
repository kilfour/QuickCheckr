## All Inputs Shrunk

**The Checkr:**  
```csharp
from input in Checkr.Input("An Input", Fuzzr.Int())
from expect in Checkr.Expect("Always fails", () => false)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                AllInputsShrunk.cs:74:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1098916919
 ------------------------------------------------------------
  Executed:
   - WARNING: All inputs were considered irrelevant.
 =======================================
  !! Expectation Failed: Always fails
 =======================================
```
  
  
## Registered But Unused

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(), () => Shrink.Values(["unused"]))
from expect in Checkr.Expect("Input < 2", () => input < 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                RegisteredButUnused.cs:58:1
 Original failing run:    1 execution
 Seed:                    1681332589
 ------------------------------------------------------------
  Executed:
   - Input: Input = 79
   - WARNING: Unused shrinker found for 'String' at key 'Input'.
 ====================================
  !! Expectation Failed: Input < 2
 ====================================
```
## Shrink Default

**The Checkr:**  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from thing in Checkr.Input("Null Thing",
    Fuzzr.Constant<Thing>(null!),
    () => Shrink.Default<Thing>())
from expect in Checkr.Expect("counter != 2", () => counter != 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShrinkDefault.cs:57:1
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
## Shrink Function

**The Checkr:**  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from thing in Checkr.Input("Null Thing",
    Fuzzr.Constant<Thing>(null!),
    // The lamda argument is the original Input value
    () => Shrink.Function((Thing a) => [new Thing()]))
from expect in Checkr.Expect("counter != 2", () => counter != 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShrinkFunction.cs:57:1
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
## Shrink Fuzzr

**The Checkr:**  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from thing in Checkr.Input("Null Thing", Fuzzr.Constant<Thing>(null!), () => Shrink.Fuzzr(Fuzzr.One<Thing>(), 2))
from expect in Checkr.Expect("counter != 2", () => counter != 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShrinkFuzzr.cs:57:1
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
## Shrink Multiple

**The Checkr:**  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from thing in Checkr.Input("Null Thing",
    Fuzzr.Constant<Thing>(null!),
    () => [Shrink.Values([new Thing()]), Shrink.Function((Thing a) => [new Thing()])])
from expect in Checkr.Expect("counter != 2", () => counter != 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShrinkMultiple.cs:57:1
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
## Shrink None

**The Checkr:**  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from thing in Checkr.Input("Null Thing",
    Fuzzr.Constant<Thing>(null!),
    () => Shrink.None<Thing>())
from expect in Checkr.Expect("counter != 2", () => counter != 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShrinkNone.cs:57:1
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
## Shrink Values

**The Checkr:**  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from thing in Checkr.Input("Null Thing",
    Fuzzr.Constant<Thing>(null!),
    () => Shrink.Values([new Thing()]))
from expect in Checkr.Expect("counter != 2", () => counter != 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShrinkValues.cs:57:1
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
## Checkrs Court
The basic building block of QuickCheckr is a `CheckrOf<T>` (also, informally, called a *Checkr* throughout these docs). 

Multiple Checkrs can be combined into a composite Checkr using `LINQ` chains, as many of the examples show.  

The *atomic* Checkrs are created through a couple of static Factory classes :

- [The Checkr Class][TheCheckrClass]
  - Provides methods to create the *basic* Checkrs.
  - All of the Checkrs created by this class are *execution-scoped*.
- [The Tracker Class][TheTrackerClass]
  - Provides methods to create the Checkrs that deal with state-mutation, and/or Checkrs that are *run-scoped*.  
- [The Tracker Class][TheShowrClass]
  - Allows for customization of the QuickChecker value printer.  
  

[TheCheckrClass]: #the-checkr-class

[TheTrackerClass]: #the-tracker-class

[TheShowrClass]: #the-showr-class
### The Checkr Class
#### Input
`Checkr.Input()` is the only way an *input* can be defined inside a Checkr.  
Inputs serve as shrinkable parameters that in all likelyhood will be consumed by an Act Checkr down the line.  

**Usage**  
```csharp
Checkr.Input("AnInt", Fuzzr.Int());
```
###### Exception Handling
The Checkr below will throw a QuickFuzzr `OneOfEmptyOptionsException`:  
```csharp
    from input in Checkr.Input("AnInt", Fuzzr.OneOf<int>((List<int>)[]))
    select Case.Closed;
```
Whenever an input throws, the exception is wrapped into a `InputFuzzrThrewException`.  
```csharp
Input 'AnInt' threw a 'OneOfEmptyOptionsException', see InnerException.
This might mean something is wrong with the way you wrote the Fuzzr.
But it might also mean that the fuzzr is depending on state.
And as 'Input' is eagerly evaluated, this can cause problems.
In the latter case please try using the 'lazy' version. 
Example: 'Checkr.Input(() => Fuzzr.Int()).
```
As you can gather from the exception message, this is done in order to inform the user of a rare, but real,
issue when a Fuzzr depends on external state.  
The solution is also supplied in said message:   
```csharp
Checkr.Input("AnInt", () => Fuzzr.Int());
```
If the *lazy* version throws:  
```csharp
    from input in Checkr.Input("AnInt", () => Fuzzr.OneOf<int>((List<int>)[]))
    select Case.Closed;
```
The Exception is **not** wrapped.  
```csharp
Fuzzr.OneOf<Int32> cannot select from an empty sequence.
Possible solutions:
- Provide at least one option (ensure the sequence is non-empty).
- Use a fallback: Fuzzr.OneOf(values).WithDefault()
- Guard upstream: values.Any() ? Fuzzr.OneOf(values) : Fuzzr.Constant(default!).
```
### The Tracker Class
### The Showr Class
```csharp
    from acc in Trackr.Stashed(() => new Accumelator())
    from count in Checkr.Input("Counter", Fuzzr.Counter("count"))
    from show in Showr.ForInput(() => Please.AllowMe().ToReplace<int>(a => $"Int {a}"))
    from a in Checkr.Input("a", Fuzzr.Int())
    from b in Checkr.Input("b", Fuzzr.Int())
    from c in Checkr.Input("c", Fuzzr.Int())
    from add in Checkr.Act("Add", () => acc.Add(a, b))
    from notAnswerOnTwo in Checkr.Expect("not 42 on execution 2", () => !(count == 2 && acc.Total == 42))
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Docs\K_CheckrsCourt\C_TheShowrClass\TheShowrClass.cs:20:1
 Original failing run:    2 executions
 Minimal failing case:    2 executions (after 3 shrinks)
 Seed:                    344916298
 ------------------------------------------------------------
  Executed: Add
   - Input: a = 8
                Int 8
   - Input: b = 16
                Int 16
 ------------------------------------------------------------
  Executed: Add
   - Input: Counter = 2
   - Input: a       = 1
                      Int 1
   - Input: b       = 17
                      Int 17
 ================================================
  !! Expectation Failed: not 42 on execution 2
 ================================================
 Passed Expectations
 - not 42 on execution 2: 1x
 ------------------------------------------------------------
```
## Conditional Execution
### Checkr Act Carefully When
```csharp
    from counter in Checkr.Input("Counter", Fuzzr.Counter("key"))
    from thing in Checkr.ActCarefullyWhen("Act", () => counter != 2, () => counter == 3 ? null : new Thing())
    from notNull in Checkr.Expect("Not Null", () => thing.Value != null)
    select Case.Closed;
```
### Checkr Act When
```csharp
    from counter in Checkr.Input("Counter", Fuzzr.Counter("key"))
    from thing in Checkr.ActWhen("Act", () => counter != 2, () => counter == 3 ? null : new Thing())
    from notNull in Checkr.Expect("Not Null", () => thing.Value != null)
    select Case.Closed;
```
```csharp
    from counter in Checkr.Input("Counter", Fuzzr.Counter("key"))
    from thing in Checkr.ActWhen("Act", () => counter != 2, Task.FromResult(counter == 3 ? null : new Thing()))
    from notNull in Checkr.Expect("Not Null", () => thing.Value != null)
    select Case.Closed;
```
### Checkr When
```csharp
    from counter in Checkr.Input("Counter", Fuzzr.Counter("key"))
    from thing in Checkr.When(() => counter != 2, Checkr.Act("Act", () => counter == 3 ? null : new Thing()))
    from notNull in Checkr.Expect("Not Null", () => thing.Value != null)
    select Case.Closed;
```
## When Things Go Wrong
### Everything Means Less Than Null
#### Trackr To Pool
```csharp
    from one in Trackr.ToPool("answer", 42)
    select Case.Closed;
```
  Throws:  
```text
A storage attempt was made for a 'Pool' of 'System.Int32' using key 'answer', but no such pool was found.
Make sure you created this pool using: 'Trackr.PoolFor<T>()'.
```
### It Is Written, ... But Only Once
In QuickCheckr, the strings you pass to `Trackr.Stashed`, `Checkr.Input`, `Checkr.Act`, etcetera,
are not just pretty captions in a report.

They function as **stable identifiers**.

QuickCheckr uses each key for two distinct purposes:

1. **Reporting:** The key appears in execution traces, minimal fail-cases, and summaries.
2. **Book-keeping:** Internally, the engine uses the same key to label stored values, look up decisions, and coordinate shrinking.

Because of this dual role, **each key may only be written once per relevant scope**.  

If you try to write the same key in the same scope twice or more, using the same Checkr, an exception is thrown.
  
#### Checkr Act
```csharp
    from one in Checkr.Act("reused", () => { })
    from two in Checkr.Act("reused", () => { })
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.Act'
- 'Checkr.ActCarefully'
```
#### Checkr Act Carefully
```csharp
    from one in Checkr.ActCarefully("reused", () => { })
    from two in Checkr.ActCarefully("reused", () => { })
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.Act'
- 'Checkr.ActCarefully'
```
#### Checkr Expect
```csharp
    from one in Checkr.Expect("reused", () => true)
    from two in Checkr.Expect("reused", () => true)
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.Expect'
- 'Trackr.Expect'
- 'Trackr.Verify'
- 'Trackr.StopWhenProven'
```
#### Checkr Input
```csharp
    from one in Checkr.Input("reused", Fuzzr.Int())
    from two in Checkr.Input("reused", Fuzzr.Int())
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.Input'
```
#### Checkr One Of
```csharp
    from one in Checkr.OneOf(Checkr.Act("One", () => { }))
    from two in Checkr.OneOf(Checkr.Act("Two", () => { }))
    select Case.Closed;
```
  Throws:  
```text
Key 'Checkr|OneOf|Default|QuickCheckr|Key' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.OneOf'
- 'Checkr.OneOfWhen'
```
```csharp
    from one in Checkr.OneOf("reused", Checkr.Act("One", () => { }))
    from two in Checkr.OneOf("reused", Checkr.Act("Two", () => { }))
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.OneOf'
- 'Checkr.OneOfWhen'
```
#### Checkr One Of When
```csharp
    from one in Checkr.OneOfWhen(Checkr.Option(() => true, Checkr.Act("One", () => { })))
    from two in Checkr.OneOfWhen(Checkr.Option(() => true, Checkr.Act("Two", () => { })))
    select Case.Closed;
```
  Throws:  
```text
Key 'Checkr|OneOfWhen|Default|QuickCheckr|Key' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.OneOf'
- 'Checkr.OneOfWhen'
```
```csharp
    from one in Checkr.OneOfWhen("reused", Checkr.Option(() => true, Checkr.Act("One", () => { })))
    from two in Checkr.OneOfWhen("reused", Checkr.Option(() => true, Checkr.Act("Two", () => { })))
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.OneOf'
- 'Checkr.OneOfWhen'
```
#### Checkr Trace
```csharp
    from one in Checkr.Trace("reused", () => "")
    from two in Checkr.Trace("reused", () => "")
    from expect in Checkr.Expect("fails", () => false) // <= this one needs to run
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.Trace'
- 'Checkr.TraceWhen'
```
#### Checkr Trace When
```csharp
    from one in Checkr.TraceWhen("reused", () => true, () => "")
    from two in Checkr.TraceWhen("reused", () => true, () => "")
    from expect in Checkr.Expect("fails", () => false) // <= this one needs to run
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.Trace'
- 'Checkr.TraceWhen'
```
#### Trackr Expect
```csharp
    from one in Trackr.Expect("reused", () => true)
    from two in Trackr.Expect("reused", () => true)
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.Expect'
- 'Trackr.Expect'
- 'Trackr.Verify'
- 'Trackr.StopWhenProven'
```
#### Trackr Pool For
```csharp
    from one in Trackr.PoolFor<int>()
    from two in Trackr.PoolFor<int>()
    select Case.Closed;
```
  Throws:  
```text
Key 'System.Int32' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Trackr.PoolFor'
```
#### Trackr Stashed
```csharp
    from one in Trackr.Stashed(() => 42)
    from two in Trackr.Stashed(() => 666)
    select Case.Closed;
```
  Throws:  
```text
Key 'System.Int32' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Trackr.Stashed'
```
```csharp
    from one in Trackr.Stashed("reused", () => 42)
    from two in Trackr.Stashed("reused", () => 666)
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Trackr.Stashed'
```
#### Trackr Stop When Proven
```csharp
    from one in Trackr.StopWhenProven("reused", () => false)
    from two in Trackr.StopWhenProven("reused", () => false)
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.Expect'
- 'Trackr.Expect'
- 'Trackr.Verify'
- 'Trackr.StopWhenProven'
```
#### Trackr Verify
```csharp
    from one in Trackr.Verify("reused", () => true)
    from two in Trackr.Verify("reused", () => true)
    select Case.Closed;
```
  Throws:  
```text
Key 'reused' is written more than once to the same memory location.
QuickCheckr treats keys as stable identifiers.
Keys must be unique per memory bank.
Reusing a key for multiple write operations makes reports and shrinking ambiguous.
The following Checkrs write to the this bank:
- 'Checkr.Expect'
- 'Trackr.Expect'
- 'Trackr.Verify'
- 'Trackr.StopWhenProven'
```
## Reducers

!! WARNING: **Under Construction** !!

> This feature is **actively** being worked on. And as such this notice will disappear soon.  
> But API and semantics may change somewhat in future versions.  
> We're ... erm ... you know, ... considering our options.
>  
> Use with caution in production code. 

  
Reducers are a relatively new addition to the QuickCheckr arsenal.  
And you could, rightfully so, point out that they violate it's core philosophy
of not rewriting what actually happened.  

But once again, it is a feature born out of necessity.  
 
With that caveat out of the way, let's dig in.  
  
### Values Stop On First Passed

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(), Reduce.Values([3, 2, 1, 0]))
from expect in Checkr.Expect("Input < 2", () => input < 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_ValuesStopOnFirstPassed.cs:48:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1681332589
 ------------------------------------------------------------
  Executed:
   - Input: Input = 79
            Redux   2
 ====================================
  !! Expectation Failed: Input < 2
 ====================================
```
### Values Stop On First Passed Alternative

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(), Reduce.Values([3, 0, 1, 2]))
from expect in Checkr.Expect("Input < 2", () => input < 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_ValuesStopOnFirstPassedAlternative.cs:48:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1681332589
 ------------------------------------------------------------
  Executed:
   - Input: Input = 79
            Redux   3
 ====================================
  !! Expectation Failed: Input < 2
 ====================================
```
### Values Report Last Failure

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(), Reduce.Values([3, 0, 1, 2], true))
from expect in Checkr.Expect("Input < 2", () => input < 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                C_ValuesReportLastFailure.cs:48:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1681332589
 ------------------------------------------------------------
  Executed:
   - Input: Input = 79
            Redux   2
 ====================================
  !! Expectation Failed: Input < 2
 ====================================
```
### Collection
Helper function in order to avoid generic type confusion.  

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int().Many(1, 5), Reduce.Collection([[3], [2], [1], [0]]))
from expect in Checkr.Expect("All < 2", () => input.All(a => a < 2))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                D_Collection.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 4 shrinks)
 Seed:                    1681332589
 ------------------------------------------------------------
  Executed:
   - Input: Input = [ 94 ]
            Redux   [ 2 ]
 ==================================
  !! Expectation Failed: All < 2
 ==================================
```
### Collection Function
Helper function in order to avoid generic type confusion.  

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int().Many(1, 5), Reduce.CollectionFunction<int>(a => [[3], [2], [1], [0]]))
from expect in Checkr.Expect("All < 2", () => input.All(a => a < 2))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                D_CollectionFunction.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 4 shrinks)
 Seed:                    1681332589
 ------------------------------------------------------------
  Executed:
   - Input: Input = [ 94 ]
            Redux   [ 2 ]
 ==================================
  !! Expectation Failed: All < 2
 ==================================
```
### Function Stop On First Passed

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(), Reduce.Function<int>(a => [3, 2, 1, 0]))
from expect in Checkr.Expect("Input < 2", () => input < 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_FunctionStopOnFirstPassed.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1681332589
 ------------------------------------------------------------
  Executed:
   - Input: Input = 79
            Redux   2
 ====================================
  !! Expectation Failed: Input < 2
 ====================================
```
### Function Stop On First Passed Alternative

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(), Reduce.Function<int>(a => [3, 0, 1, 2]))
from expect in Checkr.Expect("Input < 2", () => input < 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_FunctionStopOnFirstPassedAlternative.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1681332589
 ------------------------------------------------------------
  Executed:
   - Input: Input = 79
            Redux   3
 ====================================
  !! Expectation Failed: Input < 2
 ====================================
```
### Function Report Last Failure

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(), Reduce.Function<int>(a => [3, 0, 1, 2], true))
from expect in Checkr.Expect("Input < 2", () => input < 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                C_FunctionReportLastFailure.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1681332589
 ------------------------------------------------------------
  Executed:
   - Input: Input = 79
            Redux   2
 ====================================
  !! Expectation Failed: Input < 2
 ====================================
```
### Reduce Towards Zero

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(), Reduce.TowardsZero)
from expect in Checkr.Expect("Input should be smaller than 42", () => input < 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                SimpleReducers.cs:48:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1681332589
 ------------------------------------------------------------
  Executed:
   - Input: Input = 79
            Redux   42
 ==========================================================
  !! Expectation Failed: Input should be smaller than 42
 ==========================================================
```
### Reduce Minimize String

**The Checkr:**  
```csharp
from text in Checkr.Input("Text", Fuzzr.String(), Reduce.MinimizeString)
from expect in Checkr.Expect("no a and b", () => !(text.Contains('a') && text.Contains('b')))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                MultiStepReducers.cs:48:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1834440365
 ------------------------------------------------------------
  Executed:
   - Input: Text  = "tdblaulb"
            Redux   "ab"
 =====================================
  !! Expectation Failed: no a and b
 =====================================
```
### Custom

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(), new CustomTowardsZeroReducer().Reduce)
from expect in Checkr.Expect("Input < 2", () => input < 2)
select Case.Closed;
```

**Custom Reducer:**  
```csharp
public class CustomTowardsZeroReducer
{
    private int lastFailed = 0;
    public Reduction Reduce(ReductionResult result)
    {
        var value = result.ValueAs<int>();
        if (result.Failed)
            lastFailed = value;
        else
            return Reduction.Stop(lastFailed);
        if (value == 0)
            return Reduction.Stop(lastFailed);
        if (value > 0)
            value--;
        else
            value++;
        return Reduction.Next(value);
    }
}
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_Custom.cs:50:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    211010750
 ------------------------------------------------------------
  Executed:
   - Input: Input = 10
            Redux   2
 ====================================
  !! Expectation Failed: Input < 2
 ====================================
```
## Issues
### The Unshrinkables
#### Unknown Primitives
```csharp
    from unkown in Checkr.Input("Half", Fuzzr.Half())
    from fail in Checkr.Expect("Fails", () => false)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Drafts\Z_Issues\A_TheUnshrinkables\B_UnknownPrimitives\UnknownPrimitives.cs:19:1
 Original failing run:    1 execution
 Seed:                    746257211
 ------------------------------------------------------------
  Executed:
   - Input: Half = 65,25
   - WARNING: Can't shrink type 'Half' at input 'Half'.
 ================================
  !! Expectation Failed: Fails
 ================================
```
### System Types
```csharp
public class Thing
{
    public int Irrelevant { get; set; }
    public Type Type { get; set; } = typeof(int);
    public Type OtherType { get; set; } = typeof(int);
}
```
```csharp
    from thing in Checkr.Input("A thing with a type", Fuzzr.Constant(new Thing()))
    from fail in Checkr.Expect("Fails", () => thing.Type.Name != typeof(int).Name)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Drafts\Z_Issues\K_SystemTypes\SystemTypes.cs:20:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1487819190
 ------------------------------------------------------------
  Executed:
   - Input: A thing with a type = { Type: Int32, OtherType: Int32 }
   - WARNING: Object of type 'Type' has no shrinkable properties at input 'A thing with a type.Type'.
   - WARNING: Object of type 'Type' has no shrinkable properties at input 'A thing with a type.OtherType'.
 ================================
  !! Expectation Failed: Fails
 ================================
```
##### Workaround
```csharp
    from thing in Checkr.Input("A thing with a type",
        Fuzzr.Constant(new Thing()),
        () => Shrink.Values([typeof(string), typeof(double)]))
    from fail in Checkr.Expect("Fails", () => thing.Type.Name != typeof(int).Name)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    RunTestSolution
 Location:                Drafts\Z_Issues\K_SystemTypes\SystemTypes.cs:58:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    350297528
 ------------------------------------------------------------
  Executed:
   - Input: A thing with a type = { Type: Int32 }
 ================================
  !! Expectation Failed: Fails
 ================================
```
