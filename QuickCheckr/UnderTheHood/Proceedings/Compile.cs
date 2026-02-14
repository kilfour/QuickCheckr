using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood.Memories;
using QuickCheckr.UnderTheHood.Memories.Structure;
using QuickCheckr.UnderTheHood.Proceedings.Depositions;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Classification.Records;
using QuickPulse.Show.Bolts;

namespace QuickCheckr.UnderTheHood.Proceedings.ClerksOffice
{

    public static class Compile
    {
        public static CaseFile PassedExpectations(IReadOnlyDictionary<string, int> passedExpectations)
        {
            var caseFile = CaseFile.Empty();
            AddPassedExpectations(passedExpectations, caseFile);
            return caseFile;
        }

        public static CaseFile TheCaseFile(Dossier dossier, Memory memory, ManOfWealthAndTaste inputPrinter)
        {
            var caseFile = CaseFile.From(dossier.FailureInfo, dossier.RunInfo);
            foreach (var (executionId, shortTermMemory) in memory.Entries())
            {
                caseFile.AddExecutionDeposition(
                    GetExecutionDeposition(shortTermMemory, executionId, dossier.UseMemoryForInputReporting, inputPrinter, dossier.ReportMode));
            }
            AddPassedExpectations(dossier.PassedExpectations, caseFile);
            return caseFile;
        }

        private static void AddPassedExpectations(IReadOnlyDictionary<string, int> passedExpectations, CaseFile caseFile)
        {
            foreach (var passedExpectation in passedExpectations)
                caseFile.AddPassedExpectationDeposition(new PassedExpectationDeposition(passedExpectation.Key, passedExpectation.Value));
        }

        private static ExecutionDeposition GetExecutionDeposition(
            ExecutionMemory shortTermMemory,
            int executionNumber,
            bool useMemoryForInputReporting,
            ManOfWealthAndTaste inputPrinter,
            ReportMode reportMode)
        {
            var executionDeposition = new ExecutionDeposition(executionNumber);
            GetActionDepositions(executionDeposition, shortTermMemory);
            GetInputDepositions(executionDeposition, shortTermMemory, useMemoryForInputReporting, inputPrinter, reportMode.HasFlag(ReportMode.Labels));
            GetPoolTraceDepositions(executionDeposition, shortTermMemory, reportMode.HasFlag(ReportMode.Labels));
            GetTraceDepositions(executionDeposition, shortTermMemory, reportMode.HasFlag(ReportMode.Labels));
            if (reportMode.HasFlag(ReportMode.Warning))
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
            bool useMemoryForInputReporting,
            ManOfWealthAndTaste inputPrinter,
            bool labeled)
        {

            if (useMemoryForInputReporting)
            {
                foreach (var (key, val) in executionMemory.GetInputValues())
                {
                    executionDeposition.AddInputDeposition(
                        new InputDeposition(labeled, key, inputPrinter.IntroduceThis(val.Value!))
                        {
                            InputValue = val.Value,
                            Type = val.Type
                        });
                }
                return;
            }

            // foreach (var (key, clue) in executionMemory.GetInputShrinkOutcomes())
            // {
            //     var input = executionMemory.GetInput(key);
            //     var redux = executionMemory.GetInputReduxForKey(key);
            //     executionDeposition.AddInputDeposition(
            //             new InputDeposition(labeled, key, clue!.Report!)
            //             {
            //                 Original = executionMemory.GetOriginalForKey(key),
            //                 Redux = redux.HasValue ? Maybe.Just(inputPrinter.IntroduceThis(redux.Value)) : Maybe.Nothing(),
            //                 ReduxValue = redux,
            //                 InputValue = input.Value,
            //                 Type = input.Type
            //             });
            // }

            foreach (var (key, classification) in executionMemory.GetInputShrinkingClassifications())
            {
                var relevant = classification as InputRelevant;
                if (relevant == null) continue;
                var input = executionMemory.GetInput(key);
                var redux = executionMemory.GetInputReduxForKey(key);
                var report = relevant.Report;
                executionDeposition.AddInputDeposition(
                        new InputDeposition(labeled, key, report!)
                        {
                            Original = executionMemory.GetOriginalForKey(key),
                            Redux = classification.Redux,
                            ReduxValue = classification.ReduxValue,
                            InputValue = input.Value,
                            Type = input.Type
                        });
            }
        }

        private static void GetPoolTraceDepositions(ExecutionDeposition executionDeposition, ExecutionMemory shortTermMemory, bool labeled)
        {
            foreach (var (key, val) in shortTermMemory.GetPoolTraces())
            {
                executionDeposition.AddPoolTraceDeposition(new PoolTraceDeposition(key, val, labeled));
            }
        }

        private static void GetTraceDepositions(ExecutionDeposition executionDeposition, ExecutionMemory shortTermMemory, bool labeled)
        {
            foreach (var (key, val) in shortTermMemory.GetTraces())
            {
                executionDeposition.AddTraceDeposition(new TraceDeposition(key, val, labeled));
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
}