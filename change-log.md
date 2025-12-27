### 0.0.6
* Trackr.Analyze
* Taskr experiment
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

### 0.0.7
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