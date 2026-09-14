using System.Text;
using System.Text.RegularExpressions;
using QuickTestr.Tests.Docs.B_OracleBased.Sub.Model.Lexing;

namespace QuickTestr.Tests.Docs.B_OracleBased.Sub.Model;

public static partial class Lexer
{
    private static readonly Dictionary<char, Func<int, Token>> singleChars =
        new() {
            { '(',  a => new Token(TokenKind.LParen, "(", a) },
            { ')',  a => new Token(TokenKind.RParen, ")", a) },
            { '+',  a => new Token(TokenKind.Plus, "+", a) },
            { '-',  a => new Token(TokenKind.Minus, "-", a) },
            { '*',  a => new Token(TokenKind.Star, "*", a) },
            { '/',  a => new Token(TokenKind.Slash, "/", a) },
            { '^',  a => new Token(TokenKind.Caret, "^", a) },
            { '\0', a => new Token(TokenKind.End, "\0", a) },
        };

    [GeneratedRegex(@"^(?:0|[1-9][0-9]*)(?:\.[0-9]+)?$", RegexOptions.Compiled)]
    private static partial Regex IsDecimalRegex();
    private static bool IsValidDecimal(string s) => IsDecimalRegex().IsMatch(s);
    private static bool IsDecimalChar(char c) => char.IsDigit(c) || c == '.';

    public static IEnumerable<Token> Tokenize(string input)
    {
        var acc = new StringBuilder();
        var i = 0;
        foreach (var ch in input.Append('\0'))
        {
            var isDec = IsDecimalChar(ch);
            if (!isDec && acc.Length != 0)
            {
                var number = acc.ToString();
                if (!IsValidDecimal(number))
                    throw new Exception("Invalid decimal");
                yield return new Token(TokenKind.Number, number, i - number.Length);
                acc.Clear();
            }
            if (char.IsWhiteSpace(ch))
            {
                i++;
                continue;
            }
            if (isDec)
            {
                acc.Append(ch);
                i++;
                continue;
            }
            if (singleChars.TryGetValue(ch, out var make))
            {
                yield return make(i);
                i++;
                continue;
            }
            throw new Exception($"Unexpected character '{ch}' at {i}");
        }
    }
}
