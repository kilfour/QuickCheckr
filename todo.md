## Current Version: 0.0.3

### Doing
* Doc configuration
  * Started see the 'TheEvidenceVault'
  * DOC investigations
* Finish Deliberation Shrinking
  This is looking good, maybe add:
  * a stop when predicate like executioncount <= 4, user can then up the number of runs to 10000 relatively safely
  * decouple the casefile from the predicate
 
### Must have's for 0.0.4
* Vault interface finished, implementation stable
* Deliberation Shrinking (mostly) Finished

### Major
* The Guide never explicitly says “to replay this seed, call checkr.Run(seed)
* Implement: checkr.CloseResolvedColdCases(...)
* Create an artery derived from File.Log that deletes file if receiving empty collection as param to absorb.
* DOC known issues, limitations (null issues in object shrinking, multiple same values => greedy shrinking)
  * Consolidation: Lots of info and tests in notes
* look into different collection shrinking
  * limitations
  * known issues
  * workarounds
* Start working on the reference
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
* QuickPulse: FileLogArtery virtual Absorb
* QuickPulse: add a 'log to file here' artery, spike it in quickcheckr first
  * See QuickCheckr.Tests.PersonalNotes.T_Examples.A_TheElementRemover.TheElementRemover.cs
* QuickPulse.Explains: CodeReplace with empty string => ploef
