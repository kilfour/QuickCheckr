# Reverse List

This tests the (wrong) property that reversing a list of integers results in the same list. 
It is a basic example to validate that a library can reliably normalize simple sample data.
  

**The Testr:**  
```csharp
Testr
    .Named("Reversing a list of integers results in the same list")
    .For(Fuzzr.Int().Many(0, 10).ToList())
    .Assert(a =>
    {
        var reversed = new List<int>(a);
        reversed.Reverse();
        return reversed.SequenceEqual(a);
    });
```

**The Report:**  
```text
------------------------------------------------------------
 Test:                    Example
 Location:                ReverseList.cs:28:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 5 shrinks)
 Seed:                    12901993
 ------------------------------------------------------------
  Executed: Run
   - Input    = [ _, 94 ]
     Redux      [ _, 0 ]
   - Original = [ 76, 92, 75, 6, 94 ]
 ================================================================================
  !! Expectation Failed: Reversing a list of integers results in the same list
 ================================================================================
```
