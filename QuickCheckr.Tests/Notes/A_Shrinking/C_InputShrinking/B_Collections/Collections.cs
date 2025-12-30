using QuickCheckr.Tests.Tools.ThePress;
using QuickFuzzr;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Notes.A_Shrinking.C_InputShrinking.B_Collections;

[DocFile]
public class Collections
{

    public class Composed
    {
        public int One { get; set; }
        public int Two { get; set; }
    }

    [Fact]
    public void Collection_shrink_with_hahaha()
    {
        var check =
            from input in Checkr.Input("input", Fuzzr.Constant<IEnumerable<Composed>>(
                [
                    new Composed() { One = 42, Two = 0 },
                    new Composed() { One = 42, Two = 0 }
                ]))
            //from act in Checkr.When(() => input.Any(), Checkr.Act("act", () => input.Any(a => a.One == 42)))
            from act in Checkr.ActWhen("act", () => input.Any(), () => input.Any(a => a.One == 42))
            from expect in Checkr.Expect("expect", () => !act.Value)
            select Case.Closed;
        var article = TheJournalist.Exposes(() => check.Run());

        Assert.Equal(1, article.Total().Inputs());
        var entry = article.Execution(1).Input(1).Read();
        Assert.Equal("[ { One: 42 } ]", entry.Value);
    }
}