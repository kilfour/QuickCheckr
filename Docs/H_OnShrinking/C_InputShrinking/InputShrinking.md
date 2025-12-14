# Input Shrinking
This is the next step in QuickCheckr shrinking and the most complex one.  
QuickCheckr shrinking tries to remove inputs, one by one, to find a minimal failing case.  
The following simple example has one input but always fails.  
```csharp
    from irrelevant in Checkr.Input("Irrelevant Input", Fuzzr.Int())
    from spec in Checkr.Expect("Always Fails", () => false)
    select Case.Closed;
```
Input shrinking reruns the failing execution and prunes inputs whose values don't affect the failure.

So, in the previous example, after *Input Shrinking*, the result is:
  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Docs\H_OnShrinking\C_InputShrinking\InputShrinking.cs:37:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    165641598
 ------------------------------------------------------------
  Executed
 =======================================
  !! Expectation Failed: Always Fails
 =======================================
```
Another example, slightly trickier.  
```csharp
    from one in Checkr.Input("Input One", Fuzzr.Int())
    from two in Checkr.Input("Input Two", Fuzzr.Int())
    from three in Checkr.Input("Input Three", Fuzzr.Int())
    from result in Checkr.Act("Result", () => one + three)
    from spec in Checkr.Expect("Should not be 42", () => result != 42)
    select Case.Closed;
```
As you can see from the test definition, two inputs are involved in a possible failure.

QuickCheckr correctly identifies the relevant inputs.

Result:
  
```text
------------------------------------------------------------
 Test:                    Example_Two
 Location:                Docs\H_OnShrinking\C_InputShrinking\InputShrinking.cs:78:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    231122398
 ------------------------------------------------------------
  Executed: Result
   - Input: Input One = 12
   - Input: Input Three = 30
 ===========================================
  !! Expectation Failed: Should not be 42
 ===========================================
```
