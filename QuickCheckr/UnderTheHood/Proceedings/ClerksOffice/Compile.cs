using QuickCheckr.UnderTheHood.Memories;
using QuickCheckr.UnderTheHood.Memories.Structure;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

namespace QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;

public static class Compile
{
    public static CaseFile TheCaseFile(Dossier dossier, Memory memory)
    {
        var verdict = Verdict.FromDossier(dossier);
        var caseFile = FromMemoryAndVerdict(memory, verdict);
        AddPassedSpecs(dossier, caseFile);
        return caseFile;
    }

    private static CaseFile FromMemoryAndVerdict(Memory memory, Verdict verdict)
    {
        AddExecutionDepositions(memory, verdict);
        AddCodeDepositions(memory, verdict);
        var caseFile = CaseFile.WithVerdict(verdict);
        return caseFile;
    }

    private static void AddCodeDepositions(Memory memory, Verdict verdict)
    {
        foreach (var code in memory.GetCodeArrangeValues())
        {
            verdict.AddCodeDeposition(new CodeDeposition((string)code!));
        }
        foreach (var (_, executionMemory) in memory.Entries())
        {
            GetCodeDepositions(verdict, executionMemory);
        }
        foreach (var code in memory.GetCodeAssertValues())
        {
            verdict.AddCodeDeposition(new CodeDeposition((string)code!));
        }
    }

    private static void GetCodeDepositions(Verdict verdict, ExecutionMemory executionMemory)
    {
        foreach (var code in executionMemory.GetCodeActValues())
        {
            verdict.AddCodeDeposition(new CodeDeposition((string)code!));
        }
    }

    private static void AddPassedSpecs(Dossier dossier, CaseFile caseFile)
    {
        foreach (var passedSpec in dossier.PassedSpecs)
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
}