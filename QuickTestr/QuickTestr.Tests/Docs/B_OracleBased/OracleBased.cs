using QuickPulse.Explains;

namespace QuickTestr.Tests.Docs.B_OracleBased;

[DocFile]
[DocFileHeader("Oracle-based Style Testing")]
[DocContent("""
The second style of QuickTestr verification.

Still stateless, still one invariant, but now we compare against something that already works: an oracle.

This is a very viable option in many cases. Refactoring legacy code for example.
Or in my case, verifying student code against a golden model.
""")]
public class OracleBased
{

}