# Checkr-ing Things Out
|◀|[△][Guide]|[▶][A_TheStashRunsAndExecutions]|
|---|---|---|
  
A *Checkr* describes a sequence of steps that QuickCheckr will execute repeatedly.
Each run goes through the script from top to bottom: generating inputs, performing actions, checking expectations, and collecting diagnostics.
If any execution fails, QuickCheckr shrinks it to the smallest, easiest-to-understand failing case.  
## First Expectation
Let's start by having a look at the simplest possible Checkr:  
```csharp
[Fact]
public void RunTest()
    => Checkr.Expect("Always true", () => true).Run();
```
Congratulations! You've just checked something out. The test passes and now we know that `true` is always `true`.
  
So what happens if try to do the opposite?  

**The Checkr:**  
```csharp
Checkr.Expect("Always true", () => false);
```
The test fails and QuickCheckr produces the following:  

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_MinimalFailingCheckr.cs:43:1
 Original failing run:    1 execution
 Seed:                    1642083259
 ------------------------------------------------------------
  Executed
 ======================================
  !! Expectation Failed: Always true
 ======================================
```

The report shows a run summary containing :
* Test: Name of the test executing the Checkr.
* Location: Source code Location of the test executing the Checkr.
* Original failing run: Number of executions executed during the run.
* The minimal fail-case: In this simplest of examples it *only* specifies that an execution happened.
* Failure details: Why the run failed in the first place.
* Seed: The seed used to generate the failing run.  
  You can use this to reproduce the failing run reliably, by calling run with the seed passed in.  
```csharp
checkr.Run(1642083259);
```
## Gathering Input
Let's add an input to the Checkr, and change our expectation.  

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int())
from expect in Checkr.Expect("Input /= 42", () => input != 42)
select Case.Closed;
```

*Note:* `Case.Closed` signals the end of the Checkr script.
A Checkr is evaluated step by step, and `Case.Closed` is the final marker to indicate the scenario is complete.

QuickCheckr relies on the [`QuickFuzzr`](https://github.com/kilfour/QuickFuzzr/blob/main/README.md) library to generate inputs.

`Fuzzr.Int()` generates a random integer between 1 and 100, so this test will fail, *on average*, once every hundred times.  
Not ideal, so let's up the run number, like so:  
```csharp
checkr.Run(200.Runs());
```
With more runs, the test is more likely to find a failing input.  

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                C_GatheringInput.cs:32:1
 Original failing run:    1 execution
 Seed:                    127648209
 ------------------------------------------------------------
  Executed:
   - Input = 42
 ======================================
  !! Expectation Failed: Input /= 42
 ======================================
```
As you can see the input is now included in the report *minimal fail-case*,
this is because QuickCheckr judged the input to be relevant to the failure.  
Now, if we add another input to the Checkr, one which does not influence the failure.  

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int())
from irrelevant in Checkr.Input("Irrelevant Input", Fuzzr.Int())
from expect in Checkr.Expect("Input /= 42", () => input != 42)
select Case.Closed;
```
The following happens.  

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                D_AddingAnotherInput.cs:23:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    127648209
 ------------------------------------------------------------
  Executed:
   - Input = 42
 ======================================
  !! Expectation Failed: Input /= 42
 ======================================
```
That's basically the same thing as before, because **QuickCheckr does not report irrelevant inputs**.
> This behaviour is key. 
> QuickCheckr actively removes noise from the failing case.  
> => Less cognitive load. More signal.  
## Time to Act!
Testing inputs is fun and all, but really, we want to test some kind of behaviour, right?  

**The Checkr:**  
```csharp
from one in Checkr.Input("One", Fuzzr.Int())
from two in Checkr.Input("Two", Fuzzr.Int())
from three in Checkr.Input("Three", Fuzzr.Int())
from sum in Checkr.Act("One plus Three", () => one + three)
from expect in Checkr.Expect("Sum /= 42", () => sum != 42)
select Case.Closed;
```
Still a trivial example, but it shows how you can use `Checkr.Act` to start mutating things.  

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                E_TimeToAct.cs:20:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 1 shrink)
 Seed:                    1642083259
 ------------------------------------------------------------
  Executed: One plus Three
   - One   = 24
   - Three = 18
 ====================================
  !! Expectation Failed: Sum /= 42
 ====================================
```
As demonstrated, the name of the executed action is included in the report, as well as the relevant inputs.  

[Guide]: ../Guide.md

[A_TheStashRunsAndExecutions]: ../B_TheStashRunsAndExecutionsChapter/A_TheStashRunsAndExecutions.md
