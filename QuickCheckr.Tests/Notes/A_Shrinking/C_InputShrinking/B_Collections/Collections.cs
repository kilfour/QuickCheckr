using QuickPulse.Explains;

namespace QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.B_Collections;

[DocFile]
public class Collections
{

    // public class Composed
    // {
    //     public int One { get; set; }
    //     public int Two { get; set; }
    // }

    // [Fact]
    // public void Collection_shrink_with_hahaha()
    // {
    //     var check =
    //         from input in "input".Input(Fuzzr.Constant<IEnumerable<Composed>>(
    //             [new Composed() { One = 42, Two = 0 }, new Composed() { One = 42, Two = 0 }]))
    //         from act in "act".ActIf(() => input.Count() > 0, () => input.Any(a => a.One == 42))
    //         from spec in "spec".Spec(() => !act)
    //         select Case.Closed;
    //     var article = TheJournalist.Exposes(() => QState.Run(script)
    //         .WithOneRun()
    //         .And(15.ExecutionsPerRun()));

    //     Assert.Equal(1, article.Total().Inputs());
    //     var entry = article.Execution(1).Input(1).Read();
    //     Assert.Equal("[ { One : 42 } ]", entry.Value);
    // }
}