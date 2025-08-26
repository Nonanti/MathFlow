using System.Globalization;
using System.Text;

namespace MathFlow.Core.Parser;

public class Lexer
{
    private readonly string _input;
    private int _position;
    private readonly Dictionary<string, TokenType> _keywords;
    private readonly HashSet<string> _constants;
    
    public Lexer(string input)
    {
        _input = input ?? throw new ArgumentNullException(nameof(input));
        _position = 0;
        
        _keywords = new Dictionary<string, TokenType>(StringComparer.OrdinalIgnoreCase)
        {
            ["sin"] = TokenType.Function,
            ["cos"] = TokenType.Function,
            ["tan"] = TokenType.Function,
            ["asin"] = TokenType.Function,
            ["arcsin"] = TokenType.Function,
            ["acos"] = TokenType.Function,
            ["arccos"] = TokenType.Function,
            ["atan"] = TokenType.Function,
            ["arctan"] = TokenType.Function,
            ["sinh"] = TokenType.Function,
            ["cosh"] = TokenType.Function,
            ["tanh"] = TokenType.Function,
            ["exp"] = TokenType.Function,
            ["ln"] = TokenType.Function,
            ["log"] = TokenType.Function,
            ["log10"] = TokenType.Function,
            ["sqrt"] = TokenType.Function,
            ["abs"] = TokenType.Function,
            ["floor"] = TokenType.Function,
            ["ceil"] = TokenType.Function,
            ["ceiling"] = TokenType.Function,
            ["round"] = TokenType.Function,
            ["sign"] = TokenType.Function,
            ["min"] = TokenType.Function,
            ["max"] = TokenType.Function,
            ["pow"] = TokenType.Function,
            ["factorial"] = TokenType.Function
        };
        
        _constants = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "pi", "π", "e", "tau", "τ", "phi", "φ"
        };
    }
    
    public List<Token> Tokenize()
    {
        var tokens = new List<Token>();
        
        while (_position < _input.Length)
        {
            SkipWhitespace();
            
            if (_position >= _input.Length)
                break;
            
            var token = NextToken();
            if (token != null)
                tokens.Add(token);
        }
        
        tokens.Add(new Token(TokenType.End, "", _position));
        return tokens;
    }
    
    private Token? NextToken()
    {
        if (_position >= _input.Length)
            return null;
        
        var ch = _input[_position];
        var startPos = _position;
        
        if (char.IsDigit(ch) || ch == '.')
        {
            return ReadNumber();
        }
        
        if (char.IsLetter(ch) || ch == '_')
        {
            return ReadIdentifier();
        }
        
        _position++;
        
        return ch switch
        {
            '+' => new Token(TokenType.Plus, "+", startPos),
            '-' => new Token(TokenType.Minus, "-", startPos),
            '*' => new Token(TokenType.Multiply, "*", startPos),
            '/' => new Token(TokenType.Divide, "/", startPos),
            '^' => new Token(TokenType.Power, "^", startPos),
            '%' => new Token(TokenType.Modulo, "%", startPos),
            '(' => new Token(TokenType.LeftParen, "(", startPos),
            ')' => new Token(TokenType.RightParen, ")", startPos),
            ',' => new Token(TokenType.Comma, ",", startPos),
            '!' => new Token(TokenType.Factorial, "!", startPos),
            'π' => new Token(TokenType.Constant, "π", startPos),
            'τ' => new Token(TokenType.Constant, "τ", startPos),
            'φ' => new Token(TokenType.Constant, "φ", startPos),
            _ => throw new LexerException($"Unexpected character '{ch}' at position {startPos}")
        };
    }
    
    private Token ReadNumber()
    {
        var startPos = _position;
        var sb = new StringBuilder();
        var hasDecimal = false;
        var hasExponent = false;
        
        while (_position < _input.Length)
        {
            var ch = _input[_position];
            
            if (char.IsDigit(ch))
            {
                sb.Append(ch);
                _position++;
            }
            else if (ch == '.' && !hasDecimal && !hasExponent)
            {
                hasDecimal = true;
                sb.Append(ch);
                _position++;
            }
            else if ((ch == 'e' || ch == 'E') && !hasExponent)
            {
                hasExponent = true;
                sb.Append(ch);
                _position++;
                
                if (_position < _input.Length && (_input[_position] == '+' || _input[_position] == '-'))
                {
                    sb.Append(_input[_position]);
                    _position++;
                }
            }
            else
            {
                break;
            }
        }
        
        var numberStr = sb.ToString();
        
        if (!double.TryParse(numberStr, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
        {
            throw new LexerException($"Invalid number format '{numberStr}' at position {startPos}");
        }
        
        return new Token(TokenType.Number, numberStr, startPos);
    }
    
    private Token ReadIdentifier()
    {
        var startPos = _position;
        var sb = new StringBuilder();
        
        while (_position < _input.Length && (char.IsLetterOrDigit(_input[_position]) || _input[_position] == '_'))
        {
            sb.Append(_input[_position]);
            _position++;
        }
        
        var identifier = sb.ToString();
        
        if (_constants.Contains(identifier))
        {
            return new Token(TokenType.Constant, identifier.ToLower(), startPos);
        }
        
        if (_keywords.TryGetValue(identifier, out var tokenType))
        {
            return new Token(tokenType, identifier.ToLower(), startPos);
        }
        
        return new Token(TokenType.Variable, identifier, startPos);
    }
    
    private void SkipWhitespace()
    {
        while (_position < _input.Length && char.IsWhiteSpace(_input[_position]))
        {
            _position++;
        }
    }
}

public class LexerException : Exception
{
    public LexerException(string message) : base(message) { }
}