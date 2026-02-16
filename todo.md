## Next version: 0.0.9 : Reduce, Reuse, Recycle
* *Nested* Reducers, first on type, later by path.

### Doing
* **Reducers**
* ReductionNotPossible.cs => Warning

### Todo
* Add Reducers to Lazy Input
* Add *Deposition Tests* for collapsed Empty collections warnings.
* doc/test Reduce.Towards
* figure out which Reducer warnings are needed/useful

### Wish List

### Doc

### Ongoing
* Refactor the Tests:
  * one checkr per file (use CheckrTest instead of BaseTest, boyscout rule)
  * Next up:
    * A_TheShowrClass

### Done/Ready for Changelog after review

## 0.0.10 : All Aboard
* Do a sanity pass on the public API.
* Also, check the namespaces, put under the hood stuff in UnderTheHood and don't leak anything.
* Look into different types of collection shrinking:
  * Limitations.
  * Known issues
  * Workarounds. 
  * Try dictionaries first maybe, and see what pops up.
* Make sure Doc and Cookbook is finished and complete.
    * Polish the guide a bit more
      * InPoolsWeTrust: expectation is a bit contrived.
  * Running a Checkr
    * Maybe consolidate Test Framework Integration into this one
    * Needs a story
  * You Can Go Your Own Way is very much unfinished.
  * Consider splitting of Honey I Shrunk the Inputs.  
    This makes cross referencing impossible though.
  * Meet the Team: Clerk, Custodian, etc ...
    Glossary kind of thing.  
    Reiterate scopes.  
    Trackr vs Checkr.
  * Maybe add When Things Go Wrong.
* dotnet new checkr templates
  * minimal
  * for newbies
  * web api
* Shrink Log, i.e.: polish InternalAffairs

## 0.0.9+
* Play more pool. Not really needed I reckon, but you know, making sure ...
  * Warehousing
    * Step 5 : dynamically populate WareHouse 
    * Step 6 : new invariant: Warehouse product amount always contains atleast Max(20, highest amount ever ordered) 
* keyed overload for PoolFor, GetPool, ... 
    * maybe not, looks tricky in impl and yucky in usage
    * alternative: example in cookbook using subclasses or composition <= THIS
* Stabilize + Doc InternalAffairs
  * not using these anymore: only need them when there's a shrinking bug, so maybe hide them completely

## 0.1.0
* update xml summaries
* Checkr's Court

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
* Build HorsesForCourses.Api for testing example
  * Initial Version Ready (auth is hardcoded admin for now)
* Cookbook Additions
  * statefull list, ... maybe

## Ideas
* `Trackr.PoolWhen`, `Trackr.PoolExpect`, `Trackr.PoolExpectEach`, `Trackr.PoolExpectEachWhen`
    Still feel uneasy having these on Trackr, but let's see how it feels after a while.
* Make OneOf(-When) return T instead of Case ? let's see what comes up during explorations
* QuickPulse.Show config could use another look 
* QuickPulse.Show turn manofwealth and taste into composable config
  => for reporting inputs : ShrinkingClue.Ignore property => ignore in QPS config => print full object
* Maybe retrieve line number of failed Expectation (perf ?)
  
## Refactoring Targets 
* PrimitiveShrinkClassifier : uses non-deterministic shrinking candidates
  Guid.NewGuid(), DateTime.UtcNow, ...
* Add tests for:
  * DuplicateKeyWriteGuardForPoolTraces
  * Deliberation
    * ICaseFileSummary:
      * `int WarningsOf<T>(Func<T, int> calculateScore) where T : Warning;`;  
      * `int InputsNamed<T>(string key, Func<T, int> calculateScore)`;
    * DeliberationTarget
* Added Deliberator: 
  Maybe change it to an overseer (CaseOfficer) and move all the *Case Scoped* logic in there.  
  This could get rid of `Duplicate code in CheckrOfTGatherEvidence and Vault`, I think.
  Recap : 
    * Execution Scope: 1 Execution.
    * Run Scope: 1 Run, possibly many Executions.
    * Case Scope: Multiple Runs.  
      Can lead to :
        * Failing Run => CaseFile
        * All Runs Pass => Case Summary
    * Investigation Scope:
      * Multiple Cases investigated, only through GatherEvidence for now.
      * We could expand this.  
        For instance:  
        > Hey Deskofficer, give me three reports different reports with one execution, ASAP !   

        Probably not now though.
* Try to remove multiple execution shrinking passes.
* ShrinkingInfo.Strategy: Change from string to Type.
* Duplicate code in CheckrOfTGatherEvidence and Vault
* DeskOfficer seems a bit wastefull with all those lists
  * fixed ordered version
* Compile the CaseFile

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