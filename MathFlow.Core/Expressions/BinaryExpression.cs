using MathFlow.Core.Interfaces;

namespace MathFlow.Core.Expressions;

public class BinaryExpression : Expression
{
    public IExpression Left { get; }
    public IExpression Right { get; }
    public BinaryOperator Operator { get; }
    
    public BinaryExpression(IExpression left, BinaryOperator op, IExpression right)
    {
        Left = left ?? throw new ArgumentNullException(nameof(left));
        Right = right ?? throw new ArgumentNullException(nameof(right));
        Operator = op;
    }
    
    public override double Evaluate(Dictionary<string, double>? variables = null)
    {
        var leftValue = Left.Evaluate(variables);
        var rightValue = Right.Evaluate(variables);
        
        return Operator switch
        {
            BinaryOperator.Add => leftValue + rightValue,
            BinaryOperator.Subtract => leftValue - rightValue,
            BinaryOperator.Multiply => leftValue * rightValue,
            BinaryOperator.Divide => leftValue / rightValue,
            BinaryOperator.Power => Math.Pow(leftValue, rightValue),
            BinaryOperator.Modulo => leftValue % rightValue,
            BinaryOperator.LogBase => Math.Log(leftValue, rightValue),
            _ => throw new NotSupportedException($"Operator {Operator} is not supported")
        };
    }
    
    public override IExpression Simplify()
    {
        var left = Left.Simplify();
        var right = Right.Simplify();
        
        if (left.IsConstant() && right.IsConstant())
        {
            try
            {
                return new ConstantExpression(Evaluate());
            }
            catch
            {
                // constants might contain undefined variables
            }
        }
        
        switch (Operator)
        {
            case BinaryOperator.Add:
                if (IsZero(left)) return right;
                if (IsZero(right)) return left;
                if (left.Equals(right)) return new BinaryExpression(new ConstantExpression(2), BinaryOperator.Multiply, left);
                return SimplificationHelper.SimplifyAddition(left, right);
                
            case BinaryOperator.Subtract:
                if (IsZero(right)) return left;
                if (left.Equals(right)) return new ConstantExpression(0);
                break;
                
            case BinaryOperator.Multiply:
                if (IsZero(left) || IsZero(right)) return new ConstantExpression(0);
                if (IsOne(left)) return right;
                if (IsOne(right)) return left;
                return SimplificationHelper.SimplifyMultiplication(left, right);
                
            case BinaryOperator.Divide:
                if (IsZero(left)) return new ConstantExpression(0);
                if (IsOne(right)) return left;
                if (left.Equals(right)) return new ConstantExpression(1);
                break;
                
            case BinaryOperator.Power:
                if (IsZero(right)) return new ConstantExpression(1);
                if (IsOne(right)) return left;
                if (IsOne(left)) return new ConstantExpression(1);
                if (IsZero(left)) return new ConstantExpression(0);
                break;
        }
        
        return new BinaryExpression(left, Operator, right);
    }
    
    public override IExpression Differentiate(string variable)
    {
        var leftDiff = Left.Differentiate(variable);
        var rightDiff = Right.Differentiate(variable);
        
        switch (Operator)
        {
            case BinaryOperator.Add:
                return new BinaryExpression(leftDiff, BinaryOperator.Add, rightDiff).Simplify();
                
            case BinaryOperator.Subtract:
                return new BinaryExpression(leftDiff, BinaryOperator.Subtract, rightDiff).Simplify();
                
            case BinaryOperator.Multiply:
                var term1 = new BinaryExpression(leftDiff, BinaryOperator.Multiply, Right);
                var term2 = new BinaryExpression(Left, BinaryOperator.Multiply, rightDiff);
                return new BinaryExpression(term1, BinaryOperator.Add, term2).Simplify();
                
            case BinaryOperator.Divide:
                var numerator = new BinaryExpression(
                    new BinaryExpression(leftDiff, BinaryOperator.Multiply, Right),
                    BinaryOperator.Subtract,
                    new BinaryExpression(Left, BinaryOperator.Multiply, rightDiff)
                );
                var denominator = new BinaryExpression(Right, BinaryOperator.Power, new ConstantExpression(2));
                return new BinaryExpression(numerator, BinaryOperator.Divide, denominator).Simplify();
                
            case BinaryOperator.Power:
                if (Right.IsConstant())
                {
                    var n = Right.Evaluate();
                    var coefficient = new BinaryExpression(new ConstantExpression(n), BinaryOperator.Multiply, 
                        new BinaryExpression(Left, BinaryOperator.Power, new ConstantExpression(n - 1)));
                    return new BinaryExpression(coefficient, BinaryOperator.Multiply, leftDiff).Simplify();
                }
                else
                {
                    var ln = new UnaryExpression(UnaryOperator.Ln, Left);
                    var powerTerm1 = new BinaryExpression(rightDiff, BinaryOperator.Multiply, ln);
                    var powerTerm2 = new BinaryExpression(Right, BinaryOperator.Multiply, 
                        new BinaryExpression(leftDiff, BinaryOperator.Divide, Left));
                    var derivative = new BinaryExpression(powerTerm1, BinaryOperator.Add, powerTerm2);
                    return new BinaryExpression(this, BinaryOperator.Multiply, derivative).Simplify();
                }
                
            default:
                throw new NotSupportedException($"Differentiation of {Operator} is not supported");
        }
    }
    
    public override IExpression Clone()
    {
        return new BinaryExpression(Left.Clone(), Operator, Right.Clone());
    }
    
    public override HashSet<string> GetVariables()
    {
        var vars = Left.GetVariables();
        vars.UnionWith(Right.GetVariables());
        return vars;
    }
    
    public override bool IsConstant()
    {
        return Left.IsConstant() && Right.IsConstant();
    }
    
    public override IExpression Substitute(string variable, IExpression value)
    {
        return new BinaryExpression(
            Left.Substitute(variable, value),
            Operator,
            Right.Substitute(variable, value)
        );
    }
    
    public override string ToString()
    {
        var leftStr = WrapIfNeeded(Left, true);
        var rightStr = WrapIfNeeded(Right, false);
        
        var opStr = Operator switch
        {
            BinaryOperator.Add => " + ",
            BinaryOperator.Subtract => " - ",
            BinaryOperator.Multiply => " * ",
            BinaryOperator.Divide => " / ",
            BinaryOperator.Power => "^",
            BinaryOperator.Modulo => " % ",
            BinaryOperator.LogBase => " log ",
            _ => " ? "
        };
        
        return $"{leftStr}{opStr}{rightStr}";
    }
    
    private string WrapIfNeeded(IExpression expr, bool isLeft)
    {
        if (expr is BinaryExpression binExpr)
        {
            var needsParens = Operator switch
            {
                BinaryOperator.Multiply or BinaryOperator.Divide => 
                    binExpr.Operator == BinaryOperator.Add || binExpr.Operator == BinaryOperator.Subtract,
                BinaryOperator.Power => !isLeft,
                BinaryOperator.Subtract => !isLeft && (binExpr.Operator == BinaryOperator.Add || binExpr.Operator == BinaryOperator.Subtract),
                _ => false
            };
            
            return needsParens ? $"({expr})" : expr.ToString();
        }
        
        return expr.ToString();
    }
    
    private static bool IsZero(IExpression expr)
    {
        // checking if close enough to 0
        return expr is ConstantExpression c && Math.Abs(c.Value) < 1e-10;
    }
    
    private static bool IsOne(IExpression expr)
    {
        return expr is ConstantExpression c && Math.Abs(c.Value - 1) < 1e-10;  //approx 1
    }
}