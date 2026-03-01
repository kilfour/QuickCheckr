# Classifiers
Classification is done by trying different *candidates* for an input and verifying if the test still fails.  

- If removing/changing the input (via candidates) still reproduces the failure, the input is irrelevant.
- If the failure goes away for at least one candidate, the input is relevant.

So when supplying a custom classifier, you are basically supplying a list of candidates.  
There are various ways to do this, but here is a first example:
  

**The Checkr:**  
```csharp
from answer in Checkr.Input("answer", Fuzzr.Int(), Try.Values(0, 1, 42))
from expect in Checkr.Expect("answer == 42", () => answer == 42)
select Case.Closed;
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Classifiers.cs:37:1
 Original failing run:    1 execution
 Seed:                    1521363987
 ------------------------------------------------------------
  Executed:
   - Input: answer = 63
 =======================================
  !! Expectation Failed: answer == 42
 =======================================
```
It's a bit of a contrived example as the default classifiers handle this case just fine, but it's here for demonstration purposes.  
