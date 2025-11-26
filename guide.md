# Guide
## Contents

- [Checkr-ing Things Out][CheckringThingsOut]
  

[CheckringThingsOut]: #checkr-ing-things-out
## Checkr-ing Things Out
A *Checkr* describes a sequence of steps that QuickCheckr will execute repeatedly.
Each run goes through the script from top to bottom: generating inputs, performing actions, checking specs, and collecting diagnostics.
If any execution fails, QuickCheckr shrinks it to the smallest, easiest-to-understand failing case.  
### First Specification
Let's start by having a look at the simplest possible Checkr:  
```csharp
public class MinimalCheckr
{
    [Fact]
    public void RunTest()
        => Checkr.Spec("Always true", () => true).Run();
}
```
Congratulations! You've just checked something out. The test passes and now we know that `true` is always `true`.

So what happens if try to do the opposite?  
```csharp
Checkr.Spec("Always true", () => false);
```
The test fails and QuickCheckr produces the following report:  
```text
------------------------------------------------------------
 Test:                    SpecAlwaysFalse
 Location:                Docs\A_Guide\A_CheckringThingsOut\CheckringThingsOut.cs:42:1
 Original failing run:    1 execution
 Seed:                    1642083259
 ------------------------------------------------------------
  Executed (1)
 ===============================
  !! Spec Failed: Always true
 ===============================
```

The report shows:
* A run summary
  * Test: Name of the test executing the Checkr.
  * Location: Source code Location of the test executing the Checkr.
  * Original failing run: Number of executions executed during the run. Let me get back to you on that one.
  * Seed: Seed used to generate the failing run. You can use this to reproduce the failing run reliably.
* The minimal fail case:  The minimal failing input sequence. In this case original run and minimal run are the same.  
  And details, also in this case, consist only of one execution with it's ID in parentesis.
* Failure details: Why the run failed in the first place.
  
### Gathering Input
Let's add an input to the Checkr, and change our spec:  
```csharp
var check =
    from input in Checkr.Input("Input", Fuzzr.Int())
    from spec in Checkr.Spec("Input /= 42", () => input != 42)
    select Case.Closed;
```

*Note:* `Case.Closed` marks the end of the Checkr script.
A Checkr is just a LINQ expression that evaluates step by step,
and the final select clause must return a value representing the completed case.
`Case.Closed` simply signals "this case finished successfully, there are no more steps".

QuickCheckr relies on the [`QuickFuzzr`](https://github.com/kilfour/QuickFuzzr/blob/main/README.md) library to generate inputs.

`Fuzzr.Int()` generates a random integer between 1 and 100, so this test will fail, on avarage, once every hundred times.  
Not ideal, so let's up the run number, like so:  
```csharp
check.Run(200.Runs());
```
Now the test fails consistently:  
```text
------------------------------------------------------------
 Test:                    AddingAnInput
 Location:                Docs\A_Guide\A_CheckringThingsOut\CheckringThingsOut.cs:81:1
 Original failing run:    1 execution
 Seed:                    127648209
 ------------------------------------------------------------
  Executed (1):
   - Input: Input = 42
 ===============================
  !! Spec Failed: Input /= 42
 ===============================
```
As you can see the input is now included in the report *minimal fail case*,
this is because QuickCheckr judged the input to be relevant to the failure.  
So, if we add another input to the Checkr, one which does not influence the failure:  
```csharp
var check =
    from input in Checkr.Input("Input", Fuzzr.Int())
    from irrelevant in Checkr.Input("Irrelevant Input", Fuzzr.Int())
    from spec in Checkr.Spec("Input /= 42", () => input != 42)
    select Case.Closed;
```
The following happens:  
```text
------------------------------------------------------------
 Test:                    AddingAnotherInput
 Location:                Docs\A_Guide\A_CheckringThingsOut\CheckringThingsOut.cs:121:1
 Original failing run:    1 execution
 Seed:                    127648209
 ------------------------------------------------------------
  Executed (1):
   - Input: Input = 42
 ===============================
  !! Spec Failed: Input /= 42
 ===============================
```
That's basically the same thing as before, because QuickCheckr does not report irrelevant inputs.  
### Not Without a Trace
QuickCheckr's *shrinking* can sometimes seem magical. Not a problem in this simple test,
but when you are writing a more complex test, it is always helpful to see what's really going on.

QuickCheckr is built with transparancy and discoverability as a first class concern:  
```csharp
var check =
    from input in Checkr.Input("Input", Fuzzr.Int())
    from irrelevant in Checkr.Input("Irrelevant Input", Fuzzr.Int())
    from spec in Checkr.Spec("Input /= 42", () => input != 42)
    from trace in Checkr.Trace("Irrelevant Input", () => irrelevant)
    select Case.Closed;
```
By adding a `Checkr.Trace` to the `LINQ` chain, we get the following report:  
```text
------------------------------------------------------------
 Test:                    NotWithoutATrace
 Location:                Docs\A_Guide\A_CheckringThingsOut\CheckringThingsOut.cs:156:1
 Original failing run:    1 execution
 Seed:                    769641533
 ------------------------------------------------------------
  Executed (1):
   - Input: Input = 42
   - Trace: Irrelevant Input = 47
 ===============================
  !! Spec Failed: Input /= 42
 ===============================
```
### Time to Act!
Testing inputs is fun and all, but really, we want to test some kind of behaviour, right?  
```csharp
var check =
    from one in Checkr.Input("One", Fuzzr.Int())
    from two in Checkr.Input("Two", Fuzzr.Int())
    from three in Checkr.Input("Three", Fuzzr.Int())
    from sum in Checkr.Act("One plus Three", () => one + three)
    from spec in Checkr.Spec("Sum /= 42", () => sum != 42)
    select Case.Closed;
```
Still a trivial example, but it shows how you can use `Checkr.Act` to start mutating things.  
And here's the report:  
```text
------------------------------------------------------------
 Test:                    TimeToAct
 Location:                Docs\A_Guide\A_CheckringThingsOut\CheckringThingsOut.cs:193:1
 Original failing run:    1 execution
 Seed:                    1642083259
 ------------------------------------------------------------
  Executed (1): One plus Three
   - Input: One = 24
   - Input: Three = 18
 =============================
  !! Spec Failed: Sum /= 42
 =============================
```
As demonstrated, the name of the executed action is included in the report, as well as the relevant input.  
