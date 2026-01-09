using QuickCheckr.Tests.CookBook.W_ParsingNumbers.Model.AstNodes;

namespace QuickCheckr.Tests.CookBook.W_ParsingNumbers.Model;

public class LostIn
{
    public static AstNode Translation(string input)
        => new Parser().Parse(Lexer.Tokenize(input));

    public static AstNode FaultyTranslation(string input)
        => new Parser(6).Parse(Lexer.Tokenize(input));
}