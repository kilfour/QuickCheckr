# Collections
When shrinking collections, QuickCheckr does not attempt to rewrite or regenerate the list.
Instead, it removes items one by one,
replaying the run each time to test whether the removed element was relevant for the failure.

This means that if only some elements in a collection contribute to the failing condition,
QuickCheckr will aggressively trim the rest away.
The result is a minimal collection containing only the values that still trigger the failure,
even if the original list was large or noisy.
  
