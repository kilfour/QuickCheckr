using QuickCheckr.FilingCabinet;
using QuickCheckr.FilingCabinet.Depositions;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood.Memories;
using QuickCheckr.UnderTheHood.Memories.Structure;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;
using QuickPulse.Show.Bolts;

namespace QuickCheckr.UnderTheHood.Proceedings
{
internal static class Compile
    {
        public static CaseFile TheCaseFile(Dossier dossier, Memory memory, ManOfWealthAndTaste inputPrinter)
        {
            var caseFile = CaseFile.From(dossier.FailureInfo, dossier.RunInfo);
            foreach (var (executionId, shortTermMemory) in memory.Entries())
            {
                caseFile.AddExecutionDeposition(
                    GetExecutionDeposition(
                        shortTermMemory,
                        executionId,
                        dossier.UseMemoryForInputReporting,
                        inputPrinter,
                        dossier.ReportMode,
                        dossier.WarningLevel));
            }
            AddPassedExpectations(dossier.PassedExpectations, caseFile);
            return caseFile;
        }

        private static void AddPassedExpectations(IReadOnlyDictionary<string, int> passedExpectations, CaseFile caseFile)
        {
            foreach (var passedExpectation in passedExpectations)
                caseFile.AddPassedExpectationDeposition(new PassedExpectation(passedExpectation.Key, passedExpectation.Value));
        }

        private static ExecutionDeposition GetExecutionDeposition(
            ExecutionMemory shortTermMemory,
            int executionNumber,
            bool useMemoryForInputReporting,
            ManOfWealthAndTaste inputPrinter,
            ReportMode reportMode,
            WarningLevel warningLevel)
        {
            var executionDeposition = new ExecutionDeposition(executionNumber);
            GetActionDepositions(executionDeposition, shortTermMemory);
            GetInputDepositions(executionDeposition, shortTermMemory, useMemoryForInputReporting, inputPrinter);
            GetPoolTraceDepositions(executionDeposition, shortTermMemory);
            GetTraceDepositions(executionDeposition, shortTermMemory);
            GetFinalTraceDepositions(executionDeposition, shortTermMemory);
            GetWarningDepositions(executionDeposition, shortTermMemory, warningLevel);
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
            bool useMemoryForInputReporting,
            ManOfWealthAndTaste inputPrinter)
        {

            if (useMemoryForInputReporting)
            {
                foreach (var (key, val) in executionMemory.GetInputValues())
                {
                    executionDeposition.AddInputDeposition(
                        new InputDeposition(key, inputPrinter.IntroduceThis(val.Value!))
                        {
                            InputValue = val.Value,
                            Type = val.Type
                        });
                }
                return;
            }

            foreach (var (key, classification) in executionMemory.GetInputShrinkingClassifications())
            {
                var relevant = classification as InputRelevant;
                if (relevant == null) continue;
                var input = executionMemory.GetInput(key);
                var report = relevant.Report;
                var (reduxReport, reduxValue) =
                        relevant.Redux.HasValue
                            ? (Maybe.Just(relevant.Redux.Value!.Report), Maybe<object>.Just(relevant.Redux.Value!.Value))
                            : (Maybe.Nothing(), Maybe<object>.Nothing());
                executionDeposition.AddInputDeposition(
                        new InputDeposition(key, report!)
                        {
                            Original = executionMemory.GetOriginalForKey(key),
                            Redux = reduxReport,
                            ReduxValue = reduxValue,
                            InputValue = input.Value,
                            Type = input.Type
                        });
            }
        }

        private static void GetPoolTraceDepositions(ExecutionDeposition executionDeposition, ExecutionMemory shortTermMemory)
        {
            foreach (var (key, val) in shortTermMemory.GetPoolTraces())
            {
                executionDeposition.AddPoolTraceDeposition(new PoolTraceDeposition(key, val));
            }
        }

        private static void GetTraceDepositions(ExecutionDeposition executionDeposition, ExecutionMemory shortTermMemory)
        {
            foreach (var (key, val) in shortTermMemory.GetTraces())
            {
                executionDeposition.AddTraceDeposition(new TraceDeposition(key, val));
            }
        }

        private static void GetFinalTraceDepositions(ExecutionDeposition executionDeposition, ExecutionMemory shortTermMemory)
        {
            foreach (var (key, val) in shortTermMemory.GetFinalTraces())
            {
                executionDeposition.AddFinalTraceDeposition(new TraceDeposition(key, val));
            }
        }

        private static void GetWarningDepositions(ExecutionDeposition executionDeposition, ExecutionMemory shortTermMemory, WarningLevel warningLevel)
        {
            foreach (var warning in shortTermMemory.GetWarnings().Where(a => a.WarningLevel <= warningLevel))
            {
                executionDeposition.AddWarningDeposition(new WarningDeposition(warning));
            }
        }
    }
}
