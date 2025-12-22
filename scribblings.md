* play more pool
  * use a pool operation, but make the checkr used by Trackr.OneOfPool throw, see what happens 
    (pool mutated, SUT not, probably)
    ```csharp
    var outcome = checkr(value!)(state).Value;
    if (operations is not null)
        operations(new TrackrPoolOperations<T>(state, id));
    return CheckrResult.Some(state, outcome);
    ```
    better, but semantics
    ```csharp
    Func<TrackrPoolOperations<T>, CheckrOf<Case>>? operations
    ```
    also semantics I think, LINQ machinery is unaware of exception handling
    So yeah, I need a `[Fact]`
    Might not be an issue at all, as I think it does not need to be *atomic*
    The pool is a verification of the SUT, so if the sut fails, were gonna use the pool to verify
    Making it atomic => no way to verify
  * SOLVED by encapsulating the pool element returned