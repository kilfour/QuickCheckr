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

//Explain.This<Guide>("guide.md");

