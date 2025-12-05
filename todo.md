## Current Version: 0.0.3

### Major
* 
* DOC known issues, limitations (null issues in object shrinking, multiple same values => greedy shrinking)
  * Consilidation: Lots of info and tests in notes
* Doc configuration
  * Started see the 'TheEvidenceVault', turn 'ConfiguringTheRunner' into executable examples
* look into different collection shrinking
  * limitations
  * known issues
  * workarounds
* Start working on the reference
* DOC investigations
* Generalize Towards(Zero) reducer
* More reducers

### Minor 
* Nested InputShrinkingClue ? Refactoring Target
* QuickCheckr.Tests.Verification.SwapperTests: 
  * Main tests prove that this works, but it might help to to test this in isolation
    Kind of not needed maybe, but tests are there in comment, ... if there's a need.
    Might help with mem stuff in future.
* Loose the lazy IEnumerable in shrinking (yield)
* Scour QuickAcid for any interesting tests left behind.

### Other
* QuickPulse: add a 'log to file here' artery, spike it in quickcheckr first
* QuickPulse.Explains: CodeReplace with empty string => ploef
