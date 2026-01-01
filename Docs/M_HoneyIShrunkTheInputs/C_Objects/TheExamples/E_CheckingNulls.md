# Checking Nulls
**The Model:**  
```csharp
public class TheUniverse
{
    public class Life { public int Answer { get; set; } }
    public Life? Everything { get; set; }
    public Life? Irrelevant { get; set; }
    public int GetTheAnswer() => Everything!.Answer;
}
```
**The Checkr:**  
```csharp
    from universe in Checkr.Input("The Universe", TheUniverseFuzzr)
    from trace in Checkr.Trace("The Universe", () => universe)
    from theAnswer in Checkr.Act("The Answer", () => universe.GetTheAnswer())
    select Case.Closed;
```
**The Report:**  
```text
------------------------------------------------------------
 Test:                    RunTest
 Location:                Docs\H_OnShrinking\C_InputShrinking\C_Objects\TheExamples\E_CheckingNulls.cs:36:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    358877731
 ------------------------------------------------------------
  Executed: The Answer
   - Input: The Universe = { Everything: null }
   - Trace: The Universe = { Everything: null, Irrelevant: { Answer: 78 } }
   - WARNING: Null value for type 'Life' at input 'The Universe.Everything'.
 ===========================================================================
  !! NullReferenceException: Object reference not set to an instance of an object.
 ===========================================================================
```
Note the shrinking of the `Irrelevant` property.  
In addition, QuickCheckr reports a shrinking warning:  
> Null value for type 'Life' at input 'The Universe.Everything'.  
This tells you that the shrinker reached a `null` value that it cannot safely reduce any further.  
  
But when both `Everything` *and* `Irrelevant` are `null`, QuickCheckr cannot shrink either property, and both limitations are reported as warnings:  
```text
------------------------------------------------------------
 Test:                    RunTest_NotShrinkingNull
 Location:                Docs\H_OnShrinking\C_InputShrinking\C_Objects\TheExamples\E_CheckingNulls.cs:85:1
 Original failing run:    1 execution
 Seed:                    1269557792
 ------------------------------------------------------------
  Executed: The Answer
   - Input: The Universe = { Everything: null, Irrelevant: null }
   - Trace: The Universe = { Everything: null, Irrelevant: null }
   - WARNING: Null value for type 'Life' at input 'The Universe.Everything'.
   - WARNING: Null value for type 'Life' at input 'The Universe.Irrelevant'.
 ===========================================================================
  !! NullReferenceException: Object reference not set to an instance of an object.
 ===========================================================================
```
We could help the shrinker by rewriting your checkr, in order to avoid the irrelevant input being reported as relevant.  
```csharp
    from shrink in Shrinkr.For(TheLifeFuzzr)
    from everything in Checkr.Input("Everything", TheLifeFuzzr)
    from irrelevant in Checkr.Input("Irrelevant", TheLifeFuzzr)
    from universe in Checkr.Capture(() => new TheUniverse { Everything = everything, Irrelevant = irrelevant })
    from trace in Checkr.Trace("The Universe", () => universe)
    from theAnswer in Checkr.Act("The Answer", () => universe.GetTheAnswer())
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    RunTest_Alternative
 Location:                Docs\H_OnShrinking\C_InputShrinking\C_Objects\TheExamples\E_CheckingNulls.cs:117:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1269557792
 ------------------------------------------------------------
  Executed: The Answer
   - Input: Everything   = null
   - Trace: The Universe = { Everything: null, Irrelevant: null }
 ===========================================================================
  !! NullReferenceException: Object reference not set to an instance of an object.
 ===========================================================================
```
Or, we can solve the issue using a [`DeliberationPolicy`][DeliberationShrinking] that limits the number of warnings:  
```csharp
private static CheckrConfig Configure(CheckrConfig config)
    => config with { DeliberationPolicy = DeliberationPolicy.MinimumWarnings };
```
```text
------------------------------------------------------------
 Test:                    RunTest_WithDeliberationPolicy
 Location:                Docs\H_OnShrinking\C_InputShrinking\C_Objects\TheExamples\E_CheckingNulls.cs:166:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    1856738502
 ------------------------------------------------------------
  Executed: The Answer
   - Input: The Universe = { Everything: null }
   - Trace: The Universe = { Everything: null, Irrelevant: { Answer: 13 } }
   - WARNING: Null value for type 'Life' at input 'The Universe.Everything'.
 ===========================================================================
  !! NullReferenceException: Object reference not set to an instance of an object.
 ===========================================================================
```
As you can see the warning is still there for `Universe.Everything` but this is a relevant input,
so the warning can be ignored.  
Deliberation did succeed in removing the `Universe.Irrelevant` input from the report.  

[DeliberationShrinking]: ../../../H_OnShrinking/F_DeliberationShrinking/DeliberationShrinking.md
