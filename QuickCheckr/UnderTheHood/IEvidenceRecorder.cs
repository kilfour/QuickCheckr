namespace QuickCheckr.UnderTheHood;

internal interface IEvidenceRecorder
{
    void Append(string evidence);
    void Clear();
}

