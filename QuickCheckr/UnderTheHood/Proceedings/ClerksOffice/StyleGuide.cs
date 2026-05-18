// using System.Collections;
// using QuickCheckr.UnderTheHood.Proceedings.Depositions;
// using QuickPulse;

// namespace QuickCheckr.UnderTheHood.Proceedings.ClerksOffice;

// public static class StyleGuideExtensions
// {
//     private readonly static StyleGuide styleGuide = new();
//     public static Flow<Flow> Indent(this Flow<Flow> other, int level)
//         => other.Then(styleGuide.Indent(level));

// }

// public class StyleGuide
// {
//     private readonly static Flow<Flow> Separator = Pulse.Trace(", ");

//     private readonly static Flow<Flow> space = Pulse.Trace(" ");

//     private static readonly Flow<Flow> newLine = Pulse.Trace(Environment.NewLine);

//     protected Flow<Flow> OnNewLine()
//         => newLine.Then(space);

//     private static Flow<Flow> LineOf(int length) => Pulse.Trace(new string('-', length));

//     protected Flow<Flow> DrawLine() =>
//         space.Then(LineOf(60));

//     public Flow<Flow> Indent(int level)
//         => Pulse.Trace(new string(' ', level));

//     public record MaxLabel(int Length);

//     protected Flow<Flow> Labeled(bool labeled, string type, string label, string value) =>
//         from maxLabel in Pulse.Draw<MaxLabel>()
//         from _ in
//             OnNewLine()
//                 .Indent(2)
//                 .Trace("- ")
//                 .TraceIf(labeled, () => $"{type}: ")
//                 .Trace($"{label.PadRight(maxLabel.Length, ' ')} = {value}")
//         select Flow.Continue;

//     private static Flow<Flow> InterspersedPrimed(string input) =>
//         Pulse
//             .When<Joiner>(a => a.NeedsSeparator(), Separator)
//             .Trace(input);

//     public Flow<Flow> Interspersed(IEnumerable<string> input) =>
//         Pulse.Scoped<Joiner>(a => a.Start(),
//             Pulse.ToFlow(InterspersedPrimed, input));
// }

// public class CaseFileRender : StyleGuide
// {
//     // public static Flow<Flow> CourtStyleGuide(CaseFile input)
//     // {
//     //     var hasPassed = input.PassedExpectationDepositions.Count > 0;
//     //     var breakBeforePassed = input.HasEvidence && hasPassed;
//     //     return
//     //         Pulse
//     //             .Prime(() => new Joiner()).Dissipate()
//     //             .Prime(() => new MaxLabel(0)).Dissipate()
//     //             .ToFlowIf(input.HasEvidence, Evidence, () => input)
//     //             .OnType((CaseSummary a) => Summary(a), () => input)
//     //             .When(breakBeforePassed, newLine)
//     //             .ToFlowIf(hasPassed, PassedExpectations, () => input.PassedExpectationDepositions);
//     // }
// }
