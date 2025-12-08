# On Shrinking
## Philosophy

QuickCheckr keeps shrinking simple and predictable.  
It never tries to *outsmart* your domain with clever global algorithms.  
Shrinking is purely local and irrelevance-based: if removing a value (or execution) still makes the property fail,
QuickCheckr marks it as *irrelevant*. 
This avoids surprising or *magical* shrink results.  
In rare circumstances (for instance, symmetric cases where everything collapses),
you can guide the process by configuring a *stand-in* value (e.g. Trackr.Substitute<int>(-1)),
which helps QuickCheckr distinguish roles without adding complexity.
  
