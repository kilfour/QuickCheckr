using QuickCheckr.UnderTheHood;

namespace QuickCheckr;

public static partial class Trackr
{
    public static CheckrOf<Case> Assay(string key, Func<bool> condition) =>
        state =>
        {
            state.Memory.SpecGuard.Check(key);
            if (!state.IsThisTheRunsLastExecution())
                return CheckrResult.CaseOnly(state);
            bool passed = condition();
            if (!passed)
            {
                state.EvidenceRecorder.AsssayFailed(key);
            }
            else
            {
                state.EvidenceRecorder.SpecPassed(key);
            }
            return CheckrResult.CaseOnly(state);
        };
    public static CheckrOf<Case> Assay(string key, params (string label, Func<bool> condition)[] conditions) =>
        state =>
        {
            if (!state.IsThisTheRunsLastExecution())
                return CheckrResult.CaseOnly(state);
            bool passed = true;
            var strings = new List<string>();
            foreach (var (label, condition) in conditions)
            {
                if (!condition())
                {
                    strings.Add(label);
                    passed = false;
                }
            }
            if (!passed)
            {
                state.EvidenceRecorder.AsssayFailed(key + ": " + string.Join(", ", strings));
            }
            else
            {
                state.EvidenceRecorder.SpecPassed(key);
            }
            return CheckrResult.CaseOnly(state);
        };

    // public static QAcidScript<Acid> Assay(this string key, params (string label, Func<bool> condition)[] specs) =>
    // 	state =>
    // 		{
    // 			if (!state.IsThisTheRunsLastExecution())
    // 				return Vessel.AcidOnly(state);

    // 			bool passed = true;
    // 			var strings = new List<string>();
    // 			foreach (var (label, condition) in specs)
    // 			{
    // 				if (!condition())
    // 				{
    // 					strings.Add(label);
    // 					passed = false;
    // 				}
    // 			}
    // 			if (!passed)
    // 			{
    // 				state.AllowShrinking = false;
    // 				state.Shifter.CurrentContext.MarkFailure(string.Join(", ", strings));
    // 			}
    // 			else
    // 			{
    // 				state.SpecPassed(key);
    // 			}
    // 			return Vessel.AcidOnly(state);
    // 		};
}