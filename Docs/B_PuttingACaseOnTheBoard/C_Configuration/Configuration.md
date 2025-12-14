# Configuration
QuickCheckr runs are configured through an immutable `CheckrConfig` record. 
You typically start from `CheckrConfig.Default()` and then tweak the parts you care about.

All configuration options live in the `QuickCheckr.Protocol` namespace.  
**Default Configuration:**  
```csharp
{
    ShrinkMode = ShrinkMode.Default,
    DeliberationPolicy = null,
    FileAs = string.Empty,
    Custodian = Custodian.Default,
    Investigations = [],
}
```
