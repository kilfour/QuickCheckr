

namespace QuickCheckr.UnderTheHood.Proceedings;

public record PassedSpecsDeposition
{
    public List<PassedSpecDeposition> PassedSpecDepositions { get; } = [];
    public PassedSpecsDeposition AddPassedSpecDeposition(PassedSpecDeposition passedSpecDeposition)
    {
        PassedSpecDepositions.Add(passedSpecDeposition);
        return this;
    }
}
