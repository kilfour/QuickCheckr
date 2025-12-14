using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood;

namespace QuickCheckr.Tests.Docs.B_PuttingACaseOnTheBoard.C_Configuration.B_TheCustodian;

public class InMemoryCustodian
{
    public Dictionary<string, InMemoryRecorder> Data = [];
    public Custodian Build() =>
        new()
        {
            Recorder = fn =>
            {
                if (!Data.ContainsKey(fn))
                    Data[fn] = new InMemoryRecorder();
                return Data[fn];
            },
            Retriever = fn =>
                Data.TryGetValue(fn, out InMemoryRecorder? value)
                    ? value.Values : []
        };
}

public class InMemoryRecorder : IEvidenceRecorder
{
    public List<string> Values { get; } = [];
    public void Append(string evidence) => Values.Add(evidence);
    public void Clear() => Values.Clear();
}