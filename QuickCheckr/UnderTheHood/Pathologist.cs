using QuickPulse.Show;
using QuickPulse.Arteries;

namespace QuickCheckr.UnderTheHood;

public class Pathologist
{
    public static Pathologist FromArtery(IArtery? artery) =>
        artery is null ? new() : new() { IsWatching = true, artery = artery };

    public bool IsWatching { get; private set; }
    public void Examine<T>(object? obj) => Examine(typeof(T).Name, obj);
    public void Examine(string source, object? obj) => artery?.Absorb(new Observation(source, Introduce.This(obj!)));
    private record Observation(string Source, object? Value);

    private Pathologist() { }
    private IArtery? artery;
}



