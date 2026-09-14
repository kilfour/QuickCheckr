
QuickTestr keeps the surface area small, while still giving you:

* The original failing value.
* A reduced version when reduction is enabled.
* A reproducible seed.
* A report format that is compact enough for day-to-day use.

## Why QuickTestr?

There are already strong property-based testing tools in .NET.
QuickTestr is not trying to replace all of them.

Its niche is:

* **C#-friendly fluency.**
* **Small API surface.**
* **Simple reports.**
* **Easy opt-in custom shrinking.**
* **Integration with the QuickCheckr worldview.**

In other words: less framework energy, more getting on with the test.

## Highlights

* **Tiny DSL:** `Named`, `For`, `Assert`, `Run`, and a small set of focused extras.
* **Built on QuickCheckr:** Benefits from the same shrinking engine and reporting philosophy.
* **Custom reducers:** Plug in domain-aware shrinking when built-ins are not enough.
* **Deterministic:** Failures come with seeds, so rerunning is straightforward.
* **Good for oracle tests:** Compare a buggy implementation against a trusted model with minimal ceremony.
* **Model-based testing:** Generate and verify operation sequences against a trusted model.

## Basic Usage

### Define a property