### Major
* QOL (major): throw exception when multiple writes occur to the same memorybank with the same key on the same execution
  * check OneOf, more than one in same checkr => same key => problem
  * guard against reuse keys all over (already done for input and another one)
* port
  * object shrinking tests, (just records left basically, all seems to work though)
* async : Act, add overloads ...If and Action variants
* test ActCarefully more
* !! ShrinkEachElementStrategy.GetPlaceholder => can cause major issues. FIND THEM !! 
* verify shrink count in all the existing tests, about 50% done and seems to work, but you never know
* look into different collection shrinking

### Minor 
* Nested InputShrinkingClue ? ( not needed right now but i think it be cleaner)
* Verdict nullabillity => meh
* QuickCheckr.Tests.Verification.SwapperTests 
* Loose the lazy IEnumerable in shrinking (yield)