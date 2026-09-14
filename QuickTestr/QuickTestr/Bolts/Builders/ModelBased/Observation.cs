using QuickCheckr;

namespace QuickTestr.Bolts.Builders.ModelBased;

/// <summary>
/// Represents a state comparison used in model-based testing.
/// Use to capture an invariant that should hold between the model and the system under test.
/// </summary>
public record Observation<T, U>(Func<T, U, CheckrOf<Case>> Observe);

/// <summary>
/// Creates model-based observations from labeled predicates.
/// Use when you want to define reusable checks between the model and the system under test.
/// </summary>
public static class Observation
{
    /// <summary>
    /// Creates an observation from a labeled predicate and optional trace output.
    /// Use when a model-based assertion should emit extra evidence on failure.
    /// </summary>
    public static Observation<T, U> From<T, U>(string label, Func<T, U, bool> observe, Func<T, U, CheckrOf<Case>>? trace = null)
        => new((m, s) =>
        {
            if (trace is not null)
                return
                    from ok in Checkr.Capture(() => observe(m, s))
                    from tr in Checkr.When(() => !ok, trace(m, s))
                    from ex in Checkr.Expect(label, () => ok)
                    select Case.Closed;
            return Checkr.Expect(label, () => observe(m, s));
        });
}

/// <summary>
/// Represents a model-based operation and its accompanying trace.
/// Use when you need to package a state transition together with its diagnostic output.
/// </summary>
public record Operation<T, U>(Func<T, U, CheckrOf<Case>> Operate, Func<T, U, CheckrOf<Case>> Trace);
