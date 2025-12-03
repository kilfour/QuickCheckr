namespace QuickCheckr.UnderTheHood.Proceedings;

public record PassedSpecDeposition
{
    public string Label { get; }
    public int TimesPassed { get; }

    public PassedSpecDeposition(string passedSpec, int timesPassed)
    {
        Label = passedSpec;
        TimesPassed = timesPassed;
    }
};