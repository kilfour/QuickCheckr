## Current Version: 0.0.1

### Major
* Disable/Hide ActionShrinking completely for first release ? ... I do think so.
* Setup Discord Server
* port
  * object shrinking tests (multiple properties only ones left)
* look into different collection shrinking, not blocking but would be nice to atleast have it docced before 0.0.2
  * limitations
  * known issues
  * workarounds
* DOC investigations
* Doc configuration
* check out DX in NUnit, MSTest and TUnit

### If time allows
Report original run or even 
  * Report variations Enableshrinking Bitwise Enum 

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
* QuickPulse.Explains [DocFile] => just straight include file, usefull for .md's