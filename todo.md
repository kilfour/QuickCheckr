!! fix InputShrinking shrinkCount

InputShrinker.Run never increments shrinkCount (Runners/InputShrinking/InputShrinker.cs (lines 8-34)), so the shrink statistics always report zero even when clues are found. If that metric matters later (docket, case file), it needs to be wired up.

* new shrinking strat:
  * Input(Fuzzr..., cfg.ReuseFuzzrShrinking(10))
* Nested InputShrinkingClue ?
* Verdict nullabillity => meh


