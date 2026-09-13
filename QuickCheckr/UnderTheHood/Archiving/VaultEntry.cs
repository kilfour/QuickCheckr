namespace QuickCheckr.UnderTheHood.Archiving;

/// <summary>
/// Identifies a persisted failing run by its seed and execution count.
/// </summary>
public sealed record VaultEntry(int Seed, int ExecutionsPerRun);
