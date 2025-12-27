## Current Version: 0.0.6 

### Ready for Changelog after review

### Doing
* Pool Redesign
  * `Trackr.PoolWhen`, `Trackr.PoolExpect`, `Trackr.PoolExpectEach`, `Trackr.PoolExpectEachWhen`
    Still feel uneasy having these on Trackr. Maybe move them to Checkr or Poolr.
* Warehousing
  * Input: A targeted error rewrite (not warning) for the common misleading case
    When an eager Input throws OneOfEmptyOptionsException during evaluation, rethrow
    Ask user if it is dependent on state
    !! needs more investigation before deciding !!
  * lazy input, maybe solve in quickfuzzr : public static FuzzrOf<T> From<T>(Func<FuzzrOf<T>> factory)
* QF FromEach 
* play more pool
* review evidence gathering

### Doc Current Focus 
* Review/Polish the Guide, looks solid enough for now
* Review Configuration entry
  * [ x ] FileAs : add CaseSummary info
  * [ x ] Custodian
  * [ x ] ShrinkMode
  * [ x ] Reportmode
  * [ x ] DeliberationPolicy
  * [   ] InternalAffairs
* Style Guide for LINQ-averse devs (experimental).

### Tests Current Focus
* DuplicateKeyWriteGuardForPoolTraces
* ParsingNumbers example: Uncomment Assert, decimal separator now breaks build.

### Skipped Tests (4)
* 2 for Configuration InternalAffairs: Work in progress, not too important right now.
* GatheringEvidence: Touches the file system.
* TheBroadcasterTests: Explicit experimental, takes quite long too.

## 0.0.7: In Pools We Trust
Stabilize Pools

## 0.0.7+
* look into different collection shrinking
  * limitations
  * known issues
  * workarounds
* keyed overload for PoolFor, GetPool, ...
* Dive into Reducers a bit more
  * Generalize Towards(Zero) reducer
  * More reducers
* Editor: configure QuickPulse.Show (see ideas)
* Deliberation Shrinking. Add:
  * a stop when predicate like executioncount <= 4, user can then up the number of runs to 10000 relatively safely
  * decouple the casefile from the predicate
* Stabilize InternalAffairs

## 0.1.0
* update xml summaries

## 0.1.0++
* async & concurrency (spike cooking in Experimental, see TheNeedlerPattern)

## Future Doc Work
* Build HorsesForCourses.Api for testing example
  * Initial Version Ready (auth is hardcoded admin for now)
* Cookbook Additions
  * statefull list maybe
  * Scheduling: Non-overlapping bookings that do overlap, needs explanation
* Start working on the reference

## Ideas
* `Showr.For<T>(Func<ManofWealthAndTaste, ManofWealthAndTaste>)`
* Make OneOf(-When) return T instead of Case ? let's see what comes up during explorations
* QuickPulse.Show config could use another look 
* QuickPulse.Show turn manofwealth and taste into composable config
  => for reporting inputs : ShrinkingClue.Ignore property => ignore in QPS config => print full object
* Maybe retrieve line number of failed Expectation (perf ?)
* Bring back Feedback Shrinking, don't need a deep copy of memory with the new structure for one
  Should be a lot easier than before
  
## Refactoring Targets 
* Nested InputShrinkingClue ? probably not, had a better idea, but forgot, something to do with QP.S
  * The QP.S thing, might not be the right direction, but the InputShrinkingClues do deserve another look 
* Loose the lazy IEnumerable in shrinking (yield)
* DeskOfficer seems a bit wastefull with all those lists
* Refactoring: simplify casefile compile
  * meh, looked at it. It's annoying occasionally, if a new transformation is needed, but fine for now.
* GetFromPool (bool, T?) => Result. Also not needed, a tuple will do in this case.
* unify ConditionalResult and DelayedResult ... maybe

## Other
* Scour QuickAcid for any interesting tests left behind.