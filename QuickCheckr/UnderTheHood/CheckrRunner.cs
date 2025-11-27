using QuickCheckr.UnderTheHood.Archiving;
using QuickCheckr.UnderTheHood.Proceedings;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickCheckr.UnderTheHood.Protocol;
using QuickCheckr.UnderTheHood.Runners;
using QuickCheckr.UnderTheHood.Runners.ActionShrinking;
using QuickCheckr.UnderTheHood.Runners.ExecutionShrinking;
using QuickCheckr.UnderTheHood.Runners.InputReduction;
using QuickCheckr.UnderTheHood.Runners.InputShrinking;
using QuickPulse;
using QuickPulse.Arteries;

namespace QuickCheckr.UnderTheHood;

public class CheckrRunner(CheckrOf<Case> check, CheckrRunnerConfig config, int numberOfExecutions)
{
    public CheckrOf<Case> Check { get; init; } = check;
    public CheckrState State { get; init; } = new(numberOfExecutions, config.Signal, config.Seed);
    private readonly Vault vault =
        string.IsNullOrWhiteSpace(config.FileAs) ? new Vault() : new EvidenceVault(config.FileAs);

    public CaseFile Run()
    {
        new InitialRun(State, Check).Run(numberOfExecutions);
        if (State.EvidenceRecorder.Failed())
        {
            var caseFile = HandleFailure();
            vault.AddSeed(State.FuzzrState.Seed, numberOfExecutions);
            if (!string.IsNullOrWhiteSpace(config.FileAs))
            {
                var filenameCf = Path.Combine(".quickcheckr", config.FileAs, $"report.qr");
                Signal.From<string>(a => Pulse.Trace(a))
                    .SetArtery(FileLog.Write(filenameCf))
                    .Pulse(TheClerk.Transcribes(caseFile));
            }
            return caseFile;
        }
        return CaseFile.Empty();
    }

    private CaseFile HandleFailure()
    {
        var originalRunExecutionCount = State.Memory.Count();
        var shrinkCount = 0;
        if (!string.IsNullOrWhiteSpace(State.EvidenceRecorder.FailingAssay))
            return CompileTheCaseFile(originalRunExecutionCount, shrinkCount, true);
        if (!config.NoShrinking)
        {
            shrinkCount += new ExecutionShrinker(State, Check).Run();
            if (config.ShrinkingActions)
                shrinkCount += new ActionShrinker(State, Check).Run();
            shrinkCount += new InputShrinker(State, Check).Run();
            shrinkCount += new InputReducer(State, Check).Run();
        }
        return CompileTheCaseFile(originalRunExecutionCount, shrinkCount);
    }

    private CaseFile CompileTheCaseFile(
        int originalRunExecutionCount,
        int shrinkCount,
        bool forAssayer = false)
    {
        var dossier =
            new Dossier(
                AssayerSpec: forAssayer ? State.EvidenceRecorder.FailingAssay : null,
                FailingSpec: forAssayer ? null : State.EvidenceRecorder.FailingSpec,
                Exception: State.EvidenceRecorder.Exception,
                OriginalRunExecutionCount: originalRunExecutionCount,
                ExecutionCount: State.Memory.Count(),
                ShrinkCount: shrinkCount,
                Seed: State.FuzzrState.Seed,
                PassedSpecs: State.EvidenceRecorder.PassedSpecs
            );
        return Compile.TheCaseFile(dossier, State.Memory);
    }
}