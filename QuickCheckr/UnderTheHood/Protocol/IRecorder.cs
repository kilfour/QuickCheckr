namespace QuickCheckr.UnderTheHood.Protocol;

public interface IRecorder
{
    void Append(string evidence);
    void Clear();
}

