using QuickTestr.Tests.Tools;
using QuickFuzzr;
using QuickPulse.Explains;
using QuickPulse.Instruments;
using QuickCheckr.Authoring.ThePress.Printing;
using QuickCheckr.Authoring.ThePress;

namespace QuickTestr.Tests.Notes.M_OracleTesting;

[DocFile]
public class B_WhatIfItThrows : QuickTestrOracleTest<B_WhatIfItThrows>
{
    protected override bool Asserts => false;
    protected override bool Report => false;
    protected override bool Explain => false;

    [Fact]
    [DocTestrHeader]
    [DocTestr]
    [DocBoldHeader("Expected")]
    [DocExample(typeof(B_WhatIfItThrows), nameof(AddCorrect))]
    [DocBoldHeader("Actual")]
    [DocExample(typeof(B_WhatIfItThrows), nameof(AddBuggy))]
    [DocReportHeader]
    [DocReport]
    public override void Example() => Document();

    [CodeSnippet]
    [CodeRemoveJournalist]
    [CodeRemove("1055521326")]
    protected override void GetTestr(Journalist journalist) =>
        Testr.Named("AddBuggy Matches AddCorrect")
            .For(Fuzzr.Tuple(Fuzzr.Int(), Fuzzr.Int()))
            .Expected(a => AddCorrect(a.Item1, a.Item2))
            .Actual(a => AddBuggy(a.Item1, a.Item2))
            .StoreCaseFiles(journalist)
            .Run(1055521326);

    protected override void Verify(Article article)
    {
        Assert.Equal("AddBuggy Matches AddCorrect", article.FailureDescription());
        Assert.Equal(1, article.Total().Executions());
        Assert.Equal(2, article.Total().Actions());
        Assert.Equal(1, article.Total().Inputs());
        Assert.Equal(2, article.Total().Traces());
        Assert.Equal(1, article.Execution(1).Read().ExecutionId);
        Assert.Equal("Actual", article.Execution(1).Action(1).Read().Label);
        Assert.Equal("Expected", article.Execution(1).Action(2).Read().Label);
        Assert.Equal("Input", article.Execution(1).Input(1).Read().Label);
        Assert.Equal("( 10, _ )", article.Execution(1).Input(1).Read().Value);
        Assert.Equal("( 10, 13 )", article.Execution(1).Input(1).Read().Original.Value);
        Assert.Equal("Expected", article.Execution(1).Trace(1).Read().Label);
        Assert.Equal("23", article.Execution(1).Trace(1).Read().Value);
        Assert.Equal("Actual  ", article.Execution(1).Trace(2).Read().Label);
        Assert.Equal("ComputerSaysNo: a is too small", article.Execution(1).Trace(2).Read().Value);
    }

    [CodeExample]
    public int AddCorrect(int a, int b)
    {
        if (a < 10)
            ComputerSays.No<int>("a is too small");
        return a + b;
    }

    [CodeExample]
    public int AddBuggy(int a, int b)
    {
        if (a <= 10)
            ComputerSays.No<int>("a is too small");
        return a + b;
    }
}
