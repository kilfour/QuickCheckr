namespace QuickCheckr.UnderTheHood.Proceedings;

public class CaseFile
{
    protected CaseFile(Verdict verdict) { Verdict = verdict; }

    public static CaseFile Empty()
    {
        return new CaseFile(null!);
    }

    public Verdict? Verdict { get; private set; }
    public bool HasVerdict() { return Verdict != null; }
    public static CaseFile WithVerdict(Verdict verdict)
    {
        return new CaseFile(verdict);
    }

    public List<PassedSpecDeposition> PassedSpecDepositions { get; init; } = [];
    public CaseFile AddPassedSpecDeposition(PassedSpecDeposition passedSpecDeposition)
    {
        PassedSpecDepositions.Add(passedSpecDeposition);
        return this;
    }
}