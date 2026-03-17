## Next version: 0.0.9 : Reduce, Reuse, Recycle
* *Nested* Reducers, first on type, later by path.
      
### Doing
* Classified Input & Redux custom report Formatting
* NestedReducerInObjectOnPath
* Shrink.OnType(Try.None<Thing>()) => Shrink.None<T>()  
* FIX .CAPTURE THROWS => EXCEPTION WEIRDNESS
* **Reducers**
    * ReductionNotPossible.cs => Warning
    * QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.IssueMaybe.cs
      not *really* an issue, a single value version of 'The Case of the Invisible List'
    * Shrinking Redesign again
      > Design settled and implemented, just need to fill some API holes.
      * Execution Shrinking
      * Input Shrinking
        * Structural Collapse
        * Input Classification
        * Input Reduction (Redux)  
* DSL Experiments
 
### Todo
* Conditional Shrinkers based on context : `Shrink.When(a => a.Parent<Term>(t => t.Op == Operator.Div), ...)`
* Add Shrinkers to Lazy `.Input`s.
* Add *Deposition Tests* for collapsed Empty collections warnings.
* Doc/test/improve `Reduce.Towards()`.
* Figure out which Reducer warnings are needed/useful.
* Check what happens when collection is null.
* Reducers, maybe report passing candidate
* Consolidate first run only combinators
  * Encapsulate : if (state.EvidenceRecorder.IsReplay) return CheckrResult.CaseOnly(state);
* **DOC**  
  DocsSinglePages/doc.md (line 2611) RunTest_WithDeliberationPolicy is effectively a duplicate of the first null-handling example at DocsSinglePages/doc.md (line 2546). Publicly, both reports show the same outcome: The Universe = { Everything: null }, one relevant warning, same failure. The surrounding text claims the policy removed irrelevant warning noise, but that is already what the first example visibly shows, so this test adds little narrative value.

  DocsSinglePages/doc.md (line 2068) Built In Enumerations and DocsSinglePages/doc.md (line 2097) Custom Enumerations are near-duplicates. They demonstrate the same shrinking behavior and differ mainly by enum type. For public docs, one example plus a sentence saying custom enums behave the same would read cleaner.

  The ReportMode Reference block at DocsSinglePages/doc.md (line 1178) is intentionally exhaustive, but from a public-doc perspective it is repetitive: Default, None, Trace, Warning, and Labels all reuse the same failing checkr and mostly restate formatting deltas. If doc.md is meant to be narrative documentation rather than a full reference dump, this is the densest cluster to trim or move elsewhere.

  The same applies to ShrinkMode Reference at DocsSinglePages/doc.md (line 1306). The tests are not exact duplicates, but they are repetitive variants of one scenario. For public-facing docs, a shorter comparison would likely carry the idea better than four rendered test reports.

  DocsSinglePages/investigations.md (line 60) and DocsSinglePages/investigations.md (line 92) reuse the same heading, Nested Collections Inner Count. The second is really a “with custom shrinker” follow-up, so the narrative purpose exists, but the duplicated heading makes it look like accidental duplication.

### Wish List
* Add unchecked expectations WARNING.
  This would be very good for the dsl bit (F.i. Example)
* Add 'Witness'(candidate that made the run pass) to Input & Redux 
* more stats:
  * total runs(including shrinking candidates)
  * candidates tried
  * ...
     
### Doc

### Ongoing
* Refactor the Tests:
  * one checkr per file (use CheckrTest instead of BaseTest, boyscout rule)
  * Next up:
    * A_TheShowrClass

### Done/Ready for Changelog after review
* Input Shrinking Redesign : accomodating for Reducers for one
  * A *Shrinker* now consists out of two parts 
    1. Classifier: decides whether input is relevant
    2. Reducer: tries to find a minimal value
  This change is mostly visible in the Customizable part of the Api, but has been implemented throughout the engine. 
* Renamed Custom Shrinker factory class from `Shrink` to `Try`.
* Renamed all ShrinkOption classes and interfaces to use ClassificationOption instead.
* Added some missing PoolWhen overloads.
* Added some missing Pool operation reporting in the .qr file.
* Added `Combine.Checkrs` which is very useful for *QuickCheckr as a DSL builder*.
* In case an expectation throws it is rethrown as `ExpectationThrowsException`.
* Major improvements to shrinking behaviour as a side effect of introducing reducers.
* Added intial version allowing for customized reporting of:
  * Classified Input
  * Redux

## 0.0.10 : Excuse My Domain Specific Language

## 0.0.11 : All Aboard
* Stabilize + Doc InternalAffairs
  * Shrink Log
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
  * Maybe add When Things Go Wrong.
* dotnet new checkr templates
  * minimal
  * for newbies
  * web api

## 0.1.0
* update xml summaries
* make some noise

## 0.1.1 : Wait for Me !
* async & concurrency (spike cooking in Experimental, see TheNeedlerPattern)
  => initial web-api example (QuickWebr ?)

## Skipped Tests
* 2 for Configuration InternalAffairs: Work in progress, not too important right now.
  Might get removed.
* 2 for GatheringEvidence: Touches the file system.
* TheBroadcasterTests: Explicit experimental.
* QuickCheckr.Tests.Notes.C_ReducersOld.A_Values;
* QuickCheckr.Tests.Docs.H_OnShrinking.F_DeliberationShrinking.DeliberationShrinking.RunTest_DeliberatedGreedy()
  Fixed the wrong outcome, this now documents a no longer existing issue

## Future Doc Work
* Checkrs Court: Tricky one
* Build HorsesForCourses.Api for testing example
  * Initial Version Ready (auth is hardcoded admin for now)
* Cookbook Additions
  * statefull list, ... maybe

## Ideas
* Expect returns Passed / Failed / NotExecuted => allows for (easier) conditional tracing
  Or maybe an object with an OnFail tracing method 
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