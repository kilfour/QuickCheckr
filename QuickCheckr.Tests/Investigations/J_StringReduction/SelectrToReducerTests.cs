using QuickCheckr;
using QuickCheckr.StringReduction;
using QuickCheckr.StringReduction.Nuts;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;
using QuickCheckr.UnderTheHood.Warnings;

namespace QuickCheckr.Tests.Investigations.J_StringReduction;

public class SelectrToReducerTests
{
    public class WhistleblowerSpy : IReductionWhistleblower
    {
        public int MaxStepsPerReducer => 1024;
        public void Warn(string key, Func<Warning> factory) { }
    }

    private readonly WhistleblowerSpy whistleblower = new();

    private Reduction Act(Func<Reducer<string>> reducerFactory, string input, Func<object?, bool>? failTest = null)
    {
        var reducer = new TypeReducerOption<string>(reducerFactory).ReducerFactory();
        var predicate = failTest ?? (a => true);
        var result = ReductionRunner.ForReducer(reducer, "", "", input, predicate, whistleblower);
        return result;
    }

    [Fact]
    public void FirstTry()
    {
        var selectr =
            from a_b in Stringr.Once('a', 'b').Remove()
            select Selection.Defined;
        var reducer = selectr.Reduce();
        var result = Act(reducer, "abc");
        var step = Assert.IsType<Reduction.Unbounded>(result);
        Assert.Equal("c", step.Result);
    }

    [Fact]
    public void Trickier()
    {
        var selectr =
            from a_b in Stringr.Once('a', 'b')
            from replace in Stringr.Once('c', 'd').Replace(t => a_b.Text.ToUpper())
            select Selection.Defined;
        var reducer = selectr.Reduce();
        var result = Act(reducer, "--ac--bd--", a => ((string)a!).Contains('d'));
        var step = Assert.IsType<Reduction.Stop>(result);
        Assert.Equal("--aA--bd--", step.Result);
    }

    [Fact]
    public void Interpreter_WhiteSpace()
    {
        var result = Act(Remove.WhiteSpace.Reduce(), "  3 + 2 -  (3 + 1# - 2)", a => a!.ToString()!.Contains('#'));
        var step = Assert.IsType<Reduction.Unbounded>(result);
        Assert.Equal("3+2-(3+1#-2)", step.Result);
    }

    [Fact]
    public void Interpreter_LeftHandSideAndOperator()
    {
        var result = Act(Remove.LeftHandSideAndOperator.Reduce(), "3+2-(3+1#-2)", a => a!.ToString()!.Contains('#'));
        var step = Assert.IsType<Reduction.Unbounded>(result);
        Assert.Equal("(1#-2)", step.Result);
    }

    [Fact]
    public void Interpreter_LeftHandSideAndOperator_Unconditional()
    {
        var result = Act(Remove.LeftHandSideAndOperator.Reduce(), "3+2-(3+1#-2)", a => true);
        var step = Assert.IsType<Reduction.Unbounded>(result);
        Assert.Equal("(1#-2)", step.Result);
    }

    public class Remove
    {
        private static readonly SelectrOf<ITextSelection> Operator =
            Stringr.Once('+', '-', '*', '/', '^');
        private static readonly SelectrOf<ITextSelection> Number =
            Stringr.AtleastOnce(char.IsDigit);
        private static readonly SelectrOf<ITextSelection> Enclosed =
            Stringr.Balanced('(', ')');
        private static readonly SelectrOf<ITextSelection> Term =
            Number.Or(Enclosed);

        public static readonly SelectrOf<Selection> WhiteSpace =
            Stringr.AtleastOnce(char.IsWhiteSpace).Remove().Defined();

        public static readonly SelectrOf<Selection> BalancedParentheses =
            Stringr.Balanced('(', ')').Replace(a => a[1..^1]).Defined();

        public static readonly SelectrOf<Selection> LeftHandSideAndOperator =
            from lhs in Number.Remove()
            from op in Operator.Remove()
            select Selection.Defined;

        public static readonly SelectrOf<Selection> RightHandSideAndOperator =
            from op in Operator.Remove()
            from rhs in Number.Remove()
            select Selection.Defined;
    }
}