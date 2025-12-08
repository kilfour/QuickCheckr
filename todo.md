## Current Version: 0.0.3

### Doing
* Doc configuration
  * Started see the 'TheEvidenceVault', turn 'ConfiguringTheRunner' into executable examples
* Add an explanation to QuickCheckr.Tests.Docs.H_OnShrinking.F_DeliberationShrinking
* Multi-start run mode to automatically search for the simplest failing case
  - Re-run the Checkr with fresh seeds N times
  - Shrink each failing run as usual
  - Keep the best case using a comparator:
        (Casefile a, Casefile b) => 
            a.Executions.Count <= b.Executions.Count ? a : b
  - At end, re-run best seed once and surface as final failure
  This is looking good, maybe add:
  a stop when predicate like executioncount <= 4, user can then up the number of runs to 10000 relatively safely
 
### Must have's for 0.0.4
* Vault interface finished, implementation stable

### Major

* Implement: checkr.CloseResolvedColdCases(...)
* Create an artery derived from File.Log that deletes file if receiving empty collection as param to absorb.
* DOC known issues, limitations (null issues in object shrinking, multiple same values => greedy shrinking)
  * Consolidation: Lots of info and tests in notes

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
* QuickPulse: FileLogArtery virtual Absorb
* QuickPulse: add a 'log to file here' artery, spike it in quickcheckr first
* QuickPulse.Explains: CodeReplace with empty string => ploef
