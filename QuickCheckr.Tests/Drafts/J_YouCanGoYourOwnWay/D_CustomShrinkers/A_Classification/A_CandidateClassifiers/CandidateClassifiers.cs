using QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.D_CustomShrinkers.A_Classification.A_CandidateClassifiers.Methods;
using QuickCheckr.Tests.Drafts.M_HoneyIShrunkTheInputs.C_Objects.TheExamples;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Drafts.J_YouCanGoYourOwnWay.D_CustomShrinkers.A_Classification.A_CandidateClassifiers;

[DocFile]
[DocLink(typeof(E_CheckingNulls))]
[DocFileHeader("Candidate-based Classifiers")]
[DocContent(
@"Here is an overview of all the different ways you can define *candidates* for classification.  
Using a more relevant example this time (where possible), that allows you to deal with [this][E_CheckingNulls] relatively common issue.")]
[DocInclude(typeof(A_TryValues))]
[DocInclude(typeof(B_TryCollection))]
[DocInclude(typeof(C_TryFunction))]
[DocInclude(typeof(D_TryFuzzr))]
public class CandidateClassifiers;