using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood;

namespace QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.C_Configuration.B_TheCustodian;

public class InMemoryCustodian
{
    public Dictionary<string, InMemoryRecorder> Data = [];
    public List<string> StartingEvidence = [];
    public Custodian Build() =>
        new()
        {
            Recorder = fn =>
            {
                if (!Data.ContainsKey(fn))
                    Data[fn] = new InMemoryRecorder();
                return Data[fn];
            },
            Retriever = fn => StartingEvidence
        };
}

public class InMemoryRecorder : IEvidenceRecorder
{
    public bool Cleared;
    public List<string> Values = [];
    public void Append(string evidence) => Values.Add(evidence);
    public void Clear() { Cleared = true; Values.Clear(); }
}