using QuickPulse.Explains;
using QuickTestr.Tests;
using QuickTestr.Tests.Challenges;
using QuickTestr.Tests.Docs;
using QuickTestr.Tests.Examples.DantesCalculator;
using QuickTestr.Tests.Notes;

Explain.This<TestrChallenges>("QuickTestr/Docs/challenges.md");
Explain.This<Documentation>("QuickTestr/Docs/doc.md");
Explain.This<DantesCalculator>("QuickTestr/Examples/dantes-calculator.md");
Explain.This<NotesRoot>("QuickTestr/DocsSinglePages/notes.md");

Explain.OnlyThis<CreateReadme>("QuickTestr/README.md");