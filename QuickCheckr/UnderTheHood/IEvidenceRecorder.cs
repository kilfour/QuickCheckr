namespace QuickCheckr.UnderTheHood;

public interface IEvidenceRecorder
{
    void Append(string evidence);
    void Clear();
}

