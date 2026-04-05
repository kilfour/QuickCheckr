using QuickCheckr.StringReduction;
using QuickCheckr.Tests.Tools;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts;
using QuickCheckr.UnderTheHood.Runners.InputShrinking.Bolts.InputReduction;
using QuickCheckr.UnderTheHood.Warnings;

namespace QuickCheckr.Tests.Investigations.J_StringReduction;

public class StringReductionTests : BaseTest
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
    public void Inside()
    {
        var reducer = Select.Balanced('(', ')').Inside().Remove();
        var result = Act(reducer, "1 + ( 2 + 3)");
        var step = Assert.IsType<Reduction.Unbounded>(result);
        Assert.Equal("1 + ()", step.Result);
    }

    [Fact]
    public void InsideMany()
    {
        var reducer = Select.Balanced('(', ')').Inside().Remove();
        var result = Act(reducer, "(((1)))");
        var step = Assert.IsType<Reduction.Unbounded>(result);
        Assert.Equal("()", step.Result);
    }

    [Fact]
    public void TermReduction()
    {
        var reducer =
            Select
            .Balanced('(', ')').Enclosed().While(char.IsWhiteSpace).OneOf('+')
            .Or(Select.While(a => char.IsDigit(a) || a == ' ').While(char.IsWhiteSpace).OneOf('+'))
            .Remove();
        var result = Act(reducer, "(1 + 2) + (3 + 4) + (5 + 6)");
        var step = Assert.IsType<Reduction.Unbounded>(result);
        Assert.Equal("  ( 6)", step.Result);
    }

    [Fact]
    public void TermReduction_FromParser()
    {
        var reducer =
            Select
            .Balanced('(', ')').Enclosed().While(char.IsWhiteSpace).OneOf(operators)
            .Or(Select.While(a => char.IsDigit(a)).While(char.IsWhiteSpace).OneOf(operators))
            .Remove();
        var result = Act(reducer, "- (3 * 1 - 2 * 2 / 2 * 3) ^ (1 * 1 - 2 * 3 / 2) ^ (1 / 1) ^ 1 - (2 / 3 * 1 * 2 + 1 + 2 / 3 / 2) * (1 / 2 / 3 * 3 - 1) ^ 3");
        var step = Assert.IsType<Reduction.Unbounded>(result);
        Assert.Equal("-       3", step.Result);
    }

    [Fact]
    public void TermReduction_FromParser_Again()
    {
        var reducer =
            Select.While(char.IsDigit).While(char.IsWhiteSpace).OneOf(operators)
            .Remove();
        var result = Act(reducer, "- (3 * 1 - 2 * 2 / 2 * 3) ^ (1 * 1 - 2 * 3 / 2) ^ (1 / 1) ^ 1 - (2 / 3 * 1 * 2 + 1 + 2 / 3 / 2) * (1 / 2 / 3 * 3 - 1) ^ 3");
        var step = Assert.IsType<Reduction.Unbounded>(result);
        Assert.Equal("- (     3) ^ (    2) ^ ( 1) ^  (       2) * (    1) ^ 3", step.Result);
    }

    [Fact]
    public void Removing_Parenthesis()
    {
        var reducer = Select.Balanced('(', ')').Delimiters().Remove();
        var result = Act(reducer, "- (3 * 1 - 2 * 2 / 2 * 3) ^ (1 * 1 - 2 * 3 / 2) ^ (1 / 1) ^ 1 - (2 / 3 * 1 * 2 + 1 + 2 / 3 / 2) * (1 / 2 / 3 * 3 - 1) ^ 3");
        var step = Assert.IsType<Reduction.Unbounded>(result);
        Assert.Equal("- 3 * 1 - 2 * 2 / 2 * 3 ^ 1 * 1 - 2 * 3 / 2 ^ 1 / 1 ^ 1 - 2 / 3 * 1 * 2 + 1 + 2 / 3 / 2 * 1 / 2 / 3 * 3 - 1 ^ 3", step.Result);
    }

    private readonly char[] operators = ['+', '-', '*', '/', '^'];
}
