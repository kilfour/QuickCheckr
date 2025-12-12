## Short Term Work (Current Version: 0.0.3)

### Issues 

### 0.0.4
* Ready (try it out on ResultR Open Source Project: DONE)

### 0.0.4+
* look into different collection shrinking
  * limitations
  * known issues
  * workarounds
* Deliberation Shrinking. Add:
  * a stop when predicate like executioncount <= 4, user can then up the number of runs to 10000 relatively safely
  * decouple the casefile from the predicate
* Stabilize Investigations

### After that
* Generalize Towards(Zero) reducer
* More reducers
* Port Concurrency Spike from QuickAcid

## Doc Work
* CaseSummary
* Configuration
  * Started see the 'TheEvidenceVault'
  * Investigations
* Cookbook Additions
  * statefull list maybe
  * Scheduling: Non-overlapping bookings that do overlap, needs explanation
* Start working on the reference

### Ideas
* QuickPulse.Show turn manofwealth and taste into composable config
  => for reporting inputs : ShrinkingClue.Ignore property => ignore in QPS config => print full object

## Refactoring and Testing Targets 
* Nested InputShrinkingClue ? Refactoring Target
* QuickCheckr.Tests.Verification.SwapperTests: 
  * Main tests prove that this works, but it might help to to test this in isolation
    Kind of not needed maybe, but tests are there in comment, ... if there's a need.
    Might help with mem stuff in future.
* Loose the lazy IEnumerable in shrinking (yield)
* Unify :
```csharp
try
{
    return CheckrResult.Some(state, /* Something Something */);
}
catch (Exception exception)
{
    state.Docket.Record(new EvidenceFailure(exception));
    return CheckrResult.Default<T>(state);
}
```
## Other
* Scour QuickAcid for any interesting tests left behind.
* QuickPulse: add a 'log to file here' artery, spike it in quickcheckr first
  * See QuickCheckr.Tests.Notes.T_Examples.A_TheElementRemover.TheElementRemover.cs
* QuickPulse.Explains: CodeReplace with empty string => ploef
