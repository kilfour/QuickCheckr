## Current Version: 0.0.6 
* review evidence gathering
* play more pool
  * Another idea: keep a number of 'clients' in a pool calling a web api (or service in this case for ease of coding)
    RESTy, they get products => use it to place order f.i., client object keeps track of what happened
    Could be used to verify concurrency issues et al, client asks data, places order, another client snatched last item ...
  * Started 'Warehouse' example
    * Look at cfg-able input formatting
    * looks like preload is needed, eager eval of OrderRequestFuzzr issue => document
      => Added a *Lazy* Checkr.Input, taking a fuzzr factory function as arg
    * We need a quickfuzzr map function if I am not mistaken

### Doc Current Focus 
* Review/Polish the Guide, looks solid enough for now
* Review Configuration entry
  * [ x ] FileAs : add CaseSummary info
  * [ x ] Custodian
  * [ x ] ShrinkMode
  * [ x ] Reportmode
  * [ x ] DeliberationPolicy
  * [   ] InternalAffairs

### Tests Current Focus
none

## 0.0.7
* look into different collection shrinking
  * limitations
  * known issues
  * workarounds

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