# Acting Out Of Sync
```csharp
public class Eventually
{
    public static Task<int> AddOne(int input)
    {
        return Task.FromResult(input + 1);
    }
}
```
```csharp
    from input in Checkr.Input("Input", Fuzzr.Int())
    from result in Checkr.Act("Eventually.AddOne", Eventually.AddOne(input))
    from expect in Checkr.Expect("Is not 42", () => result != 42)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\Y_Async\ActingOutOfSync.cs:20:1
 Original failing run:    1 execution
 Seed:                    367926403
 ------------------------------------------------------------
  Executed: Eventually.AddOne
   - Input: Input = 41
 ====================================
  !! Expectation Failed: Is not 42
 ====================================
```
