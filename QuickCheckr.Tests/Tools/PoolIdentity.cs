using QuickCheckr.UnderTheHood;

namespace QuickCheckr.Tests.Tools;

// Replay investigations inspect stored identity independently of the public value-only API.
internal static class PoolIdentity
{
    internal static CheckrOf<int> Of<T>(T value) =>
        state => CheckrResult.Some(state,
            state.Memory.GetPoolDictionary<T>()
                .Single(element => EqualityComparer<T>.Default.Equals(element.Value, value)).Key);
}