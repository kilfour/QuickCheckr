using QuickCheckr.Tests.Tools;
using QuickCheckr.Tests.Tools.ThePress;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.B_Collections;

[DocFile]
public class Collections
{
    // [Fact]
    // public void Collection_nested_shrink()
    // {
    //     var check =
    //         from input in "input".Input(Fuzzr.Constant<IEnumerable<List<int>>>([[42]]))
    //         from act in "act".Act(() => { })
    //         from spec in "spec".SpecIf(
    //             () => input.Any() && input.First().Count != 0,
    //             () => input.First().First() != 42)
    //         select Case.Closed;
    //     var article = TheJournalist.Exposes(() => QState.Run(script)
    //         .WithOneRun()
    //         .And(15.ExecutionsPerRun()));

    //     var entry = article.Execution(1).Input(1).Read();
    //     Assert.Equal("[ [ 42 ] ]", entry.Value);
    // }

    // [Fact]
    // public void Collection_shrink_with_extra()
    // {
    //     var check =

    //     var article = TheJournalist.Exposes(() => QState.Run(script)
    //         .WithOneRun()
    //         .And(15.ExecutionsPerRun()));

    //     var entry = article.Execution(1).Input(1).Read();
    //     Assert.Equal("[ _, 42 ]", entry.Value);
    // }


    // [Fact]
    // public void Collection_shrink_with_haha()
    // {
    //     var check =
    //         from input in "input".Input(Fuzzr.Constant<IEnumerable<int>>([1, 2, 1]))
    //         from act in "act".Act(() => { })
    //         from spec in "spec".SpecIf(() => input.Count() > 2, () => !input.Contains(1))
    //         select Case.Closed;

    //     var article = TheJournalist.Exposes(() => QState.Run(script)
    //         .WithOneRun()
    //         .And(15.ExecutionsPerRun()));

    //     var entry = article.Execution(1).Input(1).Read();
    //     Assert.Equal("[ _, _, 1 ]", entry.Value);
    // }

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