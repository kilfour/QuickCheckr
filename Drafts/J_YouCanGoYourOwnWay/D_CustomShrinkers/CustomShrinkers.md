# Custom Shrinkers
As explained in [OnShrinking][InputShrinking], a Shrinker is comprised of:
  1. A *[Classifier][Classifiers]* : responsible for finding the relevant inputs.
  2. A *[Reducer][Reducers]* : responsible for finding a minimal value.

QuickCheckr allows you to define either one or both of these on a per-input basis.  
This gives you precise control over how shrinking behaves when the default strategies are insufficient for your domain.  

By default, built-in classifiers and reducers are provided for common types.  
Custom shrinkers are intended for cases where domain knowledge can guide shrinking more effectively. 

They are registered on a per-input basis using an overload on the `Checkr.Input)()` method.  
Contruction of a shrinker is done using factory methods on the `Shrink` static class.

Let's have a look at ...  
## A Simple Example

**The Checkr:**  
```csharp
from answers in Checkr.Input("Answer", Fuzzr.Int(),
    Shrink.OnType<int>(
        s => s.Classify(Try.Values(42)),
        s => s.Simplify(Reduce.TowardsZero)))
from expect in Checkr.Expect("Answer equals 42", () => answers == 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                A_FirstShrinker.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1895340861
 ------------------------------------------------------------
  Executed:
   - Input: Answer = 53
            Redux    43
 ===========================================
  !! Expectation Failed: Answer equals 42
 ===========================================
```
### Explaining the Example
The example above is intentionally simple. The expectation only passes for the value `42`,
which makes it easy to see how the custom shrinker influences the reported case.

QuickCheckr already provides built-in classifiers and reducers for common types such as `int`.
These default strategies work well in most situations, but they are necessarily generic: 
they do not know which values are meaningful in the context of a specific check.

By adding:

```csharp
s => s.Classify(Try.Values(42))
```

we tell QuickCheckr that `42` is a value worth trying explicitly during shrinking.

The reducer then continues simplifying the remaining failing value:

```csharp
s => s.Simplify(Reduce.TowardsZero)
```

In the report, the original failing value is `53`, while the final reported value becomes `43`.
This is useful because `43` is the closest failing value above the only passing value (`42`).

Although the expectation itself is artificial, the example illustrates the purpose of custom shrinkers:
they allow shrinking to be guided by values that are semantically meaningful for the domain, not just structurally smaller.
  
## A Second Example

**The Checkr:**  
```csharp
from answers in Checkr.Input("answers", Fuzzr.Int().Many(1, 5),
    Shrink.OnType<int>(
        s => s.Classify(Try.Values(42)),
        s => s.Simplify(Reduce.Towards(0))))
from trace in Checkr.Trace("original", () => answers)
from expect in Checkr.Expect("all answers equal 42", () => answers.All(a => a == 42))
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_SecondShrinker.cs:32:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    960262083
 ------------------------------------------------------------
  Executed:
   - Input: answers  = [ 73 ]
            Redux      [ 43 ]
   - Trace: original = [ 73 ]
 ===============================================
  !! Expectation Failed: all answers equal 42
 ===============================================
```

[InputShrinking]: ../../H_OnShrinking/C_InputShrinking/InputShrinking.md

[Classifiers]: A_Classification/Classifiers.md

[Reducers]: B_Reduction/Reducers.md
