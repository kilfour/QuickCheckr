### Main
* check OneOf, more than one in same checkr => same key => problem
* guard against reuse keys all over
## Keys as Stable Identifiers



If QuickCheckr detects that a key is written more than once, it throws a descriptive exception immediately. This ensures:

* **Consistent, trustworthy reports** – every label refers to exactly one value or action.
* **Predictable shrinking** – the engine always knows which value corresponds to which key.
* **Early, precise feedback** – accidental reuse is caught instantly during development.


* passed specs
* port
  * the remaining collection shrinking tests
  * object shrinking
* QOL (major): throw exception when multiple writes occur to the same memorybank with the same key on the same execution
* async !!

### Minor
!! fix InputShrinking shrinkCount

InputShrinker.Run never increments shrinkCount (Runners/InputShrinking/InputShrinker.cs (lines 8-34)), so the shrink statistics always report zero even when clues are found. If that metric matters later (docket, case file), it needs to be wired up.

* Nested InputShrinkingClue ?
* Verdict nullabillity => meh


