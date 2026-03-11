using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickPulse;

namespace QuickCheckr.Tests.Tools.ThePress;

public class TheEditor
{
    private static Flow<Flow> Emit(string line) => Pulse.Trace($"        {line}");

    private static Flow<Flow> EmitIf(bool flag, string line)
        => Pulse.TraceIf(flag, () => $"        {line}");

    private static readonly Flow<(int, int)> Warning =
        from cursor in Pulse.Start<(int executionIndex, int warningIndex)>()
        from article in Pulse.Draw<Article>()
        let warningArticle = article.Execution(cursor.executionIndex).Warning(cursor.warningIndex).Read()
        from value in Emit($"Assert.Equal(\"{warningArticle.Value}\", article.Execution({cursor.executionIndex}).Warning({cursor.warningIndex}).Read().Value);")
        select cursor;

    private static readonly Flow<(int, int)> Trace =
        from cursor in Pulse.Start<(int executionIndex, int traceIndex)>()
        from article in Pulse.Draw<Article>()
        let traceArticle = article.Execution(cursor.executionIndex).Trace(cursor.traceIndex).Read()
        from label in Emit($"Assert.Equal(\"{traceArticle.Label}\", article.Execution({cursor.executionIndex}).Trace({cursor.traceIndex}).Read().Label);")
        from value in Emit($"Assert.Equal(\"{ReplaceDoubleQuotes(traceArticle.Value)}\", article.Execution({cursor.executionIndex}).Trace({cursor.traceIndex}).Read().Value);")
        from labeled in EmitIf(!traceArticle.Labeled, $"Assert.False(article.Execution({cursor.executionIndex}).Trace({cursor.traceIndex}).Read().Labeled);")
        select cursor;

    private static readonly Flow<(int, int)> PoolTrace =
        from cursor in Pulse.Start<(int executionIndex, int traceIndex)>()
        from article in Pulse.Draw<Article>()
        let poolTraceArticle = article.Execution(cursor.executionIndex).PoolTrace(cursor.traceIndex).Read()
        from label in Emit($"Assert.Equal(\"{poolTraceArticle.Label}\", article.Execution({cursor.executionIndex}).PoolTrace({cursor.traceIndex}).Read().Label);")
        from value in Emit($"Assert.Equal(\"{poolTraceArticle.Value}\", article.Execution({cursor.executionIndex}).PoolTrace({cursor.traceIndex}).Read().Value);")
        from labeled in EmitIf(!poolTraceArticle.Labeled, $"Assert.False(article.Execution({cursor.executionIndex}).PoolTrace({cursor.traceIndex}).Read().Labeled);")
        select cursor;

    private static readonly Flow<(int, int)> Input =
        from cursor in Pulse.Start<(int executionIndex, int inputIndex)>()
        from article in Pulse.Draw<Article>()
        let inputArticle = article.Execution(cursor.executionIndex).Input(cursor.inputIndex).Read()
        from inputLabel in Emit($"Assert.Equal(\"{inputArticle.Label}\", article.Execution({cursor.executionIndex}).Input({cursor.inputIndex}).Read().Label);")
        from inputValue in Emit($"Assert.Equal(\"{ReplaceDoubleQuotes(inputArticle.Value)}\", article.Execution({cursor.executionIndex}).Input({cursor.inputIndex}).Read().Value);")
        from inputOriginal in EmitIf(inputArticle.Original.HasValue, $"Assert.Equal(\"{ReplaceDoubleQuotes(inputArticle.Original.Value!)}\", article.Execution({cursor.executionIndex}).Input({cursor.inputIndex}).Read().Original.Value);")
        from inputRedux in EmitIf(inputArticle.Redux.HasValue, $"Assert.Equal(\"{ReplaceDoubleQuotes(inputArticle.Redux.Value!)}\", article.Execution({cursor.executionIndex}).Input({cursor.inputIndex}).Read().Redux.Value);")
        from labeled in EmitIf(!inputArticle.Labeled, $"Assert.False(article.Execution({cursor.executionIndex}).Input({cursor.inputIndex}).Read().Labeled);")
        select cursor;

    private static object ReplaceDoubleQuotes(object value)
    {
        if (value is string str)
            return str.Replace(@"""", @"\""");
        return value;
    }

    private static readonly Flow<(int, int)> Action =
        from cursor in Pulse.Start<(int executionIndex, int actionIndex)>()
        from article in Pulse.Draw<Article>()
        let actionArticle = article.Execution(cursor.executionIndex).Action(cursor.actionIndex).Read()
        from action in Emit($"Assert.Equal(\"{actionArticle.Label}\", article.Execution({cursor.executionIndex}).Action({cursor.actionIndex}).Read().Label);")
        select cursor;

    private static readonly Flow<int> Execution =
        from index in Pulse.Start<int>()
        from article in Pulse.Draw<Article>()

        let executionDeposition = article.Execution(index).Read()
        from execution in Emit($"Assert.Equal({executionDeposition.ExecutionId}, article.Execution({index}).Read().ExecutionId);")
        from times in EmitIf(article.Execution(index).Times > 1, $"Assert.Equal({article.Execution(index).Times}, article.Execution({index}).Times);")
        from actions in Pulse.ToFlow(Action, Enumerable.Range(1, executionDeposition.ActionDepositions.Count).Select(a => (index, a)))
        from inputs in Pulse.ToFlow(Input, Enumerable.Range(1, executionDeposition.InputDepositions.Count).Select(a => (index, a)))
        from poolTraces in Pulse.ToFlow(PoolTrace, Enumerable.Range(1, executionDeposition.PoolTraceDepositions.Count).Select(a => (index, a)))
        from traces in Pulse.ToFlow(Trace, Enumerable.Range(1, executionDeposition.TraceDepositions.Count).Select(a => (index, a)))
        from warnings in Pulse.ToFlow(Warning, Enumerable.Range(1, executionDeposition.WarningDepositions.Count).Select(a => (index, a)))
        select index;

    private static readonly Flow<int> PassedExpectation =
        from index in Pulse.Start<int>()
        from article in Pulse.Draw<Article>()
        let passedExpectationDeposition = article.PassedExpectation(index).Read()
        from label in Emit($"Assert.Equal(\"{passedExpectationDeposition.Label}\", article.PassedExpectation({index}).Read().Label);")
        from times in Emit($"Assert.Equal({passedExpectationDeposition.TimesPassed}, article.PassedExpectation({index}).Read().TimesPassed);")
        select index;

    public readonly static Flow<Article> ProofReads =
        from article in Pulse.Start<Article>()
        from prime in Pulse.Prime(() => article)
        from failedExpectation in EmitIf(
            article.FailureDescription() is not null,
            $"Assert.Equal(\"{article.FailureDescription()}\", article.FailureDescription());")
        from failedAssat in EmitIf(
            article.VerifyFailed() is not null,
            $"Assert.Equal(\"{article.VerifyFailed()}\", article.VerifyFailed());")
        from executionCount in EmitIf(
            article.Total().Executions() > 0,
            $"Assert.Equal({article.Total().Executions()}, article.Total().Executions());")
        from actionCount in EmitIf(
            article.Total().Actions() > 0,
            $"Assert.Equal({article.Total().Actions()}, article.Total().Actions());")
        from inputCount in EmitIf(
            article.Total().Inputs() > 0,
            $"Assert.Equal({article.Total().Inputs()}, article.Total().Inputs());")
        from poolTraceCount in EmitIf(
            article.Total().PoolTraces() > 0,
            $"Assert.Equal({article.Total().PoolTraces()}, article.Total().PoolTraces());")
        from traceCount in EmitIf(
            article.Total().Traces() > 0,
            $"Assert.Equal({article.Total().Traces()}, article.Total().Traces());")
        from warningCount in EmitIf(
            article.Total().Warnings() > 0,
            $"Assert.Equal({article.Total().Warnings()}, article.Total().Warnings());")
        from passedExpectationCount in EmitIf(
            article.Total().PassedExpectations() > 0,
            $"Assert.Equal({article.Total().PassedExpectations()}, article.Total().PassedExpectations());")
        from shrinkCount in Emit(
            $"Assert.Equal({article.ShrinkCount}, article.ShrinkCount);")
        from executions in Pulse.ToFlow(Execution, Enumerable.Range(1, article.Total().Executions()))
        from passedExpectations in Pulse.ToFlow(PassedExpectation, Enumerable.Range(1, article.Total().PassedExpectations()))
        select article;
}
