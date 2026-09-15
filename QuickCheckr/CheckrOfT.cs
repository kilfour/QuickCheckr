using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

/// <summary>
/// Represents a single step in a Checkr script.  
/// Each step receives the current state and returns an awaitable updated result.
/// Use to compose inputs, actions, expectations, and traces into a LINQ-driven testing workflow.
/// </summary>
public delegate ValueTask<CheckrResult<T>> CheckrOf<T>(CheckrState state);


