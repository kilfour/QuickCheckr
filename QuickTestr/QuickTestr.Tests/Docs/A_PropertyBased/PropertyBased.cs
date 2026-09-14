using QuickPulse.Explains;

namespace QuickTestr.Tests.Docs.A_PropertyBased;

[DocFile]
[DocFileHeader("Property-based Style Testing")]
[DocContent("""
In property-based testing you describe **what should always be true**, regardless of the input.  
QuickTestr generates many inputs and tries to falsify your rule, shrinking failures to a minimal example.

A classic to begin with:
""")]
public class PropertyBased;