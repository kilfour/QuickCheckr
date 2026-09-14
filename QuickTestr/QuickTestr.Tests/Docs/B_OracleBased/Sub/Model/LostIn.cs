using QuickTestr.Tests.Docs.B_OracleBased.Sub.Model.AstNodes;

namespace QuickTestr.Tests.Docs.B_OracleBased.Sub.Model;

public class LostIn
{
    public static AstNode Translation(string input)
        => new Parser().Parse(Lexer.Tokenize(input));

    public static AstNode FaultyTranslation(string input)
        => new Parser(6).Parse(Lexer.Tokenize(input));
}