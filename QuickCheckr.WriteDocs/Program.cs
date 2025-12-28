using QuickCheckr.Tests;
using QuickCheckr.Tests.Docs;
using QuickCheckr.Tests.Drafts;
using QuickPulse.Explains;
using QuickCheckr.Tests.Experimental;
using QuickCheckr.Tests.Notes;

Explain.OnlyThis<CreateReadMe>("README.md");

Explain.These<Documentation>("Docs/");
Explain.These<Drafts>("Drafts/");
Explain.These<Experimental>("Experimental/");
// Explain.These<Notes>("Notes/");

Explain.This<Documentation>("DocsSinglePages/doc.md");
Explain.This<Drafts>("DocsSinglePages/drafts.md");
Explain.This<Experimental>("DocsSinglePages/experimental.md");
Explain.This<Notes>("DocsSinglePages/notes.md");

