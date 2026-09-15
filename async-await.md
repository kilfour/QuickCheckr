# Async/await migration

## Goal

Make QuickCheckr await user operations throughout initial execution, replay,
shrinking, tracing, and case selection. Carry that capability through QuickTestr.
Keep execution sequential so seeds, candidate order, and replay remain reproducible.
Async capability does not imply parallel execution.

The first acceptance targets are the two QuickTestr tests currently skipped with
`Causes a deadlock`:

- `AsyncPropertyTests.AwaitsAnAsyncInvariant`
- `OracleTaskResultTests.AwaitsTaskResultsFromBothSides`

Both use `Task.Yield()` to force suspension. Completed-task tests alone cannot
demonstrate genuine async support. Add suspending failures to exercise replay and
shrinking as well as successful execution.

## Incremental implementation

Each step should build and pass its relevant tests before moving on.

### 1. Awaitable entry points and outer runners (implemented)

- Add `RunAsync()` overloads matching all existing `Run()` overloads, for both
  `CheckrOf<Case>` and `ConfiguredCheckr`, returning the original receiver in a task.
- Preserve run counts, execution counts, seeds, configuration, reporting, and
  failure exceptions.
- Add `CaseOfficer.InvestigateAsync()` and await each `CheckrRunner.RunAsync()`.
  Share the investigation loop with the synchronous entry point.
- Keep `CheckrRunner.RunAsync()` as a completed `ValueTask` adapter over the
  existing synchronous runner for now. No thread-pool offloading or forced yields.
- Keep `.Run()` working. `CaseOfficer.Investigate()` is the explicit synchronous
  compatibility boundary; its awaited work still completes synchronously here.
- Put all new QuickCheckr feature tests in `QuickCheckr.Tests/Concurrency`.

**Limitation:** This step only prepares the outer plumbing. Scripts, replay,
shrinking, and existing task callback wrappers still execute synchronously.
Calling `RunAsync()` does not yet fix callbacks that suspend, and the two
QuickTestr tests remain skipped.

### 2. Awaitable replay and shrinking (implemented)

- Introduce awaitable initial execution, tracing, case selection, execution/action
  shrinking, and input shrinking, replacing the outer runner's synchronous adapter.
- Thread awaits through `Verifier`, classification dispatch, collapse operations,
  and reduction loops. Verification callbacks become awaitable.
- Keep candidate generation and processing of completed results synchronous.
- Keep memory swaps and replay scopes alive across awaits, and preserve cleanup,
  failure selection, traversal order, and shrink counts.
- Continue running synchronous scripts until the core delegate changes.

The runner now awaits every replay/shrinking phase. `ScriptExecution.RunAsync()`
is the single temporary adapter for invoking the still-synchronous script delegate;
step 3 replaces that adapter when the delegate changes. Async execution paths do
not call synchronous verifier/reduction wrappers retained for existing internal tests.
Pure fallback classification and candidate generation remain synchronous.

Replay loops dispose the stash in `finally` blocks, temporary memory scopes span
awaits, and tracing/case-selection flags reset even if script execution or disposal
throws. An interrupted action-removal attempt restores the action before propagating
the exception.

Step 2 tests in `Concurrency` verify suspended reduction callbacks and dispatcher
ordering, exception propagation, stash disposal, and temporary replay state cleanup.
The public script delegate and async action wrappers are unchanged, so genuinely
suspending user actions and the QuickTestr deadlock tests still await steps 3â€“5.

### 3. Awaitable script delegate and composition (implemented)

- Change `CheckrOf<T>` to return `ValueTask<CheckrResult<T>>`.
- Update LINQ composition, step builders, sequences, choices, pool operations,
  and direct delegate invocations together so the repository builds.
- Let synchronous steps return completed results; await steps that compose others.
- Update existing consumers and test helpers for the delegate contract change.
  New async feature tests still belong in `Concurrency`.

This is the broadest coordinated step. The shrinking algorithms remain intact;
the signatures and invocation sites change.

`CheckrOf<T>` now returns `ValueTask<CheckrResult<T>>`. Synchronous result factories
remain synchronous; a conversion from `CheckrResult<T>` produces a completed
`ValueTask`, so ordinary synchronous step lambdas need no async state machine.
Method-group consumers must use an awaitable return signature or wrap the factory
in a lambda. Direct consumers must await the result, including side-effect-only
invocations.

LINQ binds and projections await their source and respect a stopped run after
suspension. Conditional steps, option evaluation, once-only operations, and pool
selection await nested scripts. Sequence and other simple wrappers forward their
nested awaitable result directly; shared composition awaits it. Option and pool
traversal remain sequential.

The temporary `ScriptExecution` adapter has been removed: initial execution and
all replay loops await the script delegate directly. New `Concurrency` tests cover
suspended composition, conditional skipping, replay candidate lifetime, and parity
of shrinking/reporting between synchronous and suspending custom scripts. Replay
cleanup tests now also throw after suspension.

**Remaining limitation:** Custom async script delegates now work end to end through
`RunAsync()`. Existing task-based `Act`, `ActCarefully`, `Capture`, and related
helpers still contain blocking wrappers; replacing them belongs to step 4.
QuickTestr integration and its two skipped acceptance tests remain step 5.

### 4. Genuine asynchronous actions (implemented)

- Replace blocking task callback wrappers with awaited invocation, including
  conditional and carefully handled actions and exception capture.
- Test actual suspension on passing and failing paths, including replay, shrinking,
  tracing, and exceptions thrown after suspension.
- Establish cancellation propagation and async stash disposal before claiming full
  async lifetime support. Cancellation must remain distinct from a falsifying result.
- Keep `.Run()` as an explicitly synchronous compatibility API. Do not promise
  deadlock-free use of suspending callbacks through that API; use `RunAsync()`.

Step 4 removes blocking awaits from QuickCheckr's task-based action helpers,
including conditional, carefully handled, shrinkable, capture, and perform variants.
`ActCarefully` and `Perform` now also accept task factories. Prefer factories for
replay: legacy overloads accepting an already-created `Task` await that same task
again and cannot re-invoke the original operation.

Every `RunAsync` overload accepts an optional final `CancellationToken`. Custom
steps can read `CheckrState.CancellationToken`; token-aware action/capture/perform
and shrinkable overloads pass it to callbacks. Execution checks cancellation
between steps, runs, and replays and before publishing results. An
`OperationCanceledException` from a callback propagates as cancellation, including
through carefully handled steps, rather than becoming recorded evidence.
Cancellation is cooperative: callbacks which ignore the token must complete before
their replay state and stash are released. No detached tasks or background replay.

All replay/initial-run cleanup now awaits stash disposal. `IAsyncDisposable` takes
precedence over `IDisposable` for resources implementing both. Cleanup does not use
the canceled run token, and still attempts remaining resources if one disposal
fails. A single disposal exception is rethrown unchanged; multiple disposal
exceptions are aggregated. Existing synchronous stash resources still work.

The Concurrency tests cover genuinely suspended helpers, post-await exceptions,
conditional/skipped factories, async shrinking with fresh resources, cancellation
before execution and during replay, waiting for cleanup, and disposal failures.
The full engine path now supports task-based actions through `RunAsync()`; the
QuickTestr builder integration and its two skipped tests remain step 5.

### 5. QuickTestr integration

- Preserve asynchronous delegates in property and oracle builders, including
  two-input and deliberated variants, instead of blocking them into synchronous delegates.
- Add QuickTestr `RunAsync()` and await QuickCheckr through to reporting.
- Convert the two acceptance tests to `async Task` and `await ...RunAsync()`,
  remove their skips, and verify them on GitHub Actions.
- Test mixed synchronous/asynchronous oracle sides and suspending falsifications.

## Validation

For step 1, cover every public overload, receiver identity, requested counts,
seeded parity with `.Run()`, configured filing, failure propagation, and shrinking
parity. Run the existing suites to guard synchronous callers and case selection.

Later steps must add tests that genuinely suspend and bound deadlock regressions
with test-runner timeouts. Preserve generated reports and existing repository files
when running documentation-producing tests. Do not parallelize shared replay state.
