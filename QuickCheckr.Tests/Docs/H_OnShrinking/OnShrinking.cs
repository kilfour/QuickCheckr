using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.H_OnShrinking;

[DocFile]
[DocHeader("Philosophy")]
[DocContent(
@"
QuickCheckr keeps shrinking simple and predictable.  
It never tries to *outsmart* your domain with clever global algorithms.  
Shrinking is purely local and irrelevance-based: if removing a value (or execution) still makes the property fail,
QuickCheckr marks it as *irrelevant*.  
This avoids surprising or *magical* shrink results.")]
public class OnShrinking { }