## Current Version: 0.0.5  
* review (refine ?) evidence gathering
* add a ReportMode to config maybe ? disabling traces and/or warnings
  * ReportMode { Traces | Warnings | None }

### Doc Current Focus
* Add an introduction to pools to the guide, before Bolting Horses 
* Review Configuration entry
* Move Deliberation Shrinking elaborate examples to on shrinking

### Tests Current Focus

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
* Editor: configure QuickPulse.Show 
* Deliberation Shrinking. Add:
  * a stop when predicate like executioncount <= 4, user can then up the number of runs to 10000 relatively safely
  * decouple the casefile from the predicate
* Stabilize Investigations
* async & concurrency (spike cooking in Experimental, see TheNeedlerPattern)

## Future Doc Work
* Doc Configuration
  * [ x ] FileAs : add CaseSummary info
  * [ x ] Custodian
  * [ x ] ShrinkMode
  * [ X ] DeliberationPolicy
  * [   ] InternalAffairs
* Build HorsesForCourses.Api for testing example
  * Initial Version Ready (auth is hardcoded admin for now)
* Cookbook Additions
  * statefull list maybe
  * Scheduling: Non-overlapping bookings that do overlap, needs explanation
* Start working on the reference

## Ideas
* QuickPulse.Show turn manofwealth and taste into composable config
  => for reporting inputs : ShrinkingClue.Ignore property => ignore in QPS config => print full object
* Maybe retrieve line number of failed Expectation (perf ?)
* Test QuickCheckr with QuickCheckr
  Targets:
  * DeskOfficer
  
## Refactoring Targets 
* GetFromPool (bool, T?) => Result
* Nested InputShrinkingClue ?
* Loose the lazy IEnumerable in shrinking (yield)
* unify ConditionalResult and DelayedResult maybe
* DeskOfficer seems a bit wastefull with all those lists
* QuickCheckr internal keys (for memory storage and counter/unique fuzzrs) ii
  Centralize or better yet, if possible use records

## Other
* Scour QuickAcid for any interesting tests left behind.
