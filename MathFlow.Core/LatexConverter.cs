using MathFlow.Core.Expressions;
using MathFlow.Core.Interfaces;
namespace MathFlow.Core;
public static class LatexConverter
{
    public static string ToLatex(IExpression expression)
    {
        return expression switch
        {
            ConstantExpression constant => FormatConstant(constant),
            VariableExpression variable => variable.Name,
            BinaryExpression binary => FormatBinary(binary),
            UnaryExpression unary => FormatUnary(unary),
            FunctionExpression function => FormatFunction(function),
            _ => expression.ToString()
        };
    }
    
    private static string FormatConstant(ConstantExpression constant)
    {
        if (double.IsNaN(constant.Value)) return @"\text{NaN}";
        if (double.IsPositiveInfinity(constant.Value)) return @"\infty";
        if (double.IsNegativeInfinity(constant.Value)) return @"-\infty";
        
        if (Math.Abs(constant.Value - Math.PI) < 1e-10) return @"\pi";
        if (Math.Abs(constant.Value - Math.E) < 1e-10) return "e";
        if (Math.Abs(constant.Value - 2 * Math.PI) < 1e-10) return @"\tau";
        if (Math.Abs(constant.Value - (1 + Math.Sqrt(5)) / 2) < 1e-10) return @"\phi";
        
        return constant.Value.ToString("G");
    }
    
    private static string FormatBinary(BinaryExpression binary)
    {
        var left = ToLatex(binary.Left);
        var right = ToLatex(binary.Right);
        
        switch (binary.Operator)
        {
            case BinaryOperator.Add:
                return $"{left} + {right}";
                
            case BinaryOperator.Subtract:
                if (NeedsParentheses(binary.Right, binary.Operator))
                    right = $"\\left({right}\\right)";
                return $"{left} - {right}";
                
            case BinaryOperator.Multiply:
                if (NeedsParentheses(binary.Left, binary.Operator))
                    left = $"\\left({left}\\right)";
                if (NeedsParentheses(binary.Right, binary.Operator))
                    right = $"\\left({right}\\right)";
                return $"{left} \\cdot {right}";
                
            case BinaryOperator.Divide:
                return $"\\frac{{{left}}}{{{right}}}";
                
            case BinaryOperator.Power:
                if (binary.Left is UnaryExpression unary && IsTrigFunction(unary.Operator))
                {
                    var funcName = GetTrigLatexName(unary.Operator);
                    var operand = ToLatex(unary.Operand);
                    return $"{funcName}^{{{right}}}\\left({operand}\\right)";
                }
                if (NeedsParentheses(binary.Left, binary.Operator))
                    left = $"\\left({left}\\right)";
                return $"{left}^{{{right}}}";
                
            case BinaryOperator.Modulo:
                return $"{left} \\bmod {right}";
                
            case BinaryOperator.LogBase:
                return $"\\log_{{{right}}}\\left({left}\\right)";
                
            default:
                return $"{left} ? {right}";
        }
    }
    
    private static string FormatUnary(UnaryExpression unary)
    {
        var operand = ToLatex(unary.Operand);
        
        return unary.Operator switch
        {
            UnaryOperator.Negate => $"-{operand}",
            UnaryOperator.Sin => $"\\sin\\left({operand}\\right)",
            UnaryOperator.Cos => $"\\cos\\left({operand}\\right)",
            UnaryOperator.Tan => $"\\tan\\left({operand}\\right)",
            UnaryOperator.Asin => $"\\arcsin\\left({operand}\\right)",
            UnaryOperator.Acos => $"\\arccos\\left({operand}\\right)",
            UnaryOperator.Atan => $"\\arctan\\left({operand}\\right)",
            UnaryOperator.Sinh => $"\\sinh\\left({operand}\\right)",
            UnaryOperator.Cosh => $"\\cosh\\left({operand}\\right)",
            UnaryOperator.Tanh => $"\\tanh\\left({operand}\\right)",
            UnaryOperator.Exp => $"e^{{{operand}}}",
            UnaryOperator.Ln => $"\\ln\\left({operand}\\right)",
            UnaryOperator.Log10 => $"\\log_{10}\\left({operand}\\right)",
            UnaryOperator.Sqrt => $"\\sqrt{{{operand}}}",
            UnaryOperator.Abs => $"\\left|{operand}\\right|",
            UnaryOperator.Floor => $"\\lfloor {operand} \\rfloor",
            UnaryOperator.Ceiling => $"\\lceil {operand} \\rceil",
            UnaryOperator.Round => $"\\text{{round}}\\left({operand}\\right)",
            UnaryOperator.Sign => $"\\text{{sgn}}\\left({operand}\\right)",
            UnaryOperator.Factorial => $"{operand}!",
            _ => $"?\\left({operand}\\right)"
        };
    }
    
    private static string FormatFunction(FunctionExpression function)
    {
        var args = string.Join(", ", function.Arguments.Select(ToLatex));
        
        return function.Name.ToLower() switch
        {
            "min" => $"\\min\\left({args}\\right)",
            "max" => $"\\max\\left({args}\\right)",
            "gcd" => $"\\gcd\\left({args}\\right)",
            "lcm" => $"\\text{{lcm}}\\left({args}\\right)",
            _ => $"\\text{{{function.Name}}}\\left({args}\\right)"
        };
    }
    
    private static bool NeedsParentheses(IExpression expr, BinaryOperator parentOp)
    {
        if (expr is BinaryExpression binary)
        {
            return parentOp switch
            {
                BinaryOperator.Multiply or BinaryOperator.Divide => 
                    binary.Operator == BinaryOperator.Add || binary.Operator == BinaryOperator.Subtract,
                BinaryOperator.Power => true,
                BinaryOperator.Subtract => 
                    binary.Operator == BinaryOperator.Add || binary.Operator == BinaryOperator.Subtract,
                _ => false
            };
        }
        
        return expr is UnaryExpression unary && unary.Operator == UnaryOperator.Negate && parentOp == BinaryOperator.Power;
    }
    
    private static bool IsTrigFunction(UnaryOperator op)
    {
        return op == UnaryOperator.Sin || op == UnaryOperator.Cos || op == UnaryOperator.Tan ||
               op == UnaryOperator.Sinh || op == UnaryOperator.Cosh || op == UnaryOperator.Tanh ||
               op == UnaryOperator.Asin || op == UnaryOperator.Acos || op == UnaryOperator.Atan;
    }
    
    private static string GetTrigLatexName(UnaryOperator op)
    {
        return op switch
        {
            UnaryOperator.Sin => "\\sin",
            UnaryOperator.Cos => "\\cos",
            UnaryOperator.Tan => "\\tan",
            UnaryOperator.Asin => "\\arcsin",
            UnaryOperator.Acos => "\\arccos",
            UnaryOperator.Atan => "\\arctan",
            UnaryOperator.Sinh => "\\sinh",
            UnaryOperator.Cosh => "\\cosh",
            UnaryOperator.Tanh => "\\tanh",
            _ => ""
        };
    }
}