using MathFlow.Core.Interfaces;
namespace MathFlow.Core.Expressions;
public class ConstantExpression : Expression
{
    public double Value { get; }
    
    public ConstantExpression(double value)
    {
        Value = value;
    }
    
    public override double Evaluate(Dictionary<string, double>? variables = null) => Value;
    
    public override IExpression Simplify() => this;
    
    public override IExpression Differentiate(string variable) => new ConstantExpression(0);
    
    public override IExpression Clone() => new ConstantExpression(Value);
    
    public override HashSet<string> GetVariables() => new();
    
    public override bool IsConstant() => true;
    
    public override IExpression Substitute(string variable, IExpression value) => this;
    
    public override string ToString()
    {
        if (double.IsNaN(Value)) return "NaN";
        if (double.IsPositiveInfinity(Value)) return "∞";
        if (double.IsNegativeInfinity(Value)) return "-∞";
        
        if (Math.Abs(Value - Math.PI) < 1e-10) return "π";
        if (Math.Abs(Value - Math.E) < 1e-10) return "e";
        
        return Value.ToString("G");
    }
    
    public override bool Equals(object? obj)
    {
        return obj is ConstantExpression other && Math.Abs(Value - other.Value) < 1e-10;
    }
    
    public override int GetHashCode() => Value.GetHashCode();
}