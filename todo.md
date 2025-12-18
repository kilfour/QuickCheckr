## Current Version: 0.0.5
* Error Message if pool does not exist (same for stashed)
* Document OneOfIf behaviour during shrinking, try to find holes in the logic (seems to work)
* Nested OneOf/OneOfIf
* Maybe retrieve line number of failed Expectation (perf ?)
* Consistency If => When
* ActCarefully now has None result. (also for act) ?

### Doc current focus
* Doc Configuration
  * [   ] FileAs : add CaseSummary info
  * [ x ] Custodian
  * [ x ] ShrinkMode
  * [   ] DeliberationPolicy
  * [   ] Investigations
  
* Build HorsesForCourses.Api for testing example
  * Initial Version Ready (auth is hardcoded admin for now)

### 0.0.6
* look into different collection shrinking
  * limitations
  * known issues
  * workarounds
* update xml summaries

### 0.0.6+
* Dive into Reducers a bit more
  * Generalize Towards(Zero) reducer
  * More reducers
* add a ReportMode to config maybe ? disabling traces and/or warnings
  * [   ] ReportMode {Traces | Warnings | None}
* Editor: configure QuickPulse.Show 
* Deliberation Shrinking. Add:
  * a stop when predicate like executioncount <= 4, user can then up the number of runs to 10000 relatively safely
  * decouple the casefile from the predicate
* Stabilize Investigations
* async & concurrency (spike cooking in Experimental, see TheNeedlerPattern)

## Doc Work
* Cookbook Additions
  * statefull list maybe
  * Scheduling: Non-overlapping bookings that do overlap, needs explanation
* Start working on the reference

### Ideas
* QuickPulse.Show turn manofwealth and taste into composable config
  => for reporting inputs : ShrinkingClue.Ignore property => ignore in QPS config => print full object

## Refactoring and Testing Targets 
* CaseSummary DepositionTests
* GetFromPool (bool, T?) => Result
* Nested InputShrinkingClue ? Refactoring Target
* Loose the lazy IEnumerable in shrinking (yield)
## Other
* Scour QuickAcid for any interesting tests left behind.


Pools are not exposed as raw sequences in the guide examples on purpose.
Treating pools as lists leads to scattered .Any() / .Count() checks that tend to become brittle during shrinking.
Instead, pool operations (When, These, ForThese, ToPool) encapsulate “is this state valid?” so shrinking can replay safely.

public readonly struct ActResult<T>
{
    private readonly bool isNone;
    private readonly T? value;

    public bool IsNone => isNone;
    public bool HasValue => !isNone;

    public T Value => !isNone ? value! : throw new ActionResultIsNone();

    public static implicit operator T(ActResult<T> r) => r.Value;
}


public static explicit operator T(ActResult<T> r) => r.Value;

    But rename the exception to something that encodes intent:

ActionValueUnavailable or ActionWasNotExecuted
(IsNone reads like “Option.None”, which invites people to assume monadic semantics.)

In that catch, consider a single docket breadcrumb(even if only under an Investigation flag) :

It’ll help you later when reading shrunk traces: “why didn’t this spec fire?”

