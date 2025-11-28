## Current Version: 0.0.1

### Major
* verify shrink count in all the existing tests, about 50% done and seems to work, but you never know
* port
  * object shrinking tests, (just records left basically, all seems to work though)
* async : Act, add overloads ...If and Action variants
* QOL (major): throw exception when multiple writes occur to the same memorybank with the same key on the same execution
  * check OneOf, more than one in same checkr => same key => problem
  * guard against reuse keys all over (already done for input and another one)
* look into different collection shrinking, not blocking but would be nice to atleast have it docced before 0.0.2
  * limitations
  * known issues
  * workarounds
* !! ShrinkEachElementStrategy.GetPlaceholder => can cause major issues. FIND THEM !! 
  * again not blocking for 0.0.2 but it would help communication spelling out to users that there might be an issue
* test ActCarefully more (check if QuickAcid has some)


### Minor 
* Nested InputShrinkingClue ? ( not needed right now but i think it be cleaner)
* Verdict nullabillity => meh
* QuickCheckr.Tests.Verification.SwapperTests: 
  * main tests prove that this works, but it would help to to test this in isolation
* Loose the lazy IEnumerable in shrinking (yield)
* Scour QuickAcid for any interesting tests left behind.