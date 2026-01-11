## Next version: 0.0.8: You Can Go Your Own Way

### Doing
* Que ?
  ```csharp
  public void StoreReducer<T>(string key, Func<Reducer<T>> reducer)
      => reducers.Store(key, () => () => // aaaaaaargh
      {
          Reducer<T> f = reducer();
          return a => f(a);
      });
  ```
  
### Todo
* Add Deliberator: => easy testing of InBuilt Policies in Configuration doc

### Wish List
* Warning Customization ?
* Hey Deskofficer, give me three reports different reports with one execution, ASAP ! 
  ... maybe, see if it's easy, if not push to later version

### Done/Ready for Changelog after review
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
    * public int InputsNamed<T>(string key, Func<T, int> calculateScore);

### Doc
* Reducers
  * Demonstrate Object Reducers 
    * Just show the pattern, no need for new implementation
      Split objects into different Inputs

* Weak Spots
  * Polish the guide a bit more
    * Better examples for :
      * ChoosingTheNextMove: this one works, per chance, in reality it needs a deliberation policy
      * InPoolsWeTrust: expectation is a bit contrived
  * Running a Checkr
    * Maybe consilidate Test Framework Integration into this one
    * Needs a story

### Ongoing
* Refactor the Tests:
  * one checkr per file (use CheckrTest instead of BaseTest, boyscout rule)

## 0.0.8+
* Try to remember the cool idea you had for Custom Custodians ;-)
  * emit case files instead of reports
  * generalize the court style guide
* Look into different types of collection shrinking:
  * Limitations.
  * Known issues
  * Workarounds. 
  * Try dictionaries first maybe, and see what pops up. 
* Play more pool. Not really needed I reckon, but you know, making sure ...
  * Warehousing
    * Step 5 : dynamically populate WareHouse 
    * Step 6 : new invariant: Warehouse product amount always contains atleast Max(20, highest amount ever ordered) 
* dotnet new checkr templates
  * minimal
  * for newbies
* keyed overload for PoolFor, GetPool, ... 
    * maybe not, looks tricky in impl and yucky in usage
    * alternative: example in cookbook using subclasses or composition <= THIS
* Stabilize + Doc InternalAffairs
  * not using these anymore: only need them when there's a shrinking bug, so maybe hide them completely

## 0.1.0
* update xml summaries

## 0.1.0++
* async & concurrency (spike cooking in Experimental, see TheNeedlerPattern)

## Skipped Tests (5)
* 2 for Configuration InternalAffairs: Work in progress, not too important right now.
  Might get removed.
* 2 for GatheringEvidence: Touches the file system.
* TheBroadcasterTests: Explicit experimental.

## Future Doc Work
* Checkrs Court: Tricky one
* Prepare demo talk(s)
* Any exception thrown during execution of the Act Checkr will be caught and considered a failure
* Build HorsesForCourses.Api for testing example
  * Initial Version Ready (auth is hardcoded admin for now)
* Cookbook Additions
  * statefull list, ... maybe
  * ParsingNumbers, right after scheduling, need better DeliberationPolicies first.

## Ideas
* `Trackr.PoolWhen`, `Trackr.PoolExpect`, `Trackr.PoolExpectEach`, `Trackr.PoolExpectEachWhen`
    Still feel uneasy having these on Trackr, but let's see how it feels after a while.
* Make OneOf(-When) return T instead of Case ? let's see what comes up during explorations
* QuickPulse.Show config could use another look 
* QuickPulse.Show turn manofwealth and taste into composable config
  => for reporting inputs : ShrinkingClue.Ignore property => ignore in QPS config => print full object
* Maybe retrieve line number of failed Expectation (perf ?)
  
## Refactoring Targets 
* Add tests for DuplicateKeyWriteGuardForPoolTraces
* Try to remove multiple execution shrinking passes.
* ShrinkingInfo.Strategy: Change from string to Type.
* Duplicate code in ObjectLens and ObjectShrinkStrategy.SetPropertyValue
* DeskOfficer seems a bit wastefull with all those lists
  * fixed ordered version

### Already Considered
> Looked at these and not deemed needed (now).  
> Just keeping this list in order to avoid me doing the same thing twice.  

* Nested InputShrinkingClue ? probably not, had a better idea, but forgot, something to do with QP.S
  * The QP.S thing, might not be the right direction, but the InputShrinkingClues do deserve another look. 
* GetFromPool (bool, T?) => Result. Also not needed, a tuple will do in this case.
* unify ConditionalResult and DelayedResult ... maybe

## Other
* QuickAcid Feedback shrinking: which amounts to combining similar executions.
  F.i. Withdraw(5), Withdraw(10) becomes Withdraw(15).
  DeliberationPolicy and Reducers solve this better I think.
* Scour QuickAcid for any interesting tests left behind.