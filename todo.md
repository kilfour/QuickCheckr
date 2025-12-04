## Current Version: 0.0.3
* Save Reports with _Seed
* Save Vault Reports, Sub folder ?
* Generalize Towards(Zero) reducer
* Refine the Reducer Idea

### Major
* look into different collection shrinking, not blocking but would be nice to atleast have it docced before 0.0.2
  * limitations
  * known issues
  * workarounds
* DOC known issues, limitations (null issues in object shrinking, multiple same values => greedy shrinking)
* DOC investigations
* Doc configuration

### Minor 
* test ActCarefully more (check if QuickAcid has some), niche combinator so it can wait
* Nested InputShrinkingClue ? ( not needed right now but i think it be cleaner)
* Verdict nullabillity => ties in with "report original run"
* QuickCheckr.Tests.Verification.SwapperTests: 
  * main tests prove that this works, but it would help to to test this in isolation
* Loose the lazy IEnumerable in shrinking (yield)
* Scour QuickAcid for any interesting tests left behind.

### Other
* QuickPulse: add a log to file here artery