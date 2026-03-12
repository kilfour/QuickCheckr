# Registered But Unused
Because custom shrinkers can be defined for *nested* types, mistakes are possible.
Hence this warning.  

**The Checkr:**  
```csharp
from input in Checkr.Input("Input", Fuzzr.Int(), Try.Values(["unused"]))
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
