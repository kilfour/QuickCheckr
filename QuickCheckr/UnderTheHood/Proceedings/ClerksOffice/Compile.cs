using QuickCheckr.UnderTheHood.Memories;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

namespace QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;

public static class Compile
{
    public static CaseFile TheCaseFile(Dossier dossier, ShortTermMemories shortTermMemories)// , Memory Memory)
    {
        var verdict = Verdict.FromDossier(dossier);
        foreach (var executionNumber in dossier.ExecutionNumbers.ToList())
        {
            verdict.AddExecutionDeposition(GetExecutionDeposition(shortTermMemories.GetMemory(executionNumber), executionNumber, true));
        }
        return CaseFile.WithVerdict(verdict);
    }

    private static ExecutionDeposition GetExecutionDeposition(
        ShortTermMemory shortTermMemory, int executionNumber, bool isVerdict)
    {
        var executionDeposition = new ExecutionDeposition(executionNumber);
        GetActionDepositions(executionDeposition, shortTermMemory);
        GetInputDepositions(executionDeposition, shortTermMemory, isVerdict);
        GetTraceDepositions(executionDeposition, shortTermMemory);
        return executionDeposition;
    }

    private static void GetActionDepositions(ExecutionDeposition executionDeposition, ShortTermMemory shortTermMemory)//, Access access)
    {
        foreach (var action in shortTermMemory.GetActions())
        {
            executionDeposition.AddActionDeposition(new ActionDeposition(action));
        }
    }

    private static void GetInputDepositions(
        ExecutionDeposition executionDeposition, ShortTermMemory shortTermMemory, bool isVerdict)
    {
        foreach (var (key, val) in shortTermMemory.GetInputShrinkOutcomes())
        {
            executionDeposition.AddInputDeposition(new InputDeposition(key, ((InputShrinkingClue)val!).Report!)); //Introduce.This(val!, false)));
        }
    }

    private static void GetTraceDepositions(ExecutionDeposition executionDeposition, ShortTermMemory shortTermMemory)
    {
        foreach (var (key, val) in shortTermMemory.GetTraces())
        {
            executionDeposition.AddTraceDeposition(new TraceDeposition(key, (string)val!));
        }
    }
}