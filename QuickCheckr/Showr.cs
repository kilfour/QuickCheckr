using QuickCheckr.UnderTheHood;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.Formatters;
using QuickPulse.Show;
using QuickPulse.Show.Bolts;

namespace QuickCheckr;

public static partial class Showr
{
    public static CheckrOf<Case> ForClassification(Action<ClassificationFormatter> config) =>
        state =>
        {
            if (state.EvidenceRecorder.IsReplay)
                return CheckrResult.CaseOnly(state);
            config(state.ClassificationFormatter);
            return CheckrResult.CaseOnly(state);
        };

    public static CheckrOf<Case> ForTrace(Func<ManOfWealthAndTaste> config) =>
        state =>
        {
            state.Editor = config();
            return CheckrResult.CaseOnly(state);
        };

    public static CheckrOf<Case> ForInput() =>
        state =>
        {
            state.Memory.StoreInputShowr(() => Please.AllowMe().ToUseNonLinearTime());
            return CheckrResult.CaseOnly(state);
        };

    public static CheckrOf<Case> ForInput(Func<ManOfWealthAndTaste> config) =>
        state =>
        {
            state.Memory.StoreInputShowr(config);
            return CheckrResult.CaseOnly(state);
        };

    public static CheckrOf<Case> ForAll(Func<ManOfWealthAndTaste> config) =>
        state =>
        {
            state.Editor = config();
            state.Memory.StoreInputShowr(config);
            return CheckrResult.CaseOnly(state);
        };

    public static CheckrOf<Case> None() =>
        state =>
        {
            state.Editor = Please.AllowMe().ToUseNonLinearTime();
            state.Memory.StoreInputShowr(null!);
            return CheckrResult.CaseOnly(state);
        };
}