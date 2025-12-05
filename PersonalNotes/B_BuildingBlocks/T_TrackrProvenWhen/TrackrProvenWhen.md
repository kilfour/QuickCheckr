# Trackr Proven When
```csharp
    from counter in Checkr.Act("hack for asserting execution count", () => ++executionCount)
    from spec in Trackr.ProvenWhen("Counter equals 3", () => counter == 3)
    from shouldNeverFail in Checkr.Spec("Should never fail, because of ProvenWhen", () => counter <= 3)
    select Case.Closed;
```
