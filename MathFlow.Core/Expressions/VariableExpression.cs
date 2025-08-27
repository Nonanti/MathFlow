using MathFlow.Core.Interfaces;
namespace MathFlow.Core.Expressions;
public class VariableExpression : Expression
{
    public string Name { get; }
    
    public VariableExpression(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Variable name cannot be null or empty", nameof(name));
        
        Name = name;
    }
    
    public override double Evaluate(Dictionary<string, double>? variables = null)
    {
        if (variables == null || !variables.TryGetValue(Name, out var value))
        {
            throw new InvalidOperationException($"Variable '{Name}' is not defined");
        }
        
        return value;
    }
    
    public override IExpression Simplify() => this;
    
    public override IExpression Differentiate(string variable)
    {
        return Name == variable ? new ConstantExpression(1) : new ConstantExpression(0);
    }
    
    public override IExpression Clone() => new VariableExpression(Name);
    
    public override HashSet<string> GetVariables() => new() { Name };
    
    public override bool IsConstant() => false;
    
    public override IExpression Substitute(string variable, IExpression value)
    {
        return Name == variable ? value.Clone() : this;
    }
    
    public override string ToString() => Name;
    
    public override bool Equals(object? obj)
    {
        return obj is VariableExpression other && Name == other.Name;
    }
    
    public override int GetHashCode() => Name.GetHashCode();
}