using QuickCheckr.Tests;
using QuickCheckr.Tests.Docs;
using QuickCheckr.Tests.Docs.A_Guide;
using QuickCheckr.Tests.Docs.H_OnShrinking;
using QuickCheckr.Tests.Drafts;
using QuickCheckr.Tests.Drafts.T_Cookbook;
using QuickCheckr.Tests.Notes;
using QuickCheckr.Tests.PersonalNotes;
using QuickPulse.Explains;


Explain.These<Documentation>("Docs/");
// Explain.These<Drafts>("Drafts/");
// Explain.These<Notes>("Notes/");
// Explain.These<PersonalNotes>("PersonalNotes/");

Explain.OnlyThis<CreateReadMe>("README.md");

//Explain.This<Documentation>("DocsSinglePages/doc.md");
Explain.This<Guide>("DocsSinglePages/guide.md");
Explain.This<OnShrinking>("DocsSinglePages/shrinking.md");
Explain.This<CookBook>("DocsSinglePages/cookbook.md");

Explain.This<Drafts>("DocsSinglePages/drafts.md");
Explain.This<Notes>("DocsSinglePages/notes.md");
Explain.This<PersonalNotes>("DocsSinglePages/personal-notes.md");



