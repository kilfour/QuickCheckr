using QuickCheckr.Protocol;
using QuickCheckr.Diagnostics;
using QuickCheckr.FilingCabinet;
using QuickCheckr.Protocol.Custodians;

namespace QuickCheckr.UnderTheHood.Archiving;

internal sealed class VaultNotConfiguredException : Exception;
internal static class Vault
{
    public static EvidenceVault From(CheckrConfig config)
        => string.IsNullOrWhiteSpace(config.FileAs) ? throw new VaultNotConfiguredException() : new EvidenceVault(config);
}

internal sealed class EvidenceVault(CheckrConfig config)
{

    private HashSet<VaultEntry> entries = [];
    private readonly ICustodian custodian = config.Custodian;



    public IEnumerable<CaseFile> Check(CheckrOf<Case> check, CheckrConfig config, bool closeResolved)
        => CheckAsync(check, config, closeResolved).GetAwaiter().GetResult();

    public async Task<IEnumerable<CaseFile>> CheckAsync(CheckrOf<Case> check, CheckrConfig config, bool closeResolved)
    {
        config.CancellationToken.ThrowIfCancellationRequested();
        entries = custodian.RetrieveVaultEntries(config.FileAs);
        var caseFiles = new List<CaseFile>();
        foreach (var entry in entries)
        {
            config.CancellationToken.ThrowIfCancellationRequested();
            var caseFile = await new CheckrRunner(check, config with { Seed = entry.Seed }, entry.ExecutionsPerRun, AutopsyProbe.Empty).RunAsync().ConfigureAwait(false);
            config.CancellationToken.ThrowIfCancellationRequested();
            caseFiles.Add(caseFile);
            if (caseFile.HasEvidence)
            {
                if (!string.IsNullOrWhiteSpace(config.FileAs))
                {
                    custodian.FileEvidence(config.FileAs, entry.Seed, caseFile, config.Clerk.File);
                }
            }
            else
            {
                if (closeResolved)
                {
                    Remove(entry);
                    custodian.RemoveEvidence(config.FileAs, entry.Seed);
                }
                else
                {
                    custodian.FileEvidence(config.FileAs, entry.Seed, caseFile, config.Clerk.File);
                }
            }
        }
        config.CancellationToken.ThrowIfCancellationRequested();
        custodian.StoreVaultEntries(config.FileAs, entries);
        return caseFiles;
    }

    public void AddSeed(int seed, int numberOfExecutions)
    {
        Add(new(seed, numberOfExecutions));
    }

    private void Add(VaultEntry vaultEntry)
    {
        entries.Add(vaultEntry);
        custodian.StoreVaultEntries(config.FileAs, entries);
    }

    private void Remove(VaultEntry vaultEntry)
    {
        entries.Remove(vaultEntry);
        custodian.StoreVaultEntries(config.FileAs, entries);
    }
}
