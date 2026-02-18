# You Can Go Your Own Way

!! WARNING: **Unfinished Documentation** !!

> This feature is finished, the tests are there.  
>  
> But ..., we're working on writing down the explanation.  
> Depending on how far we are in this process, you could just see some scaffolding files here.  
> Or the raw output from the Unit Tests.  
> Or an unfinished, unpolished version of the docs.
>
> Come back soon.
  
We already saw in the chapter on [Configuration][Configuration] how you can turn some knobs to customize QuickCheckr's behaviour
and/or reporting globally.  
But there are ways to obtain more fine-grained control. That is what this chapter is about.  
## Contents

- [Showr][A_TheShowrClass]
- [Custom Shrinkers][CustomShrinkers]
- [Reducers][Reducers]
  

[Configuration]: #configuration

[A_TheShowrClass]: #the-showr-class

[CustomShrinkers]: #custom-shrinkers

[Reducers]: #reducers
## The Showr Class
This static factory class produces a CheckrOf<Case> which allows one to configure the pretty-printer used in the report.

It is implemented as such, instead of as an option on the configuration object,
because this allows for two useful things:  
> You can change configuration anywhere in the `LINQ chain`, multiple times.  

**The Checkr:**  
```csharp
from anInt in Checkr.Input("AnInt", Fuzzr.Int())
from showrOne in Showr.ForTrace(() => Please.AllowMe().ToReplace<int>(a => $"PREFIXED {a}"))
from traceOne in Checkr.Trace("One", () => anInt)
from showrTwo in Showr.ForTrace(() => Please.AllowMe().ToReplace<int>(a => $"OTHER {a}"))
from traceTwo in Checkr.Trace("Two", () => anInt)
from fail in Checkr.Expect("not 7", () => anInt != 7)
select Case.Closed;
```

**Report Excerpt:**  
```text
Executed:
   - Input: AnInt = 7
   - Trace: One   = PREFIXED 7
   - Trace: Two   = OTHER 7
```
> Configuration can depend on context.  

**The Model:**  
```csharp
public class Product
{
    public Guid Id { get; init; } = Guid.Empty;
    public string Name { get; init; } = string.Empty;
    public int AmountInStock { get; set; }
}

public record OrderRequest(Guid ProductId, int Amount);

public class Warehouse(List<Product> productsForSale)
{
    public List<Product> ProductsForSale => productsForSale;

    public void PlaceOrder(OrderRequest request) =>
        productsForSale.First(a => a.Id == request.ProductId).AmountInStock -= request.Amount;
}
```

**The Checkr:**  
```csharp
from warehouse in Trackr.Stashed(() => new Warehouse(ProductsForSale()))
from showr in Showr.ForInput(() =>
    Please.AllowMe().ToReplace<OrderRequest>(a =>
        $"Ordered {a.Amount} pieces of {warehouse.ProductsForSale.Single(b => a.ProductId == b.Id).Name}."))
from request in Checkr.Input("Order request", RequestFuzzr(warehouse))
from placeOrder in Checkr.Act("Place order", () => warehouse.PlaceOrder(request))
from noNegativeStock in Checkr.Expect("No negative stock",
    () => warehouse.ProductsForSale.Select(a => a.AmountInStock).All(a => a >= 0))
select Case.Closed;
```

**Report Excerpt:**  
```text
Executed: Place order
   - Input: Order request = { ProductId: b1c88a3c-444e-4776-a5c4-c660a75e9365, Amount: 53 }
                            Ordered 53 pieces of Product One.
```
A full reference to the available options can be found [here][A_ShowrReference].  

[A_ShowrReference]: #showr-reference
### Showr Reference
#### For Trace

**The Checkr:**  
```csharp
from showr in Showr.ForTrace(() => Please.AllowMe().ToReplace<int>(a => $"PREFIXED {a}"))
from anInt in Checkr.Input("AnInt", Fuzzr.Int())
from trace in Checkr.Trace("AnInt", () => anInt)
from fail in Checkr.Expect("not 7", () => anInt != 7)
select Case.Closed;
```

**Report Excerpt:**  
```text
Executed:
   - Input: AnInt = 7
   - Trace: AnInt = PREFIXED 7
```
#### For Input

**The Checkr:**  
```csharp
from showr in Showr.ForInput(() => Please.AllowMe().ToReplace<int>(a => $"PREFIXED {a}"))
from anInt in Checkr.Input("AnInt", Fuzzr.Int())
from trace in Checkr.Trace("AnInt", () => anInt)
from fail in Checkr.Expect("not 7", () => anInt != 7)
select Case.Closed;
```

**Report Excerpt:**  
```text
Executed:
   - Input: AnInt = 7
                    PREFIXED 7
   - Trace: AnInt = 7
```
#### For Input Without Configuration

**The Checkr:**  
```csharp
from showr in Showr.ForInput()
from anInt in Checkr.Input("AnInt", Fuzzr.Int())
from trace in Checkr.Trace("AnInt", () => anInt)
from fail in Checkr.Expect("not 7", () => anInt != 7)
select Case.Closed;
```

**Report Excerpt:**  
```text
Executed:
   - Input: AnInt = 7
                    7
   - Trace: AnInt = 7
```
#### For All

**The Checkr:**  
```csharp
from showr in Showr.ForAll(() => Please.AllowMe().ToReplace<int>(a => $"PREFIXED {a}"))
from anInt in Checkr.Input("AnInt", Fuzzr.Int())
from trace in Checkr.Trace("AnInt", () => anInt)
from fail in Checkr.Expect("not 7", () => anInt != 7)
select Case.Closed;
```

**Report Excerpt:**  
```text
Executed:
   - Input: AnInt = 7
                    PREFIXED 7
   - Trace: AnInt = PREFIXED 7
```
#### None

**The Checkr:**  
```csharp
from showr in Showr.ForAll(() => Please.AllowMe().ToReplace<int>(a => $"PREFIXED {a}"))
from showrNone in Showr.None()
from anInt in Checkr.Input("AnInt", Fuzzr.Int())
from trace in Checkr.Trace("AnInt", () => anInt)
from fail in Checkr.Expect("not 7", () => anInt != 7)
select Case.Closed;
```

**Report Excerpt:**  
```text
Executed:
   - Input: AnInt = 7
   - Trace: AnInt = 7
```
### None

**The Checkr:**  
```csharp
from showr in Showr.ForAll(() => Please.AllowMe().ToReplace<int>(a => $"PREFIXED {a}"))
from showrNone in Showr.None()
from anInt in Checkr.Input("AnInt", Fuzzr.Int())
from trace in Checkr.Trace("AnInt", () => anInt)
from fail in Checkr.Expect("not 7", () => anInt != 7)
select Case.Closed;
```

**Report Excerpt:**  
```text
Executed:
   - Input: AnInt = 7
   - Trace: AnInt = 7
```
## Custom Shrinkers

!! WARNING: **Under Construction** !!

> This feature is **actively** being worked on. And as such this notice will disappear soon.  
> But API and semantics may change somewhat in future versions.  
> We're ... erm ... you know, ... considering our options.
>  
> Use with caution in production code. 

  
### Registered But Unused
Because custom shrinkers can be defined for *nested* types, mistakes are possible.
Hence this warning.  

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(), () => Try.Values(["unused"]))
from expect in Checkr.Expect("Input < 2", () => input < 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                RegisteredButUnused.cs:59:1
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
### Shrink Default

**The Checkr:**  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from thing in Checkr.Input("Null Thing",
    Fuzzr.Constant<Thing>(null!),
    () => Try.Default<Thing>())
from expect in Checkr.Expect("counter != 2", () => counter != 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShrinkDefault.cs:68:1
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
### Shrink Function

**The Checkr:**  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from thing in Checkr.Input("Null Thing",
    Fuzzr.Constant<Thing>(null!),
    // The lamda argument is the original Input value
    () => Try.Function((Thing a) => [new Thing()]))
from expect in Checkr.Expect("counter != 2", () => counter != 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShrinkFunction.cs:68:1
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
### Shrink Fuzzr

**The Checkr:**  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
    //from thing in Checkr.Input("Null Thing", Fuzzr.Constant<Thing>(null!), () => Shrink.Fuzzr(Fuzzr.One<Thing>(), 2))
from thing in Checkr.Input("Null Thing", Fuzzr.Constant<Thing>(null!), Shrink.OnType<Thing>(() => Try.Fuzzr(Fuzzr.One<Thing>(), 2)))
from expect in Checkr.Expect("counter != 2", () => counter != 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShrinkFuzzr.cs:68:1
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
### Shrink Keep

**The Checkr:**  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from thing in Checkr.Input("Null Thing",
    Fuzzr.Constant<Thing>(null!),
    () => Try.Keep<Thing>())
from expect in Checkr.Expect("counter != 2", () => counter != 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShrinkKeep.cs:73:1
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
### Shrink Multiple

**The Checkr:**  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from thing in Checkr.Input("Null Thing",
    Fuzzr.Constant<Thing>(null!),
    () => [Try.Values([new Thing()]), Try.Function((Thing a) => [new Thing()])])
from expect in Checkr.Expect("counter != 2", () => counter != 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShrinkMultiple.cs:68:1
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
### Shrink None

**The Checkr:**  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from thing in Checkr.Input("Null Thing",
    Fuzzr.Constant<Thing>(null!),
    () => Try.None<Thing>())
from expect in Checkr.Expect("counter != 2", () => counter != 2)
select Case.Closed;
```
Weird option this one at first glance, maybe. It does not make inputs turn up in the report.  
We know it works because of the shrink count.
No shrinking => no shrinking clue => not in report.
This is correct behavior.
  

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShrinkNone.cs:68:1
 Original failing run:    2 executions
 Minimal failing case:    1 execution (after 1 shrink)
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
### Shrink Values

**The Checkr:**  
```csharp
from counter in Checkr.Input("counter", Fuzzr.Counter("counter"))
from thing in Checkr.Input("Null Thing",
    Fuzzr.Constant<Thing>(null!),
    () => Try.Values([new Thing()]))
from expect in Checkr.Expect("counter != 2", () => counter != 2)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ShrinkValues.cs:68:1
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
 Location:                A_ValuesStopOnFirstPassed.cs:59:1
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
 Location:                B_ValuesStopOnFirstPassedAlternative.cs:59:1
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
 Location:                C_ValuesReportLastFailure.cs:59:1
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
 Location:                D_Collection.cs:59:1
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
### Collection Of
Helper function in order to avoid generic type confusion.  

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int().Many(1, 5), Reduce.CollectionOf<int>(a => [[3], [2], [1], [0]]))
from expect in Checkr.Expect("All < 2", () => input.All(a => a < 2))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                D_CollectionOf.cs:59:1
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
 Location:                A_FunctionStopOnFirstPassed.cs:59:1
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
 Location:                B_FunctionStopOnFirstPassedAlternative.cs:59:1
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
 Location:                C_FunctionReportLastFailure.cs:59:1
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
//from input in Checkr.Input("Input", Fuzzr.Int(), Reduce.TowardsZero)
from input in Checkr.Input("Input", Fuzzr.Int(), Shrink.OnType(Reduce.TowardsZero))
from expect in Checkr.Expect("Input should be smaller than 42", () => input < 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_ReduceTowardsZero.cs:59:1
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
 Location:                B_ReduceMinimizeString.cs:59:1
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
### Nested In Object

**The Model:**  
```csharp
public class TheUniverse
{
    public int Answer { get; set; }
}
```

**The Checkr:**  
```csharp
from answer in Checkr.Input("Answer", Fuzzr.Int(), Reduce.TowardsZero)
from universe in Checkr.Capture(() => new TheUniverse { Answer = answer })
from notTooBig in Checkr.Expect("answer <= 42", () => universe.Answer <= 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                NestedInObject.cs:64:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1930190827
 ------------------------------------------------------------
  Executed:
   - Input: Answer = 96
            Redux    43
 =======================================
  !! Expectation Failed: answer <= 42
 =======================================
```
  
  
### Nested Reducer In Object

**The Model:**  
```csharp
public class TheUniverse
{
    public int Answer { get; set; }
}
```

**The Checkr:**  
```csharp
from universe in Checkr.Input("Universe", Fuzzr.One<TheUniverse>(), Reduce.TowardsZero)
from notTooBig in Checkr.Expect("answer <= 42", () => universe.Answer <= 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                NestedReducerInObject.cs:64:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    686899245
 ------------------------------------------------------------
  Executed:
   - Input: Universe = { Answer: 61 }
            Redux      { Answer: 43 }
 =======================================
  !! Expectation Failed: answer <= 42
 =======================================
```
  
  
### Nested Reducer In Object On Path

**The Model:**  
```csharp
public class TheUniverse
{
    public int AnswerOne { get; set; }
    public int AnswerTwo { get; set; }
    public int AnswerThree { get; set; }
}
```

**The Checkr:**  
```csharp
from universe in Checkr.Input("Universe", Fuzzr.One<TheUniverse>(),
    [("Universe.AnswerOne", Reduce.TowardsZero), ("Universe.AnswerTwo", Reduce.Towards(100))])
from trace in Checkr.Trace("Universe", () => universe)
from justRight in Checkr.Expect("just right", () => universe.AnswerOne <= 42 || universe.AnswerTwo >= 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                NestedReducerInObjectOnPath.cs:64:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 3 shrinks)
 Seed:                    1118829238
 ------------------------------------------------------------
  Executed:
   - Input: Universe = { AnswerOne: 91, AnswerTwo: 15 }
            Redux      { AnswerOne: 43, AnswerTwo: 41 }
   - Trace: Universe = { AnswerOne: 91, AnswerTwo: 15, AnswerThree: 52 }
 =====================================
  !! Expectation Failed: just right
 =====================================
```
### Custom

!! WARNING: **Experimental Feature** !!

> This feature is **experimental**.  
> Its API, behaviour, or semantics may change substantially in future versions.  
>  
> Use with caution in production code.  
> It is included for users who want to experiment and help shape the future of QuickCheckr.

  

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(), CustomTowardsZero.Reducer())
from expect in Checkr.Expect("Input < 2", () => input < 2)
select Case.Closed;
```

**Reducer:**  
```csharp
public class CustomTowardsZero
{
    /// <summary>
    /// Returns a reducer which tries to reduce the input to zero.
    /// </summary>
    /// <remarks>
    /// The reducer will try to reduce the value by one if it is greater than zero,
    /// and increase the value by one if it is less than zero.
    /// </remarks>
    public static Func<Reducer<int>> Reducer() =>
        () => new CustomTowardsZero().Reduce;
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
 Location:                A_Custom.cs:59:1
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
### Reduction Not Possible
What to do if all reduction values fail ?  

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(), Reduce.TowardsZero)
from expect in Checkr.Expect("Input should be greater than 42", () => input > 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ReductionNotPossible.cs:64:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1373577700
 ------------------------------------------------------------
  Executed:
   - Input: Input = 21
            Redux   0
 ==========================================================
  !! Expectation Failed: Input should be greater than 42
 ==========================================================
```
