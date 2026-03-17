using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickPulse.Explains;
using QuickPulse.Show;

namespace QuickCheckr.Tests.Experimental.Z_Challenges.D_Calculator;

//[DocFile]
[DocContent(
@"

")]
public class C_Calculator : CheckrTest<C_Calculator>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact(Skip = "wip")]
    [DocCheckrHeader]
    [DocCheckr]
    [DocBoldHeader("The Reducer")]
    [DocExample(typeof(C_Calculator), nameof(Prune))]
    [DocReportHeader]
    [DocReport]
    public override void Example() =>
        Run(1000.Runs());

    [CodeSnippet]
    [CodeRemove("CalculatorFuzzr.")]
    protected override CheckrOf<Case> TheCheckr() =>
        from show in Showr.ForAll(() => Please.AllowMe().ToReplace<Term>(a => a.ToString()))
        from input in Checkr.Input("Calculator", CalculatorFuzzr.TheFuzzr,
            Shrink.OnType<Expr>(
                s => s.Classify(Try.Keep<Expr>()),
                s => s.Simplify(Reduce.Function<Expr>(a => Prune(a)))),
            Shrink.OnType<int>(s => s.Simplify(Reduce.Towards(1))))
        from eval in Checkr.Act("Evaluate", () => input.Evaluate())
        select Case.Closed;

    // public static IEnumerable<Expr> Prune(Expr expression)
    // {
    //     if (expression is Term term)
    //     {
    //         yield return term.Left;
    //         yield return term.Right;
    //         foreach (var item in Prune(term.Left))
    //             yield return item;
    //         foreach (var item in Prune(term.Right))
    //             yield return item;
    //     }
    // }

    [CodeExample]
    public static IEnumerable<Expr> Prune(Expr expression)
    {
        var queue = new Queue<Expr>();
        queue.Enqueue(expression);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current is Term term)
            {
                yield return term.L;
                yield return term.R;

                queue.Enqueue(term.L);
                queue.Enqueue(term.R);
            }
        }
    }

    protected override void Verify(Article article)
    {

    }
}

