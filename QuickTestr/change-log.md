### Unreleased
* Added `AssertThrewExactly<TException>(action, message: optionalFactory)` to one- and two-input property builders, including deliberated builders. Supports sync/async actions and cancellation-token callbacks, with exact exception-type and optional ordinal message matching. Message factories receive the current generated input (`Func<TInput, string>`) or both inputs (`Func<TInput1, TInput2, string>`), including during shrinking.
* Exception assertions use the existing shrinking, deliberation, and async runner paths. Property reports include expected/actual exception details.

### 0.1.0: Bringing It All Back Home
* Added seeded debug runs with `AutopsyProbe` support.
* Improved oracle diagnostics by recording inputs and results and displaying `null` results explicitly.
* Reduced the public extension surface by sealing fluent types, hiding their constructors, and internalizing implementation helpers.
* QuickTestr now preserves async property and oracle delegates and exposes matching `RunAsync(...)` APIs.

### 0.0.7: This Year's Model
* Added initial support for model-based testing.
* Added syntactic sugar for generating Tuple input (arity 2 only)
* Added `Task<TResult>` overloads for asynchronous oracle-based testing.
* Completed return-value verification across model-based operation variants.
* Added `Task<bool>` overloads for asynchronous property assertions.

### 0.0.6: Keeping Up With the Checkrers
* Updated to the latest QuickCheckr version.
* Added support for improved shrinking and reporting features from QuickCheckr.
* Added documentation examples for legacy code characterization testing.
* Added a public shrinking challenge suite inspired by jqwik, QuickCheck, Hypothesis and test.check.
* Reorganized and expanded the documentation.

### 0.0.5: Entering the Vault
* QuickCheckr update.
* Added Xml Summary comments to the public API.
* Renamed the old investigation-style persistence API to the new vault vocabulary, including `FillVault`, `InspectVault`, `CleanupVault`, and related types.

### 0.0.3: Fixing the Oracle
* Bugfix release

### 0.0.2: Investigating with Double Vision
* Added `Expected` and `Actual` for Redux values in Oracle-based tests.
* Introduced `Testr`-specific `GatheringEvidence`.

### 0.0.1: The Seedling
* Initial implementation of a QuickCheck-style DSL built on top of QuickCheckr.
* Added Testr versions of the jqwik challenges.
* Added an example showing how complex problems can be handled by *steering* the engine.
* Added an alternative way of defining a Testr tailored for oracle-based testing.
