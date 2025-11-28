using QuickCheckr.Tests;
using QuickCheckr.Tests.Docs.A_Guide;
using QuickCheckr.Tests.Notes;
using QuickPulse.Explains;

Explain.This<Guide>("guide.md");
Explain.This<Notes>("notes.md");
Explain.These<Guide>("Docs/Guide/");
Explain.OnlyThis<CreateReadMe>("README.md");

