## Custom Shrinkers
```csharp
    from count in Checkr.Input("Counter", Fuzzr.Counter("count"))
    // One: Uses the default shrinker.
    from thingOne in Checkr.Input("Thing One", Fuzzr.Constant<Thing>(null!))
        // Two: Uses a Fuzzr for shrinking. 
        // The second argument is the number of values Fuzzr should produce.
        // The default number is 10.
    from shrinkThingTwo in Shrinkr.For(Fuzzr.One<Thing>(), 2)
    from thingTwo in Checkr.Input("Thing Two", Fuzzr.Constant<Thing>(null!))
        // Three: Puts back the default shrinker
    from shrinkThingThree in Shrinkr.For<Thing>(Shrink.Default<Thing>())
    from thingThree in Checkr.Input("Thing Three", Fuzzr.Constant<Thing>(null!))
        // Four: Uses hardcoded values for shrinking. 
    from shrinkThingFour in Shrinkr.For(new Thing(), new Thing())
    from thingFour in Checkr.Input("Thing Four", Fuzzr.Constant<Thing>(null!))
        // Five: Uses a factory function for shrinking. 
        // The lamda argument is the original Input value
    from shrinkThingFive in Shrinkr.For((Thing a) => [new Thing()])
    from thingFive in Checkr.Input("Thing Five", Fuzzr.Constant<Thing>(null!))
        // Six: Uses multiple shrinkers. 
        // The second one will remove this value from the report.
        // The first one will still emit a warning.
    from shrinkThingSix in Shrinkr.For(Shrink.Default<Thing>(), Shrink.Values(new Thing()))
    from thingSix in Checkr.Input("Thing Six", Fuzzr.Constant<Thing>(null!))
        // Seven: Multiples again. 
        // Now the first one will shortcircuit shrinking, so no warning from the second one.
    from shrinkThingSeven in Shrinkr.For(Shrink.Values(new Thing()), Shrink.Default<Thing>())
    from thingSeven in Checkr.Input("Thing Seven", Fuzzr.Constant<Thing>(null!))
    from expect in Checkr.Expect("count != 2", () => count != 2)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Drafts\G_CustomShrinkers\CustomShrinkers.cs:19:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 6 shrinks)
 Seed:                    693048172
 ------------------------------------------------------------
  Executed:
   - Input: Counter     = 2
   - Input: Thing One   = null
   - Input: Thing Three = null
   - WARNING: Null value for type 'Thing' at input 'Thing One'.
   - WARNING: Null value for type 'Thing' at input 'Thing Three'.
   - WARNING: Null value for type 'Thing' at input 'Thing Six'.
 =====================================
  !! Expectation Failed: count != 2
 =====================================
 Passed Expectations
 - count != 2: 1x
 ------------------------------------------------------------
```
#### Nested
```csharp
public class TheUniverse
{
    public class Life { public int Answer { get; set; } }
    public Life? Everything { get; set; }
    public Life? Irrelevant { get; set; }
    public int GetTheAnswer() => Everything!.Answer;
}
```
The following example, if ran without the Shrinkr, would not be able to shrink the Null values and would also emit warnings.  
```csharp
    from shrink in Shrinkr.For(TheLifeFuzzr)
    from universe in Checkr.Input("The Universe", TheUniverseFuzzr)
    from trace in Checkr.Trace("The Universe", () => universe)
    from theAnswer in Checkr.Act("The Answer", () => universe.GetTheAnswer())
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Nested
 Location:                Drafts\G_CustomShrinkers\CustomShrinkers.cs:105:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1269557792
 ------------------------------------------------------------
  Executed: The Answer
   - Input: The Universe = { Everything: null }
   - Trace: The Universe = { Everything: null, Irrelevant: null }
 ===========================================================================
  !! System.NullReferenceException: Object reference not set to an instance of an object.
   at QuickCheckr.Tests.Drafts.G_CustomShrinkers.CustomShrinkers.TheUniverse.GetTheAnswer() in Drafts\G_CustomShrinkers\CustomShrinkers.cs:line 92
   at QuickCheckr.Tests.Drafts.G_CustomShrinkers.CustomShrinkers.<>c__DisplayClass8_1.GetCheckrNested() in Drafts\G_CustomShrinkers\CustomShrinkers.cs:line 127
   at QuickCheckr.Checkr.<>c__DisplayClass10_1`1.TryCatch() in CheckrAct_Helper.cs:line 12
   at QuickCheckr.UnderTheHood.TheCatcher.InTheCheckr[T](CheckrState state, String key, Func`1 func, Action`1 onException) in UnderTheHood\TheCatcher.cs:line 15
 ===========================================================================
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
 Location:                Drafts\Z_Issues\K_SystemTypes\SystemTypes.cs:21:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    724765321
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
    from shrink in Shrinkr.For(typeof(string), typeof(double))
    from thing in Checkr.Input("A thing with a type", Fuzzr.Constant(new Thing()))
    from fail in Checkr.Expect("Fails", () => thing.Type.Name != typeof(int).Name)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    RunTestSolution
 Location:                Drafts\Z_Issues\K_SystemTypes\SystemTypes.cs:59:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    120783772
 ------------------------------------------------------------
  Executed:
   - Input: A thing with a type = { Type: Int32 }
 ================================
  !! Expectation Failed: Fails
 ================================
```
