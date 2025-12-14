## Current Version: 0.0.4

### Issues 
* did we catch em all, => Renamed:
  * Spec to Expect
  * SpecIf to ExpectWhen
  * Assay to Verify 
  * Analize to Expect
  * ProvenWhen to StopWhenProven 
* Fix Doc Configuration ShrinkMode: better checkr for execution shrinking report
* Fix Exception to use right combinator in message

### On the Table, a.k.a. Doing 
* Build HorsesForCourses.Api for testing example
  * Initial Version Ready (auth is hardcoded admin for now)

### 0.0.5
* Dive into Reducers a bit more

### 0.0.5+
* look into different collection shrinking
  * limitations
  * known issues
  * workarounds
* Deliberation Shrinking. Add:
  * a stop when predicate like executioncount <= 4, user can then up the number of runs to 10000 relatively safely
  * decouple the casefile from the predicate
* Stabilize Investigations
* Generalize Towards(Zero) reducer
* More reducers
* async & concurrency (spike cooking in Experimental)

## Doc Work
* CaseSummary
* Configuration
  * Started see the 'TheEvidenceVault'
  * Investigations
* Cookbook Additions
  * statefull list maybe
  * Scheduling: Non-overlapping bookings that do overlap, needs explanation
* Start working on the reference

### Ideas
* QuickPulse.Show turn manofwealth and taste into composable config
  => for reporting inputs : ShrinkingClue.Ignore property => ignore in QPS config => print full object

## Refactoring and Testing Targets 
* Nested InputShrinkingClue ? Refactoring Target
* Loose the lazy IEnumerable in shrinking (yield)
## Other
* Scour QuickAcid for any interesting tests left behind.

