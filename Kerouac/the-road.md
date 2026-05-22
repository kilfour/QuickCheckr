## 0.2.0: Watching the Detectives

## Hierarchy

```text
=> means produces
-> means contains

Inquiry => Dossier : IRecord
    -> many Investigations

Investigation => Findings : IRecord
    -> many Cases

Case => CaseFile : IRecord
    -> many Executions
    -> Testimony
    -> Evidence?
    -> Verdict
```
FilingCabinet
    IRecord
    CaseFile
    Findings
    Dossier

ClerksOffice
    RecordRenderer => Clerk
    RecordStyleGuide
    Decorum

public interface IClerk
{
    Flow<Flow> File(IRecord record, Decorum decorum);
}

public record Decorum
{
    public bool ShowLabels { get; init; }
    public bool ShowTraces { get; init; }
    public bool ShowFinalTrace { get; init; }
    public bool ShowWarnings { get; init; }
    public bool ShowStackTrace { get; init; }

    public int Indent { get; init; }
    public string Separator { get; init; }

    public WarningLevel WarningLevel { get; init; }
}


### Doing
* consolidate CheckrOfTConductInvestigations and CheckrOfTGatherEvidence
* Shrink Log
* ShrinkableAct consolidation
* Reused the name `GatheringEvidence` for the new casefile producing workflow.
```text


### Todo
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


### Wish List
* Doc/test/improve `Reduce.Towards()`.
* Add shrinking(reducing) multiple values together
* Add reuse fuzzr reducer with predicate (input, candidate) => score
* Ouroboros Testing : We need a better `InputDeposition`
* Depositions external project: Challenges in CsCheck, FsCheck and QuickCheckr
* Improve nested collection shrinking/regrowth  
  (current model struggles when classification collapses to empty).
* Stringr => use Result.Stop instead of State.RunStopped

### Done/Ready for Changelog after review
* Added defensive handling during scoped swaps to prevent failing property setters from aborting shrinking.
* Removed warnings for unchecked conditional expectations.  
  The implementation could not reliably detect all unchecked expectations in composed Checkrs, potentially creating false confidence.
* Added a consolidated report for GatherEvidence(...) and Conduct( n.Investigations(), ...)
* Removed the *evidence-log* vault file.

## 0.3.0 : All Aboard
* More in-built reducers
* Generic types in warnings show `IEnumerable``1` => meh
* WarningDepositionsTests EmptyCollections_Nested 
  "Empty collection deemed ..." should be "collection**s**"
* DuplicateKeyWriteException check wording (Input as example)
* Figure out which Reducer warnings are needed/useful.
* Stabilize + Doc InternalAffairs
  
* Do a sanity pass on the public API.
* Also, check the namespaces, put under the hood stuff in UnderTheHood and don't leak anything.
* more stats:
  * total runs(including shrinking candidates)
  * candidates tried
  * ...
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
  * Maybe add When Things Go Wrong.
* dotnet new checkr templates
  * minimal
  * for newbies
  * web api
* update xml summaries
* make some noise


## 0.4.0 : Wait for Me !
* async & concurrency (spike cooking in Experimental, see TheNeedlerPattern)
  => initial web-api example (QuickWebr ?)


## Skipped Tests
* 2 for Configuration InternalAffairs: Work in progress, not too important right now.
  Might get removed.
* 2 for GatheringEvidence: Touches the file system.
* TheBroadcasterTests: Explicit experimental.
* QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.D_CustomShrinkers.B_Reduction.C_ReducersOld.A_Values;
* QuickCheckr.Tests.Drafts.H_OnShrinking.F_DeliberationShrinking.DeliberationShrinking.RunTest_DeliberatedGreedy()
  Fixed the wrong outcome, this now documents a no longer existing issue


## Future Doc Work
* Checkrs Court: Tricky one
* Cookbook Additions
  * statefull list, ... maybe


## Ideas
* *Shrinker*
    1. Classifier: decides whether input is relevant
    2. Reducer: tries to find a minimal value
    3. NEW : Collapser: simplifies structure
* Expect returns Passed / Failed / NotExecuted => allows for (easier) conditional tracing
  Or maybe an object with an OnFail tracing method 
* Add 'Witness' (candidate that made the run pass) to Redux 
* QuickPulse.Show config could use another look 
* Maybe retrieve line number of failed Expectation (perf ?)
* Play more pool. Not really needed I reckon, but you know, making sure ...
  * Warehousing
    * Step 5 : dynamically populate WareHouse 
    * Step 6 : new invariant: Warehouse product amount always contains atleast Max(20, highest amount ever ordered) 
* keyed overload for PoolFor, GetPool, ... 
    * maybe not, looks tricky in impl and yucky in usage
    * alternative: example in cookbook using subclasses or composition <= !! THIS !!

  
## Refactoring Targets 
* Consolidate first run only combinators
  * Encapsulate : if (state.EvidenceRecorder.IsReplay) return CheckrResult.CaseOnly(state);
* Get rid of the dispatching thing, not worth it, annoying to debug, hides intent.
* !!! InputDeposition Value => string !!!
* PrimitiveShrinkClassifier : uses non-deterministic shrinking candidates
  instead of Guid.NewGuid(), DateTime.UtcNow, ...
* Add tests for:
  * DuplicateKeyWriteGuardForPoolTraces
  * Deliberation
    * ICaseFileSummary:
      * `int WarningsOf<T>(Func<T, int> calculateScore) where T : Warning;`;  
      * `int InputsNamed<T>(string key, Func<T, int> calculateScore)`;
    * DeliberationTarget
* Try to remove multiple execution shrinking passes.

* Compile the CaseFile

### Already Considered
> Looked at these and not deemed needed (now).  
> Just keeping this list in order to avoid me doing the same thing twice.  

* GetFromPool (bool, T?) => Result. Not needed, a tuple will do in this case.
* Unify ConditionalResult and DelayedResult


## Other