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
 
With that caveat out of the way, let's dig in.  

There exist **two different types** of reducers:
 - [Simple Reducers][SimpleReducers]
 - [Multi Step Reducers][MultiStepReducers]
  
## Simple Reducers

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
## Multi Step Reducers

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
