# Custom Shrinkers
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
 Seed:                    653821537
 ------------------------------------------------------------
  Executed:
   - Input: Counter = 2
   - Input: Thing One = null
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
### Nested
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
