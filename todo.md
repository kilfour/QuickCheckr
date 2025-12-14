## Doc current focus
* [ x ] did we catch em all ? pretty sure we did, not 100%   
  Renamed:
  * Spec to Expect
  * SpecIf to ExpectWhen
  * Assay to Verify 
  * Analize to Expect
  * ProvenWhen to StopWhenProven 
* Doc Configuration
  * [   ] FileAs : add CaseSummary info
  * [ x ] Custodian
  * [ x ] ShrinkMode
  * [   ] DeliberationPolicy
  * [   ] Investigations
  * [   ] Editor: maybe remove this one for now and revisit QuickPulse.Show first
* Build HorsesForCourses.Api for testing example
  * Initial Version Ready (auth is hardcoded admin for now)

## Current Version: 0.0.4
* Decide on Editor exposure
* Fix Exception to use right combinator in message when keys are reused
* Trackr.Expect (formerly Analyze) vs Trackr.Verify (formerly Assay)
  * First off, definitely not Verify, maybe ExpectEventually ?
  * Try making usage (when and how) clearer in the api and with examples in doc

### 0.0.5
* Dive into Reducers a bit more
  * Generalize Towards(Zero) reducer
  * More reducers
* add a ReportMode to config maybe ? disabling traces and/or warnings
  * [   ] ReportMode {Traces | Warnings | None}

### 0.0.5+
* look into different collection shrinking
  * limitations
  * known issues
  * workarounds
* Deliberation Shrinking. Add:
  * a stop when predicate like executioncount <= 4, user can then up the number of runs to 10000 relatively safely
  * decouple the casefile from the predicate
* Stabilize Investigations
* async & concurrency (spike cooking in Experimental, see TheNeedlerPattern)

## Doc Work
* Cookbook Additions
  * statefull list maybe
  * Scheduling: Non-overlapping bookings that do overlap, needs explanation
* Start working on the reference

### Ideas
* QuickPulse.Show turn manofwealth and taste into composable config
  => for reporting inputs : ShrinkingClue.Ignore property => ignore in QPS config => print full object

## Refactoring and Testing Targets 
* CaseSummary DepositionTests
* Nested InputShrinkingClue ? Refactoring Target
* Loose the lazy IEnumerable in shrinking (yield)
## Other
* Scour QuickAcid for any interesting tests left behind.
