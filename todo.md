## Current Version: 0.0.6 
* Refactoring: simplify casefile compile
* review evidence gathering
* play more pool
  * use a pool operation, but make the checkr used by Trackr.OneOfPool throw, see what happens 
    (pool mutated, SUT not, probably)
    ```csharp
    var outcome = checkr(value!)(state).Value;
    if (operations is not null)
        operations(new TrackrPoolOperations<T>(state, id));
    return CheckrResult.Some(state, outcome);
    ```
    better, but semantics
    ```csharp
    Func<TrackrPoolOperations<T>, CheckrOf<Case>>? operations
    ```
    also semantics I think, LINQ machinery is unaware of exception handling
    So yeah, I need a `[Fact]`
    Might not be an issue at all, as I think it does not need to be *atomic*
    The pool is a verification of the SUT, so if the sut fails, were gonna use the pool to verify
    Making it atomic => no way to verify

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

## 0.0.7
* look into different collection shrinking
  * limitations
  * known issues
  * workarounds
* update xml summaries

## 0.0.7+
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
* Bring back Feedback Shrinking, don't need a deep copy of memory with the new structure for one
  Should be a lot easier than before
  
## Refactoring Targets 
* GetFromPool (bool, T?) => Result
* Nested InputShrinkingClue ? probably not, had a better idea, but forgot, something to do with QP.S
* Loose the lazy IEnumerable in shrinking (yield)
* unify ConditionalResult and DelayedResult maybe
* DeskOfficer seems a bit wastefull with all those lists

## Other
* Scour QuickAcid for any interesting tests left behind.