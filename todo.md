## Short Term Work (Current Version: 0.0.3)

### Issues 

### Ideas
* Try to identify wonky shrinking through EvidenceRecorder.RecordFailure(Exception exception)
  * Warn the user, suggest fix.
* Add type to shrinking clue (maybe)
* QuickPulse.Show turn manofwealth and taste into composable config
  => for reporting inputs : ShrinkingClue.Ignore property => ignore in QPS config => print full object
* See if we can not do some QuickFuzzr magic under the hood to avoid some of the remaining issues
  * instead of null, try Fuzzr.One(type) maybe, this might throw though so ...
    * quickfuzzr shrinking config strategy ? 
      Shrinkr... == CheckrOf<Case>, just registers and returns `s => CheckrResult.CaseOnly(s);`

### 0.0.4
* Doc Issues (and fix if possible) **<= Doing**  

### 0.0.4+
* Deliberation Shrinking. Add:
  * a stop when predicate like executioncount <= 4, user can then up the number of runs to 10000 relatively safely
  * decouple the casefile from the predicate
* Stabilize Investigations

### After that
* Generalize Towards(Zero) reducer
* More reducers

## Doc Work
* Doc configuration
  * Started see the 'TheEvidenceVault'
  * Investigations
* Known issues, limitations (null issues in object shrinking, multiple same values => greedy shrinking)
  * Consolidation: Lots of info and tests in notes
* look into different collection shrinking
  * limitations
  * known issues
  * workarounds
* Cookbook
  * statefull list, play on listremover, but with two methods, add & remove 
    * fix trace to promote to cookbook (FIXED)
  * Scheduling: Non-overlapping bookings that do overlap, needs explanation
* Start working on the reference

## Refactoring and Testing Targets 
* Nested InputShrinkingClue ? Refactoring Target
* QuickCheckr.Tests.Verification.SwapperTests: 
  * Main tests prove that this works, but it might help to to test this in isolation
    Kind of not needed maybe, but tests are there in comment, ... if there's a need.
    Might help with mem stuff in future.
* Loose the lazy IEnumerable in shrinking (yield)

## Other
* Scour QuickAcid for any interesting tests left behind.
* QuickPulse: add a 'log to file here' artery, spike it in quickcheckr first
  * See QuickCheckr.Tests.Notes.T_Examples.A_TheElementRemover.TheElementRemover.cs
* QuickPulse.Explains: CodeReplace with empty string => ploef
