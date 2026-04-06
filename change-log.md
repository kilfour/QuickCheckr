### 0.0.6: Zen Confirmed
* Trackr.Analyze
* Finished/Fixed ShrinkMode reporting
* Summary for passing tests
* OneOf behaviour fix
* Pool encapsulation
* Conditional Execution Guard in ActCarefully: Guards against Act not executed during shrinking
* Consistent use of When instead of If
* Pool evolutions (operations for one .Update, .Remove)
* Some more Exceptions
* Conditional Execution Guards everywhere 
* Investigations renamed to InternalAffairs
* Evidence gathering
* Less greedy Execution Shrinking, but now more correct.
* ReportMode

### 0.0.7: In Pools We Trust
* Encapsulated the pool element returned from Trackr.OneOfPool and others
* Added unsupported type for shrinking WARNING
* Array shrinking supported
* Added initial configuration of Checkr.Trace output through QuickPulse.Show's ManOfWealthAndTaste
* Split Trackr.OneOfPool into Trackr.OneOfPool and Trackr.OneOfPoolWhen
* Renamed Stashed<T> retrieval to GetStashed<T>
* Changed Checkr.OneOfWhen argument 
  from: params (Func<bool> condition, CheckrOf<Case> checkr)[] scripts
  to:   params CheckrOf<(Func<bool> condition, CheckrOf<Case> checkr)>[] options
* Added Checkr.Option helper function that goes along the new Checkr.OneOfWhen argument.
* Pool Redesign
  * Moved the `When`'s from PoolElement<T> to `Trackr.PoolWhen`
  * Moved the `Expect`'s from PoolElement<T> to `Trackr.PoolExpect`
  * Moved the `ExpectForEach`'s from PoolElement<T> to `Trackr.PoolExpectEach`
  * Moved the `ExpectForEachWhen`'s from PoolElement<T> to `Trackr.PoolExpectEachWhen`
  * Unified `Pool.These` into `Trackr.PoolWhen`
  * Added *PoolTraces* to report.
  * Only expose Pool IEnumerable for predicates as IReadOnlyCollection
  * Changed PoolWhen argument order for consistency, key first.
* Added Stacktrace (yes/no) ReportMode
* Added *lazy* Checkr.Input
* When an *eager* Input throws, exception is wrapped in a InputFuzzrThrewException.
  With an explanation and a suggestion to try the lazy version.
* `Trackr.PoolWhen<T1,T2>` now also exposes PoolElements
* Minor change to Exception Failure message formatting in report

### 0.0.8: You Can Go Your Own Way
* Added a WARNING when encountering an object with no shrinkable properties.
* Added ReportMode.Labeled (default enabled)
* Showr.ForInputs
* Report: Execution section: Better label alignment.
* Added a WARNING when an execution has at least one input, but all are deemed irrelevant.
* FIX: PoolExpectEachWhen. If predicate is false, it no longer qualifies as passed expectation.
* FIX: Reports without InputShrinking now (also) use QuickPulse.Show to report inputs.
* CHANGE: Now the first failing expectation in an execution is the one that matters, not the last.
* Added Input overloads taking Custom Shrinker(s) as extra argument.
* Added a WARNING when a registered Custom Shrinker is unused (avoiding type mismatches).
* Added Input overloads taking a *Reducer* as extra argument.
* DeliberationPolicy
  * Changed sig from `Func<CaseFile, CaseFile, CaseFile>` to `Func<ICaseFileSummary, int>`
  * Added DeliberationTarget, Deliberation stops when reached.
  * ICaseFileSummary added:
    * `int InputsNamed<T>(string key, Func<T, int> calculateScore)`;
    * `int WarningsOf<T>(Func<T, int> calculateScore) where T : Warning;`;  
* Custodians now takes a `CaseFile` and `VaultEntry` objects instead of already stringified ones. 
* Better formatting for *Empty collection deemed irrelevant* WARNING.
* new Custom Shrinker: `Shrink.Keep<T>()`

### 0.0.9 : Reduce, Reuse, Recycle
* Input Shrinking Redesign
  * A *Shrinker* now consists of two explicit parts:
    1. Classifier: decides whether input is relevant
    2. Reducer: tries to find a minimal value
  This introduces a clearer mental model for shrinking and enables
  better explainability, extensibility, and domain-aware control.
  Result: significantly improved shrinking and explanation quality.
  * Added `Shrink.When` enabling domain-aware shrinking.
* Renamed Custom Shrinker factory class from `Shrink` to `Try`
  to better reflect classification semantics.
* Renamed all ShrinkOption classes and interfaces to use ClassificationOption instead.
* Added initial support for customized reporting of:
  * Classified Input
  * Redux
  (making shrinking behaviour more transparent)
* Added `Combine.Checkrs` which is very useful for *QuickCheckr as a DSL builder*.
* In case an expectation throws, it is rethrown as `ExpectationThrowsException`
  to improve diagnostics.
* Added some missing PoolWhen overloads.
* Added more complete Pool operation reporting in the `.qr` file.
* Added warning when a reducer does not encounter a value that makes the run pass.

### 0.0.10 : Excuse My Domain Specific Language
* Added shrinking support for Tuples.
* Added shrinking support for Fields.
* Added a warning when conditional expectations are never exercised during a run.
* Introduced pluggable report rendering via `Flow<CaseFile>`.
* Added simplified report flow for Testr.
* Removed Internal Affairs, it has served it's purpose.
* Improved shrinking top level Collections.  
  **Known limitation:** nested collection shrinking/regrowth still has edge cases when classification collapses to empty and the domain really needs regrowth.
* Added `MaxStepsPerReducer` to configuration.  
  If a `Reducer` exceeds this value it stops and logs a warning.

### 0.0.11 : SELECT Might Still Be Broken
* Added support for domain aware string reduction.  
  * Experimental and prone to change, but to useful not to put out there. 