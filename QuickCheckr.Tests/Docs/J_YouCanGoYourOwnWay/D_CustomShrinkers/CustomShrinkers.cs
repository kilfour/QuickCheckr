using QuickCheckr.Tests.Docs.H_OnShrinking.C_InputShrinking;
using QuickCheckr.Tests.Docs.J_YouCanGoYourOwnWay.D_CustomShrinkers.Including;
using QuickCheckr.Tests.Docs.M_HoneyIShrunkTheInputs.C_Objects.TheExamples;
using QuickCheckr.Tests.Tools;
using QuickPulse.Explains;

namespace QuickCheckr.Tests.Docs.J_YouCanGoYourOwnWay.D_CustomShrinkers;

[DocFile]
[DocLink(typeof(InputShrinking))]
[DocLink(typeof(E_CheckingNulls))]
[DocUnderConstruction]
[DocContent(
@"As explained in [OnShrinking][InputShrinking], a Shrinker is comprised of:
  1. A *Classifier* : responsible for finding the relevant inputs.
  2. A *Reducer* : responsible for finding a minimal value.

QuickCheckr allows you to define either one or both of these on a per-input basis.  
This gives you precise control over how shrinking behaves when the default strategies are insufficient for your domain.  

By default, built-in classifiers and reducers are provided for common types.  
Custom shrinkers are intended for cases where domain knowledge can guide shrinking more effectively.  

Let's start by having a look at supplying ...
")]
[DocInclude(typeof(A_ASimpleClassifier))]
[DocInclude(typeof(B_TheVariousCandidateSupplyingClassifiers))]
public class CustomShrinkers;