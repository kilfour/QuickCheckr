using QuickCheckr.Tests.Notes.T_Examples.Q_ParsingNumbers.Model.AstNodes;

namespace QuickCheckr.Tests.Notes.T_Examples.Q_ParsingNumbers.Model;

public class LostIn
{
    public static AstNode Translation(string input)
        => new Parser().Parse(Lexer.Tokenize(input));
}