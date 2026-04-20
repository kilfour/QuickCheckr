

# QuickCheckr – Exception Behaviour (Centralized)

This document centralizes **all exception-related behaviour** in QuickCheckr.
It describes what happens when exceptions occur in each combinator, how they are reported, and how they interact with shrinking.

---

## 1. Checkr.Input

### Eager Input (default)

* If the fuzzr throws during generation:

  * The exception is **wrapped** in `InputFuzzrThrewException`
  * The original exception is available via `InnerException`
  * The report includes a **hint** suggesting the lazy version

👉 Intent:

* Prevents silent crashes
* Makes fuzzr failures diagnosable

### Lazy Input

* Exceptions are deferred until execution
* Behaves like a normal runtime failure (similar to `Act`)

---

## 2. Checkr.Act

* If the action throws:

  * The execution **fails immediately**
  * The exception is **reported directly** (not wrapped)
  * Stack trace is included (depending on `ReportMode`)

👉 Key property:

* This is the **standard failure path** for runtime errors

---

## 3. Checkr.Perform

* Semantically equivalent to `Act`, but:

  * Does **not produce a value**
  * Only contributes to execution side-effects

### Exception behaviour

* If the action throws:

  * The execution **fails immediately**
  * Exception is **reported directly** (same as `Act`)

👉 Important note:

* Historically had a **missing guard** (bug)
* Now aligned with `Act` semantics

---

## 4. Checkr.ActCarefully

* Executes an action but **captures exceptions instead of failing immediately**

### Behaviour

* If the action throws:

  * Result is captured as a **delayed failure value**
  * Accessible via:

    * `result.Threw`
    * `result.ThrewAs<T>()`

👉 Intent:

* Enables assertions *about* exceptions
* Supports testing failure conditions without aborting the run

---

## 5. Checkr.Expect

### Normal behaviour

* Returns pass/fail

### If expectation throws

* Exception is:

  * **Wrapped** in `ExpectationThrowsException`
  * Original exception stored as `InnerException`
  * Expectation key is included in the message

👉 Example message:
"The expectation 'X' threw a 'InvalidOperationException'."

👉 Intent:

* Makes it explicit that the *assertion itself* is broken

---

## 6. Checkr.ExpectWhen

* Same behaviour as `Expect`, but conditional

### Important nuance

* If condition is false:

  * Expectation is **not executed**
  * No exception can occur

* If condition is true and expectation throws:

  * Same wrapping as `ExpectationThrowsException`

---

## 7. Shrinking & Exceptions

### Execution identity

* Exceptions participate in shrinking
* The **execution index matters**
* QuickCheckr preserves which execution caused the failure

👉 Result:

* Minimal failing case still reproduces the same failing execution

### Reduction behaviour

* Reducers attempt to simplify inputs
* Failure must still:

  * Throw the same exception OR
  * Violate the same expectation

### Warnings

* If reducers cannot find a passing case:

  * Warning is emitted

---

## 8. Reporting

### Exception display

* Includes:

  * Exception type
  * Message
  * Stack trace (optional via `ReportMode`)

### Variants

* Direct exceptions (Act/Perform)
* Wrapped exceptions (Expect, Input)
* Captured exceptions (ActCarefully)

---

## 9. DSL Layer (QuickTestr / SystemCheckr)

* Exceptions surface through:

  * `.Actual(...)`
  * `.Perform(...)`
  * `.Example(...)`

### Behaviour

* Mirrors underlying Checkr semantics
* Exceptions are treated as **observed outcomes** in oracle tests

---

## 10. Summary Table

| Combinator    | Throws? | Wrapped? | Fails Run? | Capturable? |
| ------------- | ------- | -------- | ---------- | ----------- |
| Input (eager) | Yes     | Yes      | Yes        | No          |
| Input (lazy)  | Yes     | No       | Yes        | No          |
| Act           | Yes     | No       | Yes        | No          |
| Perform       | Yes     | No       | Yes        | No          |
| ActCarefully  | Yes     | No       | No         | Yes         |
| Expect        | Yes     | Yes      | Yes        | No          |
| ExpectWhen    | Yes     | Yes      | Yes        | No          |

---

## 11. Design Intent

QuickCheckr distinguishes **three categories of exceptions**:

1. **System failures** (Act / Perform)
   → fail fast, report directly

2. **Assertion failures** (Expect)
   → wrapped for clarity

3. **Observed failures** (ActCarefully)
   → captured as data

This separation enables:

* Clear diagnostics
* Precise shrinking
* Expressive DSLs on top

---

## 12. Known Gaps / To Verify

* Explicit docs for `Perform` (now fixed, but under-documented)
* Clear examples of `InputFuzzrThrewException`
* Cross-linking between shrinking and exception behaviour
* More examples in DSL docs (QuickTestr)

---

## 13. Suggested Tests to Add

* Input fuzzr throws (eager vs lazy)
* Perform throws (regression test for missing guard)
* ActCarefully capturing multiple exception types
* Expect throwing vs returning false
* Shrinking preserves exception type

---

End of document.
