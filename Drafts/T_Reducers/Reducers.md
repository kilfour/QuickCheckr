# Reducers

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

With this feature we're venturing closer too the QuickCheck style,
and I actually tried to use other existing libs first, but that did not work out all that well.  
Granted, I'm biased, I wrote this thing after all,
so ofcourse it's going to be easier for me to incorporate it into my own lib
than to struggle learning someone else's.  

With that caveat out of the way, let's dig in.  

There exist **two different types** of reducers:
  
## Simple Reducers

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(), Reducr.StopOnFirstPassed<int>(TowardsZero))
from expect in Checkr.Expect("Input should be smaller than 42", () => input < 42)
select Case.Closed;
```

**The Reducer:**  
```csharp
private static IEnumerable<int> TowardsZero(int input)
{
    var local = input;
    while (local != 0)
    {
        if (local > 0)
            yield return --local;
        else
            yield return ++local;
    }
}
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
## Multi Step Reducers

**The Checkr:**  
```csharp
from text in Checkr.Input("Text", Fuzzr.String(), new MinimizeString().Reduce)
from expect in Checkr.Expect("no a and b", () => !(text.Contains('a') && text.Contains('b')))
select Case.Closed;
```

**The Reducer:**  
```csharp
public class MinimizeString
{
    private int index = 0;
    public Reduction Reduce(ReductionResult result)
    {
        var str = result.ValueAs<string>();
        if (result.Passed)
            index++;
        if (index >= str.Length)
            return Reduction.Stop(str);
        return Reduction.Next(str.Remove(index, 1));
    }
}
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
