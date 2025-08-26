namespace MathFlow.Core.Parser;

public enum TokenType
{
    Number,
    Variable,
    Plus,
    Minus,
    Multiply,
    Divide,
    Power,
    Modulo,
    LeftParen,
    RightParen,
    Comma,
    Function,
    Constant,
    Factorial,
    End
}

public class Token
{
    public TokenType Type { get; }
    public string Value { get; }
    public int Position { get; }
    
    public Token(TokenType type, string value, int position)
    {
        Type = type;
        Value = value;
        Position = position;
    }
    
    public override string ToString() => $"{Type}: {Value}";
}