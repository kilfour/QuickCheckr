# Removes Irrelevant Properties
```csharp
public class BaggyInters
{
    public int IntOne { get; set; }
    public int IntTwo { get; set; }
    public int IntThree { get; set; }
}
```
```csharp
    from obj in Checkr.Input("Object", Fuzzr.One<BaggyInters>())
    from spec in Checkr.Spec("IntOne /= 42", () => obj.IntOne != 42)
    select Case.Closed;
```
```text
------------------------------------------------------------
 Test:                    Example
 Location:                Notes\A_Shrinking\C_InputShrinking\C_Objects\TheExamples\A_RemovesIrrelevantProperties.cs:20:1
 Original failing run:    1 execution
 Minimal failing case:    1 execution (after 2 shrinks)
 Seed:                    488581569
 ------------------------------------------------------------
  Executed:
   - Input: Object = { IntOne: 42 }
 ================================
  !! Spec Failed: IntOne /= 42
 ================================
```
