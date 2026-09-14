using QuickCheckr;
using QuickCheckr.UnderTheHood;

namespace QuickTestr.Bolts.Builders.ModelBased;

/// <summary>
/// Configures failure traces for model-based observations.
/// Use when you want to show model and system values that explain a failing comparison.
/// </summary>
public interface ITracer<T, U>
{
    /// <summary>
    /// Traces the full model and system state.
    /// Use when the entire pair of states is useful diagnostic output.
    /// </summary>
    Tracer<T, U> Trace();

    /// <summary>
    /// Traces projected values from the model and system state.
    /// Use when only selected values should appear in the failure report.
    /// </summary>
    Tracer<T, U> Trace<V, W>(Func<T, U, (V, W)> projector);
}

/// <summary>
/// Builds failure traces for model-based observations.
/// Use when defining what evidence should be emitted when an observation fails.
/// </summary>
public class Tracer<T, U> : ITracer<T, U>
{
    /// <summary>
    /// Gets the trace checkr built from the current tracer configuration.
    /// Use when turning a configured tracer into observation diagnostics.
    /// </summary>
    public Func<T, U, CheckrOf<Case>> TraceCheckr { get; private set; } = (m, s) => s => CheckrResult.CaseOnly(s);

    /// <summary>
    /// Traces the full model and system state.
    /// Use when the entire pair of states is useful diagnostic output.
    /// </summary>
    public Tracer<T, U> Trace() => Trace((m, s) => (m, s));

    /// <summary>
    /// Traces projected values from the model and system state.
    /// Use when only selected values should appear in the failure report.
    /// </summary>
    public Tracer<T, U> Trace<V, W>(Func<T, U, (V, W)> projector)
    {
        TraceCheckr =
            (m, s) =>
            {
                var result = projector(m, s);
                return
                    from modelTrace in Checkr.Trace("Model:", () => result.Item1)
                    from sutTrace in Checkr.Trace("Sut:  ", () => result.Item2)
                    select Case.Closed;
            };
        return this;
    }
}
