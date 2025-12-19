## Current Version: 0.0.5

* It is written only once, keyed overloads for 
  * tracewhen (also add conditional execution)
  * Trackr.Expect

### Tests to Add
* Nested OneOf/OneOfIf
* CaseSummary DepositionTests
* What happens if a muppet puts an Input inside of a When
  Probably just works, better to make sure.

### Doc current focus
* Add an introduction to pools before Bolting Horses
* Doc Configuration
  * [ x ] FileAs : add CaseSummary info
  * [ x ] Custodian
  * [ x ] ShrinkMode
  * [ X ] DeliberationPolicy
  * [   ] Investigations

* Build HorsesForCourses.Api for testing example
  * Initial Version Ready (auth is hardcoded admin for now)

## 0.0.6
* look into different collection shrinking
  * limitations
  * known issues
  * workarounds
* update xml summaries

## 0.0.6+
* keyed overload for PoolFor, GetPool, ...
* Dive into Reducers a bit more
  * Generalize Towards(Zero) reducer
  * More reducers
* add a ReportMode to config maybe ? disabling traces and/or warnings
  * ReportMode { Traces | Warnings | None }
* Editor: configure QuickPulse.Show 
* Deliberation Shrinking. Add:
  * a stop when predicate like executioncount <= 4, user can then up the number of runs to 10000 relatively safely
  * decouple the casefile from the predicate
* Stabilize Investigations
* async & concurrency (spike cooking in Experimental, see TheNeedlerPattern)

## Future Doc Work
* Cookbook Additions
  * statefull list maybe
  * Scheduling: Non-overlapping bookings that do overlap, needs explanation
* Start working on the reference

## Ideas
* QuickPulse.Show turn manofwealth and taste into composable config
  => for reporting inputs : ShrinkingClue.Ignore property => ignore in QPS config => print full object
* Maybe retrieve line number of failed Expectation (perf ?)

## Refactoring Targets 
* GetFromPool (bool, T?) => Result
* Nested InputShrinkingClue ?
* Loose the lazy IEnumerable in shrinking (yield)
* unify ConditionalResult and DelayedResult

## Other
* Scour QuickAcid for any interesting tests left behind.
