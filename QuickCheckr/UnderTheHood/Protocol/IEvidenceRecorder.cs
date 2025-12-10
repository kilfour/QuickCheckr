namespace QuickCheckr.UnderTheHood.Protocol;

public interface IEvidenceRecorder
{
    void Append(string evidence);
    void Clear();
}

