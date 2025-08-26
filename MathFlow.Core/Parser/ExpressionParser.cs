using System.Globalization;
using MathFlow.Core.Expressions;
using MathFlow.Core.Interfaces;

namespace MathFlow.Core.Parser;

public class ExpressionParser
{
    private List<Token> _tokens = new();
    private int _currentIndex;
    
    public Expression Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be null or empty", nameof(input));
        
        var lexer = new Lexer(input);
        _tokens = lexer.Tokenize();
        _currentIndex = 0;
        
        var result = ParseExpression();
        
        if (CurrentToken.Type != TokenType.End)
        {
            throw new ParserException($"Syntax error near '{CurrentToken.Value}'");
        }
        
        return result;
    }
    
    private Token CurrentToken => _tokens[_currentIndex];
    
    private Token PeekToken(int offset = 1)
    {
        var index = _currentIndex + offset;
        return index < _tokens.Count ? _tokens[index] : _tokens[^1];
    }
    
    private void Advance()
    {
        if (_currentIndex < _tokens.Count - 1)
            _currentIndex++;
    }
    
    private Expression ParseExpression()
    {
        return ParseAdditive();
    }
    
    private Expression ParseAdditive()
    {
        var left = ParseMultiplicative();
        
        while (CurrentToken.Type == TokenType.Plus || CurrentToken.Type == TokenType.Minus)
        {
            var op = CurrentToken.Type == TokenType.Plus ? BinaryOperator.Add : BinaryOperator.Subtract;
            Advance();
            
            if (CurrentToken.Type == TokenType.Plus || CurrentToken.Type == TokenType.Multiply || 
                CurrentToken.Type == TokenType.Divide || CurrentToken.Type == TokenType.End || 
                CurrentToken.Type == TokenType.RightParen || CurrentToken.Type == TokenType.Comma)
            {
                throw new ParserException($"Syntax error: '{CurrentToken.Value}'");
            }
            
            var right = ParseMultiplicative();
            left = new BinaryExpression(left, op, right);
        }
        
        return left;
    }
    
    private Expression ParseMultiplicative()
    {
        var left = ParsePower();
        
        while (CurrentToken.Type == TokenType.Multiply || CurrentToken.Type == TokenType.Divide || CurrentToken.Type == TokenType.Modulo)
        {
            var op = CurrentToken.Type switch
            {
                TokenType.Multiply => BinaryOperator.Multiply,
                TokenType.Divide => BinaryOperator.Divide,
                TokenType.Modulo => BinaryOperator.Modulo,
                _ => throw new ParserException($"Unexpected token {CurrentToken.Type}")
            };
            
            Advance();
            var right = ParsePower();
            left = new BinaryExpression(left, op, right);
        }
        
        return left;
    }
    
    private Expression ParsePower()
    {
        var left = ParseUnary();
        
        if (CurrentToken.Type == TokenType.Power)
        {
            Advance();
            var right = ParsePower(); // Right associative
            return new BinaryExpression(left, BinaryOperator.Power, right);
        }
        
        return left;
    }
    
    private Expression ParseUnary()
    {
        if (CurrentToken.Type == TokenType.Minus)
        {
            Advance();
            return new UnaryExpression(UnaryOperator.Negate, ParseUnary());
        }
        
        if (CurrentToken.Type == TokenType.Plus)
        {
            Advance();
            return ParseUnary();
        }
        
        return ParsePostfix();
    }
    
    private Expression ParsePostfix()
    {
        var expr = ParsePrimary();
        
        while (CurrentToken.Type == TokenType.Factorial)
        {
            Advance();
            expr = new UnaryExpression(UnaryOperator.Factorial, expr);
        }
        
        return expr;
    }
    
    private Expression ParsePrimary()
    {
        switch (CurrentToken.Type)
        {
            case TokenType.Number:
                return ParseNumber();
                
            case TokenType.Variable:
                return ParseVariable();
                
            case TokenType.Constant:
                return ParseConstant();
                
            case TokenType.Function:
                return ParseFunction();
                
            case TokenType.LeftParen:
                return ParseParenthesized();
                
            default:
                throw new ParserException($"Syntax error near '{CurrentToken.Value}'");
        }
    }
    
    private Expression ParseNumber()
    {
        var value = double.Parse(CurrentToken.Value, CultureInfo.InvariantCulture);
        Advance();
        return new ConstantExpression(value);
    }
    
    private Expression ParseVariable()
    {
        var name = CurrentToken.Value;
        Advance();
        
        if (CurrentToken.Type == TokenType.LeftParen)
        {
            //hack: convert variable to function call
            _currentIndex--;
            _tokens[_currentIndex] = new Token(TokenType.Function, name, _tokens[_currentIndex].Position);
            return ParseFunction();
        }
        
        return new VariableExpression(name);
    }
    
    private Expression ParseConstant()
    {
        var constant = CurrentToken.Value.ToLower();
        Advance();
        
        var value = constant switch
        {
            "pi" or "π" => Math.PI,
            "e" => Math.E,
            "tau" or "τ" => 2 * Math.PI,
            "phi" or "φ" => (1 + Math.Sqrt(5)) / 2,
            _ => throw new ParserException($"Unknown constant '{constant}'")
        };
        
        return new ConstantExpression(value);
    }
    
    private Expression ParseFunction()
    {
        var functionName = CurrentToken.Value.ToLower();
        Advance();
        
        if (CurrentToken.Type != TokenType.LeftParen)
        {
            throw new ParserException($"Missing '(' after '{functionName}'");
        }
        
        Advance();
        
        var arguments = new List<Expression>();
        
        if (CurrentToken.Type != TokenType.RightParen)
        {
            arguments.Add(ParseExpression());
            
            while (CurrentToken.Type == TokenType.Comma)
            {
                Advance();
                arguments.Add(ParseExpression());
            }
        }
        
        if (CurrentToken.Type != TokenType.RightParen)
        {
            throw new ParserException("Missing closing parenthesis");
        }
        
        Advance();
        
        return CreateFunctionExpression(functionName, arguments);
    }
    
    private Expression CreateFunctionExpression(string functionName, List<Expression> arguments)
    {
        switch (functionName)
        {
            case "sin" when arguments.Count == 1:
                return new UnaryExpression(UnaryOperator.Sin, arguments[0]);
                
            case "cos" when arguments.Count == 1:
                return new UnaryExpression(UnaryOperator.Cos, arguments[0]);
                
            case "tan" when arguments.Count == 1:
                return new UnaryExpression(UnaryOperator.Tan, arguments[0]);
                
            case "asin" or "arcsin" when arguments.Count == 1:
                return new UnaryExpression(UnaryOperator.Asin, arguments[0]);
                
            case "acos" or "arccos" when arguments.Count == 1:
                return new UnaryExpression(UnaryOperator.Acos, arguments[0]);
                
            case "atan" or "arctan" when arguments.Count == 1:
                return new UnaryExpression(UnaryOperator.Atan, arguments[0]);
                
            case "sinh" when arguments.Count == 1:
                return new UnaryExpression(UnaryOperator.Sinh, arguments[0]);
                
            case "cosh" when arguments.Count == 1:
                return new UnaryExpression(UnaryOperator.Cosh, arguments[0]);
                
            case "tanh" when arguments.Count == 1:
                return new UnaryExpression(UnaryOperator.Tanh, arguments[0]);
                
            case "exp" when arguments.Count == 1:
                return new UnaryExpression(UnaryOperator.Exp, arguments[0]);
                
            case "ln" when arguments.Count == 1:
                return new UnaryExpression(UnaryOperator.Ln, arguments[0]);
                
            case "log10" when arguments.Count == 1:
                return new UnaryExpression(UnaryOperator.Log10, arguments[0]);
                
            case "log" when arguments.Count == 1:
                return new UnaryExpression(UnaryOperator.Ln, arguments[0]);
                
            case "log" when arguments.Count == 2:
                return new BinaryExpression(arguments[0], BinaryOperator.LogBase, arguments[1]);
                
            case "sqrt" when arguments.Count == 1:
                return new UnaryExpression(UnaryOperator.Sqrt, arguments[0]);
                
            case "abs" when arguments.Count == 1:
                return new UnaryExpression(UnaryOperator.Abs, arguments[0]);
                
            case "floor" when arguments.Count == 1:
                return new UnaryExpression(UnaryOperator.Floor, arguments[0]);
                
            case "ceil" or "ceiling" when arguments.Count == 1:
                return new UnaryExpression(UnaryOperator.Ceiling, arguments[0]);
                
            case "round" when arguments.Count == 1:
                return new UnaryExpression(UnaryOperator.Round, arguments[0]);
                
            case "sign" when arguments.Count == 1:
                return new UnaryExpression(UnaryOperator.Sign, arguments[0]);
                
            case "pow" when arguments.Count == 2:
                return new BinaryExpression(arguments[0], BinaryOperator.Power, arguments[1]);
                
            case "min" when arguments.Count == 2:
                return new FunctionExpression("min", arguments);
                
            case "max" when arguments.Count == 2:
                return new FunctionExpression("max", arguments);
                
            default:
                if (arguments.Count == 0)
                    throw new ParserException($"Function '{functionName}' requires arguments");
                
                return new FunctionExpression(functionName, arguments);
        }
    }
    
    private Expression ParseParenthesized()
    {
        Advance(); // Skip '('
        var expr = ParseExpression();
        
        if (CurrentToken.Type != TokenType.RightParen)
        {
            throw new ParserException("Missing closing parenthesis");
        }
        
        Advance();
        return expr;
    }
}

public class ParserException : Exception
{
    public ParserException(string message) : base(message) { }
}