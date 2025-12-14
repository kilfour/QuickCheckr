## Current Version: 0.0.4
* Doc Configuration
  * [ x ] FileAs  
  * [   ] Custodian
  * [   ] WithTraces => ReportMode {Traces | Warnings| none}
  * [ x ] ShrinkMode
  * [   ] DeliberationPolicy
  * [   ] Investigations
  * [   ] Editor
  
### Issues 
* did we catch em all, => Renamed:
  * Spec to Expect
  * SpecIf to ExpectWhen
  * Assay to Verify 
  * Analize to Expect
  * ProvenWhen to StopWhenProven 
* Fix Exception to use right combinator in message

### On the Table, a.k.a. Doing 
* Build HorsesForCourses.Api for testing example
  * Initial Version Ready (auth is hardcoded admin for now)

### 0.0.5
* Dive into Reducers a bit more

### 0.0.5+
* look into different collection shrinking
  * limitations
  * known issues
  * workarounds
* Deliberation Shrinking. Add:
  * a stop when predicate like executioncount <= 4, user can then up the number of runs to 10000 relatively safely
  * decouple the casefile from the predicate
* Stabilize Investigations
* Generalize Towards(Zero) reducer
* More reducers
* async & concurrency (spike cooking in Experimental)

## Doc Work
* CaseSummary

* Cookbook Additions
  * statefull list maybe
  * Scheduling: Non-overlapping bookings that do overlap, needs explanation
* Start working on the reference

### Ideas
* QuickPulse.Show turn manofwealth and taste into composable config
  => for reporting inputs : ShrinkingClue.Ignore property => ignore in QPS config => print full object

## Refactoring and Testing Targets 
* Nested InputShrinkingClue ? Refactoring Target
* Loose the lazy IEnumerable in shrinking (yield)
## Other
* Scour QuickAcid for any interesting tests left behind.



# Taskr – Next Step Ideas

A short, opinionated set of next steps for **Taskr** in QuickCheckr.

## 1) End-of-run cleanup as a guarantee

Async spikes become painful the moment tasks leak across executions/runs.

**Add:**

* `Taskr.CancelAll()` / `Taskr.StopAll()`
* Invoked automatically at end-of-run (or opt-in via config).

**Config knobs (optional):**

* `OnRunEnd = Cancel | Await | Detach`

**Goal:** make Taskr-safe-by-default.

---

## 2) Start should capture evidence by default

When a task faults/cancels/times out, users want the story in the report.

**Enhance:**

* `Taskr.Start("label", ct => ...)` records:

  * label
  * status: `Completed | Faulted | Canceled | TimedOut`
  * exception (inner + stack)
  * any relevant inputs already captured by the Checkr

**Goal:** “Task failed” becomes a first-class, nicely reported failure mode.

---

## 3) First-class await points

Most async properties need explicit sync points.

**Add:**

* `Taskr.AwaitAll("await all")`
* `Taskr.AwaitAny("await any")`
* `Taskr.AwaitOne("await one")` (random from pool)

**Return shapes (pick one style):**

* raw results (for `T`)
* or `Outcome<T>` (like an async-friendly `ActCarefully`)

**Goal:** remove the need for `.GetAwaiter().GetResult()` inside `Expect`.

---

## 4) Timeout as modelling tool

Timeouts are not only safety rails; they’re often the property.

**Add:**

* `Taskr.AwaitAll(timeout: …)`
* `Taskr.AwaitOne(timeout: …)`

**Report:** timeouts as a distinct outcome (not “canceled”, not “faulted”).

**Goal:** “completes promptly” checks become idiomatic.

---

## 5) Lightweight concurrency shaping

Without building a full scheduler, provide a few practical shapes.

**Add (lightweight):**

* `Taskr.LimitConcurrency(max: n)` (queue starts)
* `Taskr.Yield()` / `Taskr.Step()` (insert a scheduling “breath”)

**Goal:** widen race windows intentionally, without inventing a fake runtime.

---

## 6) Leak detection / invariants about background work

Fire-and-forget bugs are common and painful.

**Add:**

* `Taskr.ExpectNoneRunning("No background tasks")`
* or `Taskr.VerifyNoneRunning(...)` (run-scoped)

**Goal:** ensure tests don’t hide background activity.

---

## 7) Better pooling semantics

Make pooling tasks feel as native as Trackr pooling.

**Add/Align:**

* `Taskr.PoolFor<T>(key?)`
* `Taskr.Pool("label", task)` (explicit store)
* `Taskr.ClearPool()` per run

**Goal:** Taskr pools become a clean modelling tool for multi-step async behaviour.

---

## Suggested “best next step”

If you want maximum payoff with minimal surface area:

1. **Cleanup guarantee** (Cancel/Stop all tasks at run end)
2. **Await combinators** (`AwaitAll` / `AwaitAny` / `AwaitOne`)

Those immediately make async Checkrs safer and cleaner.


