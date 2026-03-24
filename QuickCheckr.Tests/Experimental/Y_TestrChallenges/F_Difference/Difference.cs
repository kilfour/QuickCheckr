using QuickCheckr.Tests.Tools;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Experimental.Y_TestrChallenges.F_Difference;

[DocFile]
[DocFuzzrHeader]
[DocExample(typeof(Difference), nameof(TheFuzzr))]
[DocBoldHeader("The Shrinkr")]
[DocExample(typeof(The))]
public class Difference : QCTest
{
    public record Pair(int A, int B);

    [CodeSnippet]
    public static readonly FuzzrOf<Pair> TheFuzzr =
        from _ in Configr<Pair>.Construct(Fuzzr.Int(), Fuzzr.Int())
        from pair in Fuzzr.One<Pair>()
        select pair;

    [CodeExample]
    public class The
    {
        public static Shrinker Shrinkr =
            Shrink.OnType<Pair>(
                s => s.Simplify(
                    Reduce.Function<Pair>(a => TowardsZero(a))));

        private static IEnumerable<Pair> TowardsZero(Pair input)
        {
            var local = input;
            while (local.A >= 0)
            {
                local = local with { A = local.A - 1, B = local.B - 1 };
                yield return local;
            }
        }
    }
}