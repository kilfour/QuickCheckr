## Current Version: 0.0.7: In Pools We Trust

### Current Focus
* Custom Shrinkers:
  Shrinkr.For<T>(params ShrinkerOptions)
  * ShrinkerOption 
    * Fuzzr
    * Func<T, IEnumerable<T>
    * Default
  * it would be amazing to get it to work like QuickFuzzr's Configr, but that looks tricky
* Check shrinking of 'System.Type' et al see QP.S.
    
### Ready for Changelog after review

### Doing/Ongoing
* Play more pool.
  * Warehousing
    * Step 5 : dynamically populate WareHouse 
    * Step 6 : new invariant: Warehouse product amount always contains atleast Max(20, highest amount ever ordered) 

### Doc Current Focus 
* hunt down every last spec
* Polish the guide a bit more
  * Better examples for :
    * ChoosingTheNextMove: this one works, per chance, in reality it needs a deliberation policy
    * InPoolsWeTrust: expectation is a bit contrived
* Work on the reference (Checkrs Court) 

### Tests Current Focus
* DuplicateKeyWriteGuardForPoolTraces

### Skipped Tests (4)
* 2 for Configuration InternalAffairs: Work in progress, not too important right now.
  Might get removed.
* GatheringEvidence: Touches the file system.
* TheBroadcasterTests: Explicit experimental.

## 0.0.8: You Can Go Your Own Way
> **Main focus:** Custom Shrinkers.  
* Showr
  * Just play around with it, doc in reference, ... 
  * For inputs ? Same as trace feature, but only apply to *relevant* Inputs
    Showing the original (cause we need to know what properties were important),
    but also potentially adding a more readable notation right below.
    See Warehouse example for ideas.  

And if *that* does not lead us down a rabbit hole:
* Reducers
  * Generalize Towards(Zero) reducer.
  * More of them.
* Look into different collection shrinking:
  * Limitations.
  * Known issues
  * Workarounds. 
  * Try dictionaries first maybe, and see what pops up. 

## 0.0.8+
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
* Any exception thrown by the Checkr will be caught and considered a failure.
* Build HorsesForCourses.Api for testing example
  * Initial Version Ready (auth is hardcoded admin for now)
* Cookbook Additions
  * statefull list maybe
  * Scheduling: Non-overlapping bookings that do overlap, needs explanation

## Ideas
* `Trackr.PoolWhen`, `Trackr.PoolExpect`, `Trackr.PoolExpectEach`, `Trackr.PoolExpectEachWhen`
    Still feel uneasy having these on Trackr, but let's see how it feels after a while.
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
* QuickAcid did multiple passes on Execution Shrinking
  I never liked it. The Account example however shows it could be usefull.
  On the other hand, DeliberationPolicies seems like a cleaner solution.
* QuickAcid Feedback shrinking: which amounts to combining similar executions.
  F.i. Withdraw(5), Withdraw(10) becomes Withdraw(15).
  Again DeliberationPolicy and possibly, in the future, Reducers will solve this better I think.
* Scour QuickAcid for any interesting tests left behind.