## Current Version: 0.0.5  
* QuickCheckr internal keys (for memory storage and counter/unique fuzzrs)
  Centralize or better yet, if possible use records
* review evidence gathering
* play more pool

### Doc Current Focus 
* Add an introduction to pools to the guide, before Bolting Horses 
* Review Configuration entry
  * [ x ] FileAs : add CaseSummary info
  * [ x ] Custodian
  * [ x ] ShrinkMode
  * [ / ] Reportmode
  * [ x ] DeliberationPolicy
  * [   ] InternalAffairs

### Tests Current Focus
none

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
  
## Refactoring Targets 
* GetFromPool (bool, T?) => Result
* Nested InputShrinkingClue ? probably not, had a better idea, but forgot, something to do with QP.S
* Loose the lazy IEnumerable in shrinking (yield)
* unify ConditionalResult and DelayedResult maybe
* DeskOfficer seems a bit wastefull with all those lists

## Other
* Scour QuickAcid for any interesting tests left behind.
