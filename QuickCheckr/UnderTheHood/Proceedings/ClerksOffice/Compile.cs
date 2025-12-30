using QuickCheckr.UnderTheHood.Memories;
using QuickCheckr.UnderTheHood.Memories.Structure;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;

namespace QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;

public static class Compile
{
    public static CaseFile PassedExpectations(IReadOnlyDictionary<string, int> passedExpectations)
    {
        var caseFile = CaseFile.Empty();
        AddPassedExpectations(passedExpectations, caseFile);
        return caseFile;
    }

    public static CaseFile TheCaseFile(Dossier dossier, Memory memory)
    {
        var verdict = Verdict.FromDossier(dossier);
        var caseFile = FromMemoryAndVerdict(memory, verdict, dossier.UseMemoryForInputReporting, dossier.ReportWarnings);
        AddPassedExpectations(dossier.PassedExpectations, caseFile);
        return caseFile;
    }

    private static CaseFile FromMemoryAndVerdict(
        Memory memory,
        Verdict verdict,
        bool useMemoryForInputReporting,
        bool reportWarnings)
    {
        AddExecutionDepositions(memory, verdict, useMemoryForInputReporting, reportWarnings);
        var caseFile = CaseFile.WithVerdict(verdict);
        return caseFile;
    }

    private static void AddPassedExpectations(IReadOnlyDictionary<string, int> passedExpectations, CaseFile caseFile)
    {
        foreach (var passedExpectation in passedExpectations)
            caseFile.AddPassedExpectationDeposition(new PassedExpectationDeposition(passedExpectation.Key, passedExpectation.Value));
    }

    private static void AddExecutionDepositions(
        Memory memory,
        Verdict verdict,
        bool useMemoryForInputReporting,
        bool reportWarnings)
    {
        foreach (var (executionId, shortTermMemory) in memory.Entries())
        {
            verdict.AddExecutionDeposition(
                GetExecutionDeposition(shortTermMemory, executionId, useMemoryForInputReporting, reportWarnings));
        }
    }

    private static ExecutionDeposition GetExecutionDeposition(
        ExecutionMemory shortTermMemory, int executionNumber, bool useMemoryForInputReporting, bool reportWarnings)
    {
        var executionDeposition = new ExecutionDeposition(executionNumber);
        GetActionDepositions(executionDeposition, shortTermMemory);
        GetInputDepositions(executionDeposition, shortTermMemory, useMemoryForInputReporting);
        GetPoolTraceDepositions(executionDeposition, shortTermMemory);
        GetTraceDepositions(executionDeposition, shortTermMemory);
        if (reportWarnings)
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
        ExecutionDeposition executionDeposition,
        ExecutionMemory executionMemory,
        bool useMemoryForInputReporting)
    {
        if (useMemoryForInputReporting)
        {
            foreach (var (key, val) in executionMemory.GetInputValues())
            {

                executionDeposition.AddInputDeposition(new InputDeposition(key, val!));
            }
            return;
        }
        foreach (var (key, val) in executionMemory.GetInputShrinkOutcomes())
        {
            var showr = executionMemory.GetOriginalForKey(key);
            var original = executionMemory.GetOriginalForKey(key);
            // showr != null
            //     ? showr().IntroduceThis(((InputShrinkingClue)val!).Original!)
            //     : null;
            executionDeposition.AddInputDeposition(new InputDeposition(key, ((InputShrinkingClue)val!).Report!, original));
        }
    }

    private static void GetPoolTraceDepositions(ExecutionDeposition executionDeposition, ExecutionMemory shortTermMemory)
    {
        foreach (var (key, val) in shortTermMemory.GetPoolTraces())
        {
            executionDeposition.AddPoolTraceDeposition(new PoolTraceDeposition(key, (string)val!));
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