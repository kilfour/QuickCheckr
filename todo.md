## Current Version: 0.0.6 

### Ready for Changelog after review

### Doing/Ongoing
* Doc: working on the reference (Checkrs Court)  
* play more pool
  * Warehousing
    * Step 5 : dynamically populate WareHouse 
    * Step 6 : new invariant: Warehouse product amount always contains atleast Max(20, highest amount ever ordered) 

### Doc Current Focus 
* Polish the guide a bit more
  * better examples for 
    * ChoosingTheNextMove: this one works, per chance, in reality it needs a deliberation policy
    * InPoolsWeTrust: expectation is a bit contrived

### Tests Current Focus
* DuplicateKeyWriteGuardForPoolTraces

### Skipped Tests (4)
* 2 for Configuration InternalAffairs: Work in progress, not too important right now.
  Might get removed.
* GatheringEvidence: Touches the file system.
* TheBroadcasterTests: Explicit experimental.

## 0.0.7: In Pools We Trust

## 0.0.7+
* look into different collection shrinking
  * limitations
  * known issues
  * workarounds
* `Trackr.PoolWhen`, `Trackr.PoolExpect`, `Trackr.PoolExpectEach`, `Trackr.PoolExpectEachWhen`
    Still feel uneasy having these on Trackr, but let's see how it feels after a while.
* Dive into Reducers a bit more
  * Generalize Towards(Zero) reducer
  * More reducers
* Deliberation Shrinking. Add:
  * a stop when predicate like executioncount <= 4, user can then up the number of runs to 10000 relatively safely
  * decouple the casefile from the predicate
* keyed overload for PoolFor, GetPool, ... 
    * maybe not, looks tricky in impl and yucky in usage
    * alternative: example in cookbook using subclasses or composition
* Editor: configure QuickPulse.Show (see ideas)
* Stabilize + Doc InternalAffairs
  * not using these anymore: only need them when there's a shrinking bug, so maybe hide them completely

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

## Ideas
* Make OneOf(-When) return T instead of Case ? let's see what comes up during explorations
* QuickPulse.Show config could use another look 
* QuickPulse.Show turn manofwealth and taste into composable config
  => for reporting inputs : ShrinkingClue.Ignore property => ignore in QPS config => print full object
* Maybe retrieve line number of failed Expectation (perf ?)
* Bring back Feedback Shrinking, don't need a deep copy of memory with the new structure for one
  Should be a lot easier than before
  
## Refactoring Targets 
* DeskOfficer seems a bit wastefull with all those lists
  * fixed ordered version

### Already Considered
> Looked at these and not deemed needed (now). Just keeping this list in order to avoid me doing the same thing twice.
* Nested InputShrinkingClue ? probably not, had a better idea, but forgot, something to do with QP.S
  * The QP.S thing, might not be the right direction, but the InputShrinkingClues do deserve another look 
* Refactoring: simplify casefile compile
  * meh, looked at it. It's annoying occasionally, if a new transformation is needed, but fine for now.
* GetFromPool (bool, T?) => Result. Also not needed, a tuple will do in this case.
* unify ConditionalResult and DelayedResult ... maybe

## Other
* Scour QuickAcid for any interesting tests left behind.