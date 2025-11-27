using QuickCheckr.Tests;
using QuickCheckr.Tests.Docs.A_Guide;
using QuickCheckr.Tests.Notes;
using QuickPulse.Explains;

//Explain.This<A_Notes>("notes.md");
Explain.This<Guide>("guide.md");

// Explain.This<Documentation>("full-doc.md");
// Explain.This<Reference>("reference.md");
// Explain.This<Cookbook>("cookbook.md");

Explain.OnlyThis<CreateReadMe>("README.md");

