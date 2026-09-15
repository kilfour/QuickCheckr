using QuickPulse.Explains;
using QuickTestr.Tests;
using QuickTestr.Tests.Challenges;
using QuickTestr.Tests.Docs;
using QuickTestr.Tests.Examples.DantesCalculator;
using QuickTestr.Tests.Notes;

Explain.This<TestrChallenges>("QuickTestr/Docs/quicktestr-challenges.md");
Explain.This<Documentation>("QuickTestr/Docs/quicktestr-doc.md");
Explain.This<DantesCalculator>("QuickTestr/Examples/dantes-calculator.md");
Explain.This<NotesRoot>("QuickTestr/DocsSinglePages/quicktestr-notes.md");

Explain.OnlyThis<CreateReadme>("QuickTestr/README.md");