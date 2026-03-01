using QuickCheckr.Tests;
using QuickCheckr.Tests.Docs;
using QuickCheckr.Tests.Drafts;
using QuickPulse.Explains;
using QuickCheckr.Tests.Experimental;
using QuickCheckr.Tests.Notes;
using QuickCheckr.Tests.CookBook;
using QuickCheckr.Tests.Docs.J_YouCanGoYourOwnWay.D_CustomShrinkers;


// Explain.OnlyThis<CreateReadMe>("README.md");

Explain.These<CookBook>("CookBook/");
Explain.These<Documentation>("Docs/");
Explain.These<Drafts>("Drafts/");
Explain.These<Experimental>("Experimental/");
Explain.These<Notes>("Notes/");

Explain.This<CookBook>("DocsSinglePages/cookbook.md");
Explain.This<Documentation>("DocsSinglePages/doc.md");
Explain.This<Drafts>("DocsSinglePages/drafts.md");
Explain.This<Experimental>("DocsSinglePages/experimental.md");
Explain.This<Notes>("DocsSinglePages/notes.md");

// Explain.This<OnShrinking>("temp-on-shrinking.md");
// Explain.This<Reducers>("temp-reducers.md");
Explain.This<CustomShrinkers>("temp-shrinkers.md");
// Explain.This<GoingFunctional>("temp-invariants.md");
