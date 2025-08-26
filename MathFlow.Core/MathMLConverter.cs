using System.Xml.Linq;
using MathFlow.Core.Expressions;
using MathFlow.Core.Interfaces;

namespace MathFlow.Core;

public static class MathMLConverter
{
    private static readonly XNamespace MathMLNamespace = "http://www.w3.org/1998/Math/MathML";
    
    public static string ToMathML(IExpression expression)
    {
        var mathElement = new XElement(MathMLNamespace + "math",
            new XAttribute("xmlns", MathMLNamespace.ToString()),
            ConvertExpression(expression)
        );
        
        return mathElement.ToString();
    }
    
    private static XElement ConvertExpression(IExpression expression)
    {
        return expression switch
        {
            ConstantExpression constant => ConvertConstant(constant),
            VariableExpression variable => ConvertVariable(variable),
            BinaryExpression binary => ConvertBinary(binary),
            UnaryExpression unary => ConvertUnary(unary),
            FunctionExpression function => ConvertFunction(function),
            _ => new XElement(MathMLNamespace + "mtext", expression.ToString())
        };
    }
    
    private static XElement ConvertConstant(ConstantExpression constant)
    {
        if (double.IsNaN(constant.Value))
            return new XElement(MathMLNamespace + "mtext", "NaN");
        
        if (double.IsPositiveInfinity(constant.Value))
            return new XElement(MathMLNamespace + "mi", "∞");
        
        if (double.IsNegativeInfinity(constant.Value))
            return new XElement(MathMLNamespace + "mrow",
                new XElement(MathMLNamespace + "mo", "-"),
                new XElement(MathMLNamespace + "mi", "∞")
            );
        
        if (Math.Abs(constant.Value - Math.PI) < 1e-10)
            return new XElement(MathMLNamespace + "mi", "π");
        
        if (Math.Abs(constant.Value - Math.E) < 1e-10)
            return new XElement(MathMLNamespace + "mi", "e");
        
        return new XElement(MathMLNamespace + "mn", constant.Value.ToString("G"));
    }
    
    private static XElement ConvertVariable(VariableExpression variable)
    {
        return new XElement(MathMLNamespace + "mi", variable.Name);
    }
    
    private static XElement ConvertBinary(BinaryExpression binary)
    {
        switch (binary.Operator)
        {
            case BinaryOperator.Divide:
                return new XElement(MathMLNamespace + "mfrac",
                    ConvertExpression(binary.Left),
                    ConvertExpression(binary.Right)
                );
                
            case BinaryOperator.Power:
                return new XElement(MathMLNamespace + "msup",
                    WrapIfNeeded(binary.Left),
                    ConvertExpression(binary.Right)
                );
                
            case BinaryOperator.LogBase:
                return new XElement(MathMLNamespace + "mrow",
                    new XElement(MathMLNamespace + "msub",
                        new XElement(MathMLNamespace + "mi", "log"),
                        ConvertExpression(binary.Right)
                    ),
                    new XElement(MathMLNamespace + "mo", "⁡"),
                    new XElement(MathMLNamespace + "mo", "("),
                    ConvertExpression(binary.Left),
                    new XElement(MathMLNamespace + "mo", ")")
                );
                
            default:
                var opSymbol = binary.Operator switch
                {
                    BinaryOperator.Add => "+",
                    BinaryOperator.Subtract => "-",
                    BinaryOperator.Multiply => "·",
                    BinaryOperator.Modulo => "mod",
                    _ => "?"
                };
                
                return new XElement(MathMLNamespace + "mrow",
                    WrapIfNeeded(binary.Left, binary.Operator),
                    new XElement(MathMLNamespace + "mo", opSymbol),
                    WrapIfNeeded(binary.Right, binary.Operator)
                );
        }
    }
    
    private static XElement ConvertUnary(UnaryExpression unary)
    {
        switch (unary.Operator)
        {
            case UnaryOperator.Negate:
                return new XElement(MathMLNamespace + "mrow",
                    new XElement(MathMLNamespace + "mo", "-"),
                    WrapIfNeeded(unary.Operand)
                );
                
            case UnaryOperator.Sqrt:
                return new XElement(MathMLNamespace + "msqrt",
                    ConvertExpression(unary.Operand)
                );
                
            case UnaryOperator.Abs:
                return new XElement(MathMLNamespace + "mrow",
                    new XElement(MathMLNamespace + "mo", "|"),
                    ConvertExpression(unary.Operand),
                    new XElement(MathMLNamespace + "mo", "|")
                );
                
            case UnaryOperator.Floor:
                return new XElement(MathMLNamespace + "mrow",
                    new XElement(MathMLNamespace + "mo", "⌊"),
                    ConvertExpression(unary.Operand),
                    new XElement(MathMLNamespace + "mo", "⌋")
                );
                
            case UnaryOperator.Ceiling:
                return new XElement(MathMLNamespace + "mrow",
                    new XElement(MathMLNamespace + "mo", "⌈"),
                    ConvertExpression(unary.Operand),
                    new XElement(MathMLNamespace + "mo", "⌉")
                );
                
            case UnaryOperator.Factorial:
                return new XElement(MathMLNamespace + "mrow",
                    WrapIfNeeded(unary.Operand),
                    new XElement(MathMLNamespace + "mo", "!")
                );
                
            case UnaryOperator.Exp:
                return new XElement(MathMLNamespace + "msup",
                    new XElement(MathMLNamespace + "mi", "e"),
                    ConvertExpression(unary.Operand)
                );
                
            default:
                var funcName = unary.Operator switch
                {
                    UnaryOperator.Sin => "sin",
                    UnaryOperator.Cos => "cos",
                    UnaryOperator.Tan => "tan",
                    UnaryOperator.Asin => "arcsin",
                    UnaryOperator.Acos => "arccos",
                    UnaryOperator.Atan => "arctan",
                    UnaryOperator.Sinh => "sinh",
                    UnaryOperator.Cosh => "cosh",
                    UnaryOperator.Tanh => "tanh",
                    UnaryOperator.Ln => "ln",
                    UnaryOperator.Log10 => "log₁₀",
                    UnaryOperator.Round => "round",
                    UnaryOperator.Sign => "sgn",
                    _ => "?"
                };
                
                return new XElement(MathMLNamespace + "mrow",
                    new XElement(MathMLNamespace + "mi", funcName),
                    new XElement(MathMLNamespace + "mo", "⁡"),
                    new XElement(MathMLNamespace + "mo", "("),
                    ConvertExpression(unary.Operand),
                    new XElement(MathMLNamespace + "mo", ")")
                );
        }
    }
    
    private static XElement ConvertFunction(FunctionExpression function)
    {
        var args = new List<XElement>();
        
        for (int i = 0; i < function.Arguments.Count; i++)
        {
            if (i > 0)
            {
                args.Add(new XElement(MathMLNamespace + "mo", ","));
            }
            args.Add(ConvertExpression(function.Arguments[i]));
        }
        
        return new XElement(MathMLNamespace + "mrow",
            new XElement(MathMLNamespace + "mi", function.Name),
            new XElement(MathMLNamespace + "mo", "⁡"),
            new XElement(MathMLNamespace + "mo", "("),
            args,
            new XElement(MathMLNamespace + "mo", ")")
        );
    }
    
    private static XElement WrapIfNeeded(IExpression expr, BinaryOperator? parentOp = null)
    {
        var needsWrap = false;
        
        if (expr is BinaryExpression binary)
        {
            if (parentOp == BinaryOperator.Multiply || parentOp == BinaryOperator.Divide)
            {
                needsWrap = binary.Operator == BinaryOperator.Add || binary.Operator == BinaryOperator.Subtract;
            }
            else if (parentOp == BinaryOperator.Power)
            {
                needsWrap = true;
            }
        }
        else if (expr is UnaryExpression unary && unary.Operator == UnaryOperator.Negate)
        {
            needsWrap = parentOp == BinaryOperator.Power;
        }
        
        if (needsWrap)
        {
            return new XElement(MathMLNamespace + "mrow",
                new XElement(MathMLNamespace + "mo", "("),
                ConvertExpression(expr),
                new XElement(MathMLNamespace + "mo", ")")
            );
        }
        
        return ConvertExpression(expr);
    }
}