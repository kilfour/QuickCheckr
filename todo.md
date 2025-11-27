### Main
!! fix InputShrinking shrinkCount
* QOL (major): throw exception when multiple writes occur to the same memorybank with the same key on the same execution
  * check OneOf, more than one in same checkr => same key => problem
  * guard against reuse keys all over
* port
  * the remaining collection shrinking tests, almost done 
  * object shrinking, (just records left basically, all seems to work though)
* async : Act, add overloads ...If and Action variants
* test ActCarefully more

### Minor


InputShrinker.Run never increments shrinkCount (Runners/InputShrinking/InputShrinker.cs (lines 8-34)), so the shrink statistics always report zero even when clues are found. If that metric matters later (docket, case file), it needs to be wired up.

* Nested InputShrinkingClue ?
* Verdict nullabillity => meh
* QuickCheckr.Tests.Verification.SwapperTests