# Checkr When
```csharp
    from counter in Checkr.Input("Counter", Fuzzr.Counter("key"))
    from thing in Checkr.When(() => counter != 2, Checkr.Act("Act", () => counter == 3 ? null : new Thing()))
    from notNull in Checkr.Expect("Not Null", () => thing.Value != null)
    select Case.Closed;
```
