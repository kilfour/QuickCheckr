using QuickPulse.Explains;
using QuickTestr.Tests.Docs.A_GettingStarted;
using QuickTestr.Tests.Docs.A_PropertyBased;
using QuickTestr.Tests.Docs.B_OracleBased;
using QuickTestr.Tests.Docs.C_ModelBasedTesting;

namespace QuickTestr.Tests.Docs;

[DocFile]
[DocFileHeader("QuickTestr")]
[DocLink(typeof(GettingStarted))]
[DocLink(typeof(PropertyBased))]
[DocLink(typeof(OracleBased))]
[DocLink(typeof(ModelBased))]
[DocContent("""
Start with [Getting Started][GettingStarted], then choose the style that fits the
thing you want to verify:

- [Property-based][PropertyBased] : Define what should always hold.
- [Oracle-based][OracleBased]: Compare against something that already works.
- [Model-based][ModelBased]: Compare state transitions against a model.
""")]
public class Documentation { }
