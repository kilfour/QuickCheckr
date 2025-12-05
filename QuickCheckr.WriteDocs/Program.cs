using QuickCheckr.Tests;
using QuickCheckr.Tests.Docs;
using QuickCheckr.Tests.Drafts;
using QuickCheckr.Tests.Notes;
using QuickCheckr.Tests.PersonalNotes;
using QuickPulse.Explains;


Explain.These<Documentation>("Docs/");
Explain.These<Drafts>("Drafts/");
Explain.These<Notes>("Notes/");
Explain.These<PersonalNotes>("PersonalNotes/");

Explain.OnlyThis<CreateReadMe>("README.md");

Explain.This<Documentation>("Single/doc.md");
Explain.This<Drafts>("Single/drafts.md");
Explain.This<Notes>("Single/notes.md");
Explain.This<PersonalNotes>("Single/personal-notes.md");

