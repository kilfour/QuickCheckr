# Checkr Act When
```csharp
    from counter in Checkr.Input("Counter", Fuzzr.Counter("key"))
    from never in Checkr.ActWhen("Never", () => false, () => { })
    from neverAgain in Checkr.ActWhen("Never Again", () => false, () => true)
    from thing in Checkr.ActWhen("Act", () => counter != 2, () => counter == 3 ? null : new Thing())
    from notNull in Checkr.Expect("Not Null", () => thing.Value != null)
    select Case.Closed;
```
```csharp
    from counter in Checkr.Input("Counter", Fuzzr.Counter("key"))
    from never in Checkr.ActWhen("Never", () => false, () => Task.CompletedTask)
    from neverAgain in Checkr.ActWhen("Never Again", () => false, () => Task.FromResult(true))
    from thing in Checkr.ActWhen("Act", () => counter != 2, () => Task.FromResult(counter == 3 ? null : new Thing()))
    from notNull in Checkr.Expect("Not Null", () => thing.Value != null)
    select Case.Closed;
```
