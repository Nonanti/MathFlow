using MathFlow.Core.Interfaces;
namespace MathFlow.Core.Expressions;
public abstract class Expression : IExpression
{
    public abstract double Evaluate(Dictionary<string, double>? variables = null);
    public abstract IExpression Simplify();
    public abstract IExpression Differentiate(string variable);
    public abstract IExpression Clone();
    public abstract HashSet<string> GetVariables();
    public abstract bool IsConstant();
    public abstract IExpression Substitute(string variable, IExpression value);
    
    public override abstract string ToString();
    
    public static Expression Parse(string expression)
    {
        var parser = new Parser.ExpressionParser();
        return parser.Parse(expression);
    }
    
    public static Expression operator +(Expression left, Expression right)
        => new BinaryExpression(left, BinaryOperator.Add, right);
    
    public static Expression operator -(Expression left, Expression right)
        => new BinaryExpression(left, BinaryOperator.Subtract, right);
    
    public static Expression operator *(Expression left, Expression right)
        => new BinaryExpression(left, BinaryOperator.Multiply, right);
    
    public static Expression operator /(Expression left, Expression right)
        => new BinaryExpression(left, BinaryOperator.Divide, right);
    
    public static Expression operator ^(Expression left, Expression right)
        => new BinaryExpression(left, BinaryOperator.Power, right);
    
    public static Expression operator -(Expression expr)
        => new UnaryExpression(UnaryOperator.Negate, expr);
    
    public static implicit operator Expression(double value)
        => new ConstantExpression(value);
}