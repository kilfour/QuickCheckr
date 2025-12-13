# Configuration
QuickCheckr runs are configured through an immutable `CheckrConfig` record. 
You typically start from `CheckrConfig.Default()` and then tweak the parts you care about  
**Default Configuration:**  
```csharp
{
    ShrinkMode = ShrinkMode.Default,
    DeliberationPolicy = null,
    FileAs = string.Empty,
    Custodian = Custodian.Default,
    Investigations = [],
    Editor =
        new ManOfWealthAndTaste()
            .ToUseNonLinearTime()
            .ToInline<List<int>>(),
    WithTraces = true
}
```
