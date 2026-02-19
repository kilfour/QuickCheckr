# Seeded

**The Invariant:**  
```csharp
Check.ForAll(1948674479, Fuzzr.Int(0, 10), a => a != 6);
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                B_Seeded.cs:26:1
 Original failing run:    1 execution
 Seed:                    1948674479
 ------------------------------------------------------------
  Executed:
   - Input: Input = 6
 ===================================
  !! Expectation Failed: Property
 ===================================
```
