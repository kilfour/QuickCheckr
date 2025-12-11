using QuickCheckr.UnderTheHood.Memories;
using QuickCheckr.UnderTheHood.Memories.Structure;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

namespace QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;

public static class Compile
{
    public static CaseFile PassedSpecs(IReadOnlyDictionary<string, int> passedSpecs)
    {
        var caseFile = CaseFile.Empty();
        AddPassedSpecs(passedSpecs, caseFile);
        return caseFile;
    }

    public static CaseFile TheCaseFile(Dossier dossier, Memory memory)
    {
        var verdict = Verdict.FromDossier(dossier);
        var caseFile = FromMemoryAndVerdict(memory, verdict);
        AddPassedSpecs(dossier.PassedSpecs, caseFile);
        return caseFile;
    }

    private static CaseFile FromMemoryAndVerdict(Memory memory, Verdict verdict)
    {
        AddExecutionDepositions(memory, verdict);
        var caseFile = CaseFile.WithVerdict(verdict);
        return caseFile;
    }

    private static void AddPassedSpecs(IReadOnlyDictionary<string, int> passedSpecs, CaseFile caseFile)
    {
        foreach (var passedSpec in passedSpecs)
            caseFile.AddPassedSpecDeposition(new PassedSpecDeposition(passedSpec.Key, passedSpec.Value));
    }

    private static void AddExecutionDepositions(Memory memory, Verdict verdict)
    {
        foreach (var (executionId, shortTermMemory) in memory.Entries())
        {
            verdict.AddExecutionDeposition(GetExecutionDeposition(shortTermMemory, executionId));
        }
    }

    private static ExecutionDeposition GetExecutionDeposition(
        ExecutionMemory shortTermMemory, int executionNumber)
    {
        var executionDeposition = new ExecutionDeposition(executionNumber);
        GetActionDepositions(executionDeposition, shortTermMemory);
        GetInputDepositions(executionDeposition, shortTermMemory);
        GetTraceDepositions(executionDeposition, shortTermMemory);
        GetWarningDepositions(executionDeposition, shortTermMemory);
        return executionDeposition;
    }

    private static void GetActionDepositions(ExecutionDeposition executionDeposition, ExecutionMemory shortTermMemory)
    {
        foreach (var action in shortTermMemory.GetActions())
        {
            executionDeposition.AddActionDeposition(new ActionDeposition(action));
        }
    }

    private static void GetInputDepositions(
        ExecutionDeposition executionDeposition, ExecutionMemory shortTermMemory)
    {
        foreach (var (key, val) in shortTermMemory.GetInputShrinkOutcomes())
        {
            executionDeposition.AddInputDeposition(new InputDeposition(key, ((InputShrinkingClue)val!).Report!));
        }
    }

    private static void GetTraceDepositions(ExecutionDeposition executionDeposition, ExecutionMemory shortTermMemory)
    {
        foreach (var (key, val) in shortTermMemory.GetTraces())
        {
            executionDeposition.AddTraceDeposition(new TraceDeposition(key, (string)val!));
        }
    }

    private static void GetWarningDepositions(ExecutionDeposition executionDeposition, ExecutionMemory shortTermMemory)
    {
        foreach (var warning in shortTermMemory.GetWarnings())
        {
            executionDeposition.AddWarningDeposition(new WarningDeposition(warning));
        }
    }
}