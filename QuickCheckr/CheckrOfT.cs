using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

/// <summary>
/// Represents a single step in a Checkr script.  
/// Each step receives the current state, performs work, and returns an updated result.  
/// Use to compose inputs, actions, expectations, and traces into a LINQ-driven testing workflow.
/// </summary>
public delegate CheckrResult<T> CheckrOf<T>(CheckrState state);


