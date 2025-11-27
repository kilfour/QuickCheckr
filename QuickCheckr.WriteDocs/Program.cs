using QuickCheckr.Tests;
using QuickCheckr.Tests.Docs.A_Guide;
using QuickPulse.Explains;

Explain.This<Guide>("guide.md");
Explain.These<Guide>("Docs/Guide/");
Explain.OnlyThis<CreateReadMe>("README.md");

