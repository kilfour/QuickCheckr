using QuickCheckr.Tests.Tools.ThePress.Printing;
using QuickPulse;

namespace QuickCheckr.Tests.Tools.ThePress;

public class TheEditor
{
    private static Flow<Flow> Emit(string line) => Pulse.Trace($"        {line}");

    private static Flow<Flow> EmitIf(bool flag, string line)
        => Pulse.TraceIf(flag, () => $"        {line}");

    private static readonly Flow<(int, int)> Trace =
        from cursor in Pulse.Start<(int executionIndex, int traceIndex)>()
        from article in Pulse.Draw<Article>()
        let inputArticle = article.Execution(cursor.executionIndex).Trace(cursor.traceIndex).Read()
        from inputLabel in Emit($"Assert.Equal(\"{inputArticle.Label}\", article.Execution({cursor.executionIndex}).Trace({cursor.traceIndex}).Read().Label);")
        from inputValue in Emit($"Assert.Equal(\"{inputArticle.Value}\", article.Execution({cursor.executionIndex}).Trace({cursor.traceIndex}).Read().Value);")
        from newline in Pulse.Trace("")
        select cursor;

    private static readonly Flow<(int, int)> Input =
        from cursor in Pulse.Start<(int executionIndex, int inputIndex)>()
        from article in Pulse.Draw<Article>()
        let inputArticle = article.Execution(cursor.executionIndex).Input(cursor.inputIndex).Read()
        from inputLabel in Emit($"Assert.Equal(\"{inputArticle.Label}\", article.Execution({cursor.executionIndex}).Input({cursor.inputIndex}).Read().Label);")
        from inputValue in Emit($"Assert.Equal(\"{inputArticle.Value}\", article.Execution({cursor.executionIndex}).Input({cursor.inputIndex}).Read().Value);")
        from newline in Pulse.Trace("")
        select cursor;

    private static readonly Flow<(int, int)> Action =
        from cursor in Pulse.Start<(int executionIndex, int actionIndex)>()
        from article in Pulse.Draw<Article>()
        let actionArticle = article.Execution(cursor.executionIndex).Action(cursor.actionIndex).Read()
        from action in Emit($"Assert.Equal(\"{actionArticle.Label}\", article.Execution({cursor.executionIndex}).Action({cursor.actionIndex}).Read().Label);")
        from newline in Pulse.Trace("")
        select cursor;

    private static readonly Flow<int> Execution =
        from index in Pulse.Start<int>()
        from article in Pulse.Draw<Article>()
        let executionDeposition = article.Execution(index).Read()
        from execution in Emit($"Assert.Equal({executionDeposition.ExecutionId}, article.Execution({index}).Read().ExecutionId);")
        from times in EmitIf(executionDeposition.Times > 1, $"Assert.Equal({executionDeposition.Times}, article.Execution({index}).Read().Times);")
        from newline in Pulse.Trace("")
        from actions in Pulse.ToFlow(Action, Enumerable.Range(1, executionDeposition.ActionDepositions.Count).Select(a => (index, a)))
        from inputs in Pulse.ToFlow(Input, Enumerable.Range(1, executionDeposition.InputDepositions.Count).Select(a => (index, a)))
        from traces in Pulse.ToFlow(Trace, Enumerable.Range(1, executionDeposition.TraceDepositions.Count).Select(a => (index, a)))
        select index;

    public readonly static Flow<Article> ProofReads =
        from article in Pulse.Start<Article>()
        from prime in Pulse.Prime(() => article)
        from failedSpec in EmitIf(
            article.FailedSpec() is not null,
            $"Assert.Equal(\"{article.FailedSpec()}\", article.FailedSpec());")
        from newline1 in Pulse.Trace("")
        from executionCount in EmitIf(
            article.Total().Executions() > 0,
            $"Assert.Equal({article.Total().Executions()}, article.Total().Executions());")
        from actionCount in EmitIf(
            article.Total().Actions() > 0,
            $"Assert.Equal({article.Total().Actions()}, article.Total().Actions());")
        from inputCount in EmitIf(
            article.Total().Inputs() > 0,
            $"Assert.Equal({article.Total().Inputs()}, article.Total().Inputs());")
        from traceCount in EmitIf(
            article.Total().Traces() > 0,
            $"Assert.Equal({article.Total().Traces()}, article.Total().Traces());")
        from newline2 in Pulse.Trace("")
        from executions in Pulse.ToFlow(Execution, Enumerable.Range(1, article.Total().Executions()))
        select article;
}
