## Short Term Work (Current Version: 0.0.3)

### Ideas
* Try to identify wonky shrinking through EvidenceRecorder.RecordFailure(Exception exception)
  * Warn the user, suggest fix.

### 0.0.4
* Vault interface finished, implementation stable
  * Implement: checkr.CloseResolvedColdCases(...)
  * Create an artery (or something else) derived from File.Log that deletes file if receiving empty collection as param to absorb.

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
* QuickPulse: FileLogArtery virtual Absorb
* QuickPulse: add a 'log to file here' artery, spike it in quickcheckr first
  * See QuickCheckr.Tests.PersonalNotes.T_Examples.A_TheElementRemover.TheElementRemover.cs
* QuickPulse.Explains: CodeReplace with empty string => ploef
