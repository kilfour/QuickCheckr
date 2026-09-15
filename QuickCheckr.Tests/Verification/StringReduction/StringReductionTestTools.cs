using QuickCheckr.StringReduction;
using QuickCheckr.StringReduction.Nuts;
using QuickCheckr.Tests.Tools;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;

namespace QuickCheckr.Tests.Verification.StringReduction;

internal static class StringReductionTestTools
{
    public static string RunAll(SelectrOf<Selection> selector, string input, int maximumCandidates = 100)
    {
        var result = input;
        var run = selector.Run(input);
        var candidateCount = 0;

        while (!run.EndOfText)
        {
            if (candidateCount++ >= maximumCandidates)
                throw new InvalidOperationException("The selector did not reach the end of the text.");

            result = run.Next();
        }

        return result;
    }

    public static IReadOnlyList<string> RunCandidates(
        SelectrOf<Selection> selector,
        string input,
        int maximumCandidates = 100)
    {
        var candidates = new List<string>();
        var run = selector.Run(input);

        while (!run.EndOfText)
        {
            if (candidates.Count >= maximumCandidates)
                throw new InvalidOperationException("The selector did not reach the end of the text.");

            candidates.Add(run.Next());
        }

        return candidates;
    }

    public static ValueTask<Reduction> ReduceAsync(
        SelectrOf<Selection> selector,
        string input,
        Func<object, bool> failureStillPresent) =>
        TestReductionRunner.ForReducerAsync(selector.Reduce(), input, failureStillPresent);
}
