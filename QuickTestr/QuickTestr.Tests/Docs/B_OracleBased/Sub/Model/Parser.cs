using QuickTestr.Tests.Docs.B_OracleBased.Sub.Model.AstNodes;
using QuickTestr.Tests.Docs.B_OracleBased.Sub.Model.Lexing;

namespace QuickTestr.Tests.Docs.B_OracleBased.Sub.Model;

public class Parser(int unaryMinusPrecedence = 4)
{
    public AstNode Parse(IEnumerable<Token> tokens)
    {
        foreach (var token in tokens)
        {
            if (token.Kind == TokenKind.Number)
            {
                PushNumber(token.Text);
                continue;
            }

            if (token.IsOperator)
            {
                PushOperator(token.Kind);
                continue;
            }

            if (token.Kind == TokenKind.LParen)
            {
                OpenParen();
                continue;
            }

            if (token.Kind == TokenKind.RParen)
            {
                CloseParen();
                continue;
            }
        }
        return Finish();
    }

    private Stack<AstNode> Values { get; } = new();
    private Stack<TokenKind> Operators { get; } = new();

    private TokenKind? lastToken;

    private int Precedence(TokenKind kind) => kind switch
    {
        TokenKind.Caret => 5,
        TokenKind.UnaryMinus => unaryMinusPrecedence,
        TokenKind.Star or TokenKind.Slash => 3,
        TokenKind.Plus or TokenKind.Minus => 2,
        _ => 0
    };

    private static bool IsRightAssociative(TokenKind kind) =>
        kind is TokenKind.Caret or TokenKind.UnaryMinus;

    private void PushNumber(string text)
    {
        Values.Push(new Number(double.Parse(text, System.Globalization.CultureInfo.InvariantCulture)));
        lastToken = TokenKind.Number;
    }

    private void PushOperator(TokenKind op)
    {
        if (IsUnaryMinus(op))
            op = TokenKind.UnaryMinus;

        if (op == TokenKind.UnaryMinus)
        {
            Operators.Push(op);
            lastToken = op;
            return;
        }

        while (Operators.Count > 0 && Operators.Peek() != TokenKind.LParen)
        {
            var top = Operators.Peek();
            var cond = IsRightAssociative(op)
                ? Precedence(top) > Precedence(op)
                : Precedence(top) >= Precedence(op);
            if (!cond) break;
            ApplyTop();
        }

        Operators.Push(op);
        lastToken = op;
    }

    private bool IsUnaryMinus(TokenKind op) =>
        op == TokenKind.Minus && (
           lastToken is null
        || lastToken is TokenKind.LParen
        || lastToken is TokenKind.Plus
        || lastToken is TokenKind.Minus
        || lastToken is TokenKind.Star
        || lastToken is TokenKind.Slash
        || lastToken is TokenKind.Caret
        || lastToken is TokenKind.UnaryMinus);

    private void OpenParen()
    {
        Operators.Push(TokenKind.LParen);
        lastToken = TokenKind.LParen;
    }

    private void CloseParen()
    {
        while (Operators.Count > 0 && Operators.Peek() != TokenKind.LParen)
            ApplyTop();
        if (Operators.Count == 0 || Operators.Pop() != TokenKind.LParen)
            throw new InvalidOperationException("Mismatched parentheses");
        lastToken = TokenKind.RParen;
    }

    public AstNode Finish()
    {
        while (Operators.Count > 0)
        {
            if (Operators.Peek() == TokenKind.LParen)
                throw new Exception("Mismatched parentheses");
            ApplyTop();
        }
        return Values.Count == 1 ? Values.Pop() : throw new Exception("Parse error");
    }

    private void ApplyTop()
    {
        if (Operators.Count == 0)
            throw new InvalidOperationException("Operator stack empty");

        var op = Operators.Pop();

        if (op == TokenKind.UnaryMinus)
        {
            if (Values.Count < 1)
                throw new InvalidOperationException("Unary '-' missing operand");
            var value = Values.Pop();
            Values.Push(new UnaryMinus(value));
            return;
        }

        if (Values.Count < 2)
            throw new InvalidOperationException($"Operator '{op}' missing operands");

        var rightHandSide = Values.Pop();
        var leftHandSide = Values.Pop();

        Values.Push(op switch
        {
            TokenKind.Plus => new Addition(leftHandSide, rightHandSide),
            TokenKind.Minus => new Subtraction(leftHandSide, rightHandSide),
            TokenKind.Star => new Multiplication(leftHandSide, rightHandSide),
            TokenKind.Slash => new Division(leftHandSide, rightHandSide),
            TokenKind.Caret => new Power(leftHandSide, rightHandSide),
            _ => throw new InvalidOperationException($"Unexpected operator {op}")
        });
    }
}
