

using QuickCheckr.UnderTheHood.Proceedings.Depositions;

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

    public List<PassedExpectationDeposition> PassedExpectationDepositions { get; init; } = [];
    public CaseFile AddPassedExpectationDeposition(PassedExpectationDeposition passedExpectationDepositions)
    {
        PassedExpectationDepositions.Add(passedExpectationDepositions);
        return this;
    }

    public int InputsNamed(string key, Func<string, int> scorer)
    {
        if (!HasVerdict()) return int.MaxValue;
        return Verdict!.ExecutionDepositions
            .SelectMany(a => a.InputDepositions)
            .Where(a => a.Label == key)
            .Select(a => a.GetReduxOrValue())
            .Sum(a => scorer(a));
    }
}