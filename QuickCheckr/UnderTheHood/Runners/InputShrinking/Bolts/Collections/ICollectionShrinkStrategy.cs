namespace QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Collections;

public interface ICollectionShrinkStrategy
{
    void Shrink<T>(CheckrState state, CheckrOf<Case> check, int executionId, string key, T value, string fullKey);
}
