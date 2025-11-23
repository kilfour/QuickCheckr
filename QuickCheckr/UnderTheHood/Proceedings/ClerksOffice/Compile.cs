using QuickCheckr.UnderTheHood.Memories;
using QuickPulse.Show;

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

    // public static RunDeposition TheRun(string label, Memory memory, List<int> ExecutionNumbers)
    // {
    //     var runDeposition = new RunDeposition(label);
    //     foreach (var executionNumber in ExecutionNumbers.ToList())
    //     {
    //         runDeposition.AddExecutionDeposition(GetExecutionDeposition(memory, executionNumber, false));
    //     }
    //     return runDeposition;
    // }

    private static ExecutionDeposition GetExecutionDeposition(
        ShortTermMemory shortTermMemory, int executionNumber, bool isVerdict)
    {
        var executionDeposition = new ExecutionDeposition(executionNumber);
        //GetStashedDepositions(memory, executionNumber, executionDeposition);
        //var access = shortTermMemory.AccessFor(executionNumber);
        GetActionDepositions(executionDeposition, shortTermMemory);
        GetInputDepositions(executionDeposition, shortTermMemory, isVerdict);
        GetTraceDepositions(executionDeposition, shortTermMemory);
        return executionDeposition;
    }

    // private static void GetStashedDepositions(Memory Memory, int executionNumber, ExecutionDeposition executionDeposition)
    // {
    //     if (Memory.ShashedValuesPerExecution().TryGetValue(executionNumber, out var snapshot))
    //     {
    //         foreach (var (key, val) in snapshot)
    //             executionDeposition.AddStashedDeposition(new StashedDeposition(key, val));
    //     }
    // }

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
            executionDeposition.AddInputDeposition(new InputDeposition(key, val!)); //Introduce.This(val!, false)));
        }
        // foreach (var (key, val) in access.GetAll())
        // {
        //     var shrinkOutcome = val.GetShrinkOutcome();
        //     if (!isVerdict)
        //     {
        //         if (val.ReportingIntent != ReportingIntent.Never)
        //             executionDeposition.AddInputDeposition(new InputDeposition(key, Introduce.This(val.Value!, false)));
        //     }
        //     else if (shrinkOutcome is ShrinkOutcome.ReportedOutcome(var msg))
        //     {
        //         executionDeposition.AddInputDeposition(new InputDeposition(key, msg));
        //     }
        // }
    }

    private static void GetTraceDepositions(ExecutionDeposition executionDeposition, ShortTermMemory shortTermMemory)
    {
        foreach (var (key, val) in shortTermMemory.GetTraces())
        {
            executionDeposition.AddTraceDeposition(new TraceDeposition(key, (string)val!));
        }
    }
}