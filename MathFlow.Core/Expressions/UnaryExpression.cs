using MathFlow.Core.Interfaces;

namespace MathFlow.Core.Expressions;

public class UnaryExpression : Expression
{
    public UnaryOperator Operator { get; }
    public IExpression Operand { get; }
    
    public UnaryExpression(UnaryOperator op, IExpression operand)
    {
        Operator = op;
        Operand = operand ?? throw new ArgumentNullException(nameof(operand));
    }
    
    public override double Evaluate(Dictionary<string, double>? variables = null)
    {
        var value = Operand.Evaluate(variables);
        
        return Operator switch
        {
            UnaryOperator.Negate => -value,
            UnaryOperator.Sin => Math.Sin(value),
            UnaryOperator.Cos => Math.Cos(value),
            UnaryOperator.Tan => Math.Tan(value),
            UnaryOperator.Asin => Math.Asin(value),
            UnaryOperator.Acos => Math.Acos(value),
            UnaryOperator.Atan => Math.Atan(value),
            UnaryOperator.Sinh => Math.Sinh(value),
            UnaryOperator.Cosh => Math.Cosh(value),
            UnaryOperator.Tanh => Math.Tanh(value),
            UnaryOperator.Exp => Math.Exp(value),
            UnaryOperator.Ln => Math.Log(value),
            UnaryOperator.Log10 => Math.Log10(value),
            UnaryOperator.Sqrt => Math.Sqrt(value),
            UnaryOperator.Abs => Math.Abs(value),
            UnaryOperator.Floor => Math.Floor(value),
            UnaryOperator.Ceiling => Math.Ceiling(value),
            UnaryOperator.Round => Math.Round(value),
            UnaryOperator.Sign => Math.Sign(value),
            UnaryOperator.Factorial => Factorial(value),
            _ => throw new NotSupportedException($"Operator {Operator} is not supported")
        };
    }
    
    private static double Factorial(double n)
    {
        if (n < 0 || n != Math.Floor(n))
            throw new ArgumentException("Factorial is only defined for non-negative integers");
        
        if (n > 170)
            return double.PositiveInfinity;
        
        double result = 1;
        for (int i = 2; i <= (int)n; i++)
            result *= i;
        
        return result;
    }
    
    public override IExpression Simplify()
    {
        var operand = Operand.Simplify();
        
        if (operand.IsConstant())
        {
            return new ConstantExpression(Evaluate());
        }
        
        if (Operator == UnaryOperator.Negate && operand is UnaryExpression unary && unary.Operator == UnaryOperator.Negate)
        {
            return unary.Operand;
        }
        
        return new UnaryExpression(Operator, operand);
    }
    
    public override IExpression Differentiate(string variable)
    {
        var operandDiff = Operand.Differentiate(variable);
        
        IExpression derivative = Operator switch
        {
            UnaryOperator.Negate => new UnaryExpression(UnaryOperator.Negate, operandDiff),
            
            UnaryOperator.Sin => new BinaryExpression(
                new UnaryExpression(UnaryOperator.Cos, Operand),
                BinaryOperator.Multiply,
                operandDiff
            ),
            
            UnaryOperator.Cos => new UnaryExpression(
                UnaryOperator.Negate,
                new BinaryExpression(
                    new UnaryExpression(UnaryOperator.Sin, Operand),
                    BinaryOperator.Multiply,
                    operandDiff
                )
            ),
            
            UnaryOperator.Tan => new BinaryExpression(
                new BinaryExpression(
                    new ConstantExpression(1),
                    BinaryOperator.Add,
                    new BinaryExpression(
                        new UnaryExpression(UnaryOperator.Tan, Operand),
                        BinaryOperator.Power,
                        new ConstantExpression(2)
                    )
                ),
                BinaryOperator.Multiply,
                operandDiff
            ),
            
            UnaryOperator.Exp => new BinaryExpression(
                new UnaryExpression(UnaryOperator.Exp, Operand),
                BinaryOperator.Multiply,
                operandDiff
            ),
            
            UnaryOperator.Ln => new BinaryExpression(
                operandDiff,
                BinaryOperator.Divide,
                Operand
            ),
            
            UnaryOperator.Log10 => new BinaryExpression(
                operandDiff,
                BinaryOperator.Divide,
                new BinaryExpression(
                    Operand,
                    BinaryOperator.Multiply,
                    new UnaryExpression(UnaryOperator.Ln, new ConstantExpression(10))
                )
            ),
            
            UnaryOperator.Sqrt => new BinaryExpression(
                operandDiff,
                BinaryOperator.Divide,
                new BinaryExpression(
                    new ConstantExpression(2),
                    BinaryOperator.Multiply,
                    new UnaryExpression(UnaryOperator.Sqrt, Operand)
                )
            ),
            
            UnaryOperator.Sinh => new BinaryExpression(
                new UnaryExpression(UnaryOperator.Cosh, Operand),
                BinaryOperator.Multiply,
                operandDiff
            ),
            
            UnaryOperator.Cosh => new BinaryExpression(
                new UnaryExpression(UnaryOperator.Sinh, Operand),
                BinaryOperator.Multiply,
                operandDiff
            ),
            
            UnaryOperator.Tanh => new BinaryExpression(
                new BinaryExpression(
                    new ConstantExpression(1),
                    BinaryOperator.Subtract,
                    new BinaryExpression(
                        new UnaryExpression(UnaryOperator.Tanh, Operand),
                        BinaryOperator.Power,
                        new ConstantExpression(2)
                    )
                ),
                BinaryOperator.Multiply,
                operandDiff
            ),
            
            UnaryOperator.Asin => new BinaryExpression(
                operandDiff,
                BinaryOperator.Divide,
                new UnaryExpression(
                    UnaryOperator.Sqrt,
                    new BinaryExpression(
                        new ConstantExpression(1),
                        BinaryOperator.Subtract,
                        new BinaryExpression(Operand, BinaryOperator.Power, new ConstantExpression(2))
                    )
                )
            ),
            
            UnaryOperator.Acos => new UnaryExpression(
                UnaryOperator.Negate,
                new BinaryExpression(
                    operandDiff,
                    BinaryOperator.Divide,
                    new UnaryExpression(
                        UnaryOperator.Sqrt,
                        new BinaryExpression(
                            new ConstantExpression(1),
                            BinaryOperator.Subtract,
                            new BinaryExpression(Operand, BinaryOperator.Power, new ConstantExpression(2))
                        )
                    )
                )
            ),
            
            UnaryOperator.Atan => new BinaryExpression(
                operandDiff,
                BinaryOperator.Divide,
                new BinaryExpression(
                    new ConstantExpression(1),
                    BinaryOperator.Add,
                    new BinaryExpression(Operand, BinaryOperator.Power, new ConstantExpression(2))
                )
            ),
            
            UnaryOperator.Abs => new BinaryExpression(
                new UnaryExpression(UnaryOperator.Sign, Operand),
                BinaryOperator.Multiply,
                operandDiff
            ),
            
            _ => throw new NotSupportedException($"Differentiation of {Operator} is not supported")
        };
        
        return derivative.Simplify();
    }
    
    public override IExpression Clone()
    {
        return new UnaryExpression(Operator, Operand.Clone());
    }
    
    public override HashSet<string> GetVariables()
    {
        return Operand.GetVariables();
    }
    
    public override bool IsConstant()
    {
        return Operand.IsConstant();
    }
    
    public override IExpression Substitute(string variable, IExpression value)
    {
        return new UnaryExpression(Operator, Operand.Substitute(variable, value));
    }
    
    public override string ToString()
    {
        var operandStr = Operand.ToString();
        
        if (Operand is BinaryExpression && Operator != UnaryOperator.Negate)
        {
            operandStr = $"({operandStr})";
        }
        
        return Operator switch
        {
            UnaryOperator.Negate => $"-{operandStr}",
            UnaryOperator.Sin => $"sin({operandStr})",
            UnaryOperator.Cos => $"cos({operandStr})",
            UnaryOperator.Tan => $"tan({operandStr})",
            UnaryOperator.Asin => $"asin({operandStr})",
            UnaryOperator.Acos => $"acos({operandStr})",
            UnaryOperator.Atan => $"atan({operandStr})",
            UnaryOperator.Sinh => $"sinh({operandStr})",
            UnaryOperator.Cosh => $"cosh({operandStr})",
            UnaryOperator.Tanh => $"tanh({operandStr})",
            UnaryOperator.Exp => $"exp({operandStr})",
            UnaryOperator.Ln => $"ln({operandStr})",
            UnaryOperator.Log10 => $"log10({operandStr})",
            UnaryOperator.Sqrt => $"sqrt({operandStr})",
            UnaryOperator.Abs => $"abs({operandStr})",
            UnaryOperator.Floor => $"floor({operandStr})",
            UnaryOperator.Ceiling => $"ceil({operandStr})",
            UnaryOperator.Round => $"round({operandStr})",
            UnaryOperator.Sign => $"sign({operandStr})",
            UnaryOperator.Factorial => $"{operandStr}!",
            _ => $"?({operandStr})"
        };
    }
}