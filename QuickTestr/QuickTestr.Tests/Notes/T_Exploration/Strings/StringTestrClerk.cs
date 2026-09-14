using QuickCheckr.FilingCabinet;
using QuickCheckr.FilingCabinet.Depositions;
using QuickCheckr.FilingCabinet.Depositions.Failure;
using QuickCheckr.Protocol;
using QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;
using QuickPulse;
using QuickTestr.Bolts.ClerksOffice;

namespace QuickTestr.Bolts.Builders.Strings;


public record IndexedMessage(int Index, string Message);

public class StringTestrClerk : ITranscribe
{
    public Flow<Flow> File(IRecord record) =>
         Pulse
            .OnType((Inquiry a) => Inquiry(a), () => record)
            .OnType((Findings a) => Findings(a), () => record)
            .OnType((CaseFile a) => CaseFile(a), () => record);

    private static Flow<Flow> Inquiry(Inquiry inquiry) =>
        Style
            .DrawTopLine()
            .ToFlow(PassedExpectations, inquiry.PassedExpectations);

    private static Flow<Flow> Findings(Findings findings) =>
        Style
            .DrawTopLine()
            .ToFlow(PassedExpectations, findings.PassedExpectations);

    private static Flow<Flow> CaseFile(CaseFile caseFile) =>
        Style
            .DrawTopLine()
            .ToFlow(Failure, caseFile.FailureDeposition)
            .ToFlow(PassedExpectations, caseFile.PassedExpectations);
    //.ToFlow(ExecutionFlow, caseFile.ExecutionDepositions);

    private static Flow<Flow> ExecutionFlow(ExecutionDeposition execution) =>
        Pulse
            //.ToFlow(InputFlow, execution.InputDepositions)
            .ToFlow(CommonClerk.WarningsFlow, execution.GetWarningDepositionsForReport());

    private static Flow<Flow> Failure(FailureDeposition input) =>
        Pulse
            .OnType((FailedExpectationDeposition a) => FailedExpectation(a), () => input)
            .OnType((FailedExceptionDeposition a) => Exception(a), () => input)
            .OnType((FailedVerificationDeposition a) => FailedVerification(a), () => input);

    private static Flow<Flow> FailedVerification(FailedVerificationDeposition input) =>
        Pulse.ToFlow(FailedExpectationMessage,
            input.Messages.Select((a, index) => new IndexedMessage(index, a)));

    private static Flow<Flow> FailedExpectationMessage(IndexedMessage indexedMessage) =>
        Style.OnNewLine()
            .Trace(indexedMessage.Message)
            .When(indexedMessage.Index > 0 && indexedMessage.Index % 2 != 0, Style.DrawLine());

    private static Flow<Flow> FailedExpectation(FailedExpectationDeposition input) =>
        Pulse.ToFlow(FailedExpectationMessage,
            input.Messages.Select((a, index) => new IndexedMessage(index, a)));

    private static Flow<Flow> Exception(FailedExceptionDeposition input) =>
        from withStackTrace in Pulse.Draw<Decorum, bool>(a => a.StackTrace)
        let message = withStackTrace ? input.StackTrace : input.FailureDescription
        from _ in
            Style
                .DoubleLine()
                .OnNewLine().Indent(1).Exclaim(message)
                .DoubleLine()
        select Flow.Continue;

    public static Flow<Flow> PassedExpectations(IEnumerable<PassedExpectation> input) =>
        Pulse
            .ToFlow(a => Style.OnNewLine().Trace($"- {a.Label}: {a.TimesPassed}x"), input)
            .DrawLine();
}