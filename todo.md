## Current Version: 0.0.5
* move trackr to QuickCheckr.Experimental, add broadcaster example

### Doc current focus
* Doc Configuration
  * [   ] FileAs : add CaseSummary info
  * [ x ] Custodian
  * [ x ] ShrinkMode
  * [   ] DeliberationPolicy
  * [   ] Investigations
  
* Build HorsesForCourses.Api for testing example
  * Initial Version Ready (auth is hardcoded admin for now)

### 0.0.6
* look into different collection shrinking
  * limitations
  * known issues
  * workarounds

### 0.0.6+
* Dive into Reducers a bit more
  * Generalize Towards(Zero) reducer
  * More reducers
* add a ReportMode to config maybe ? disabling traces and/or warnings
  * [   ] ReportMode {Traces | Warnings | None}
* Editor: configure QuickPulse.Show 
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
