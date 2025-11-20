Debugging is ephemeral. You step through the code, watching states evolve and building a mental model, until you tap F10 at the wrong moment.
Suddenly everything you were holding in your head is gone, the trail erased, and you’re forced to start over.

QuickCheckr eliminates that fragility.
Every run produces a persistent artifact you can inspect, rerun, and share. The reporting is fully customizable, and shrinking is adjustable, right up to the point where you capture the true minimal failing case, even when the default settings occasionally misfire.

---

Section: Why Transparency Matters

You could put this right after the first example + report:

Writing good property-based tests is hard.

When a test fails, you always have two questions:

Is the implementation wrong?

Or did I mess up the test (or the generator, or the shrinker)?

QuickAcid is built on the assumption that you can’t answer those questions from a single boolean result.
You need to see what happened.

Section: Transparency as a First-Class Feature

Something like:

Many PBT tools optimise for minimal counterexamples and treat everything else as noise.
QuickAcid goes one step further: it optimises for explainable counterexamples.

That’s why the report always shows:

Inputs – the actual values fed into each step of the script

Specs – which checks passed, which one failed

Trace – any extra diagnostics you attached yourself

Shrinking history – how many executions it took and how far it shrank

This isn’t just decoration. It’s how you:

see whether your spec is too strict or just wrong,

notice when your generator isn’t producing the right shapes,

and decide whether you need to tweak shrinking (maybe you’re shrinking away the interesting context).

Tie it directly to the list example

Right after the report with the trace:

Executed (4): Delete from List
 - Input: List = [ 3, 3 ]
 - Input: Item to Remove = 3
 - Trace: Original list = [ 2, 3, 0, 5, 7, 2, 3, 9, 6 ]


You can explicitly show how transparency answers both “who is wrong?” questions:

Is the implementation wrong?
From the minimal case [3, 3] and the spec “list does not contain the element”, it’s obvious that:

the spec is reasonable, and

ListDeleter.Remove only removes the first 3, so the implementation is the culprit.

Is the generator OK?
The trace line proves the generator can produce realistic, messy input ([2, 3, 0, 5, 7, 2, 3, 9, 6]) and that shrinking preserved the essence of the bug.
So you don’t waste time suspecting the generator.

You could even add a short note like:

If this trace had shown something bizarre – say an empty list, or a list that never contained the element – that would be a strong hint that our test or generator was wrong instead of the implementation.

Shrinking & generation tweaking (explicit call-out)

A compact paragraph to close the loop:

In more complex cases, the minimal counterexample isn’t enough on its own.
You may need to:

bias the generator towards certain patterns,

or adjust the shrinking strategy so it doesn’t throw away important structure.

QuickAcid’s reporting is designed for that feedback loop:

Inspect the inputs and traces in the failing report.

Decide whether the implementation, spec, generator or shrinker is at fault.

Tweak the one that’s wrong.

Run again and compare the new report.

Transparency isn’t an add-on; it’s how you learn to write good properties.