using MathFlow.Core.ComplexMath;
using MathFlow.Core.Interfaces;
namespace MathFlow.Core.Expressions;
/// <summary>
/// Represents a complex number expression
/// </summary>
public class ComplexExpression : Expression
{
    public ComplexNumber Value { get; }
    
    public ComplexExpression(ComplexNumber value)
    {
        Value = value;
    }
    
    public ComplexExpression(double real, double imaginary = 0)
    {
        Value = new ComplexNumber(real, imaginary);
    }
    
    public override double Evaluate(Dictionary<string, double>? variables = null)
    {
        if (Math.Abs(Value.Imaginary) < 1e-10)
            return Value.Real;
        
        throw new InvalidOperationException($"Cannot evaluate complex number {this} to real value");
    }
    
    public ComplexNumber EvaluateComplex(Dictionary<string, ComplexNumber>? variables = null)
    {
        return Value;
    }
    
    public override IExpression Differentiate(string variable)
    {
        return new ConstantExpression(0);
    }
    
    public override IExpression Simplify()
    {
        if (Math.Abs(Value.Imaginary) < 1e-10)
            return new ConstantExpression(Value.Real);
        return this;
    }
    
    public override IExpression Substitute(string variable, IExpression replacement)
    {
        return this;
    }
    
    public override HashSet<string> GetVariables()
    {
        return new HashSet<string>();
    }
    
    public override bool IsConstant()
    {
        return true;
    }
    
    public override Expression Clone()
    {
        return new ComplexExpression(Value);
    }
    
    public override string ToString()
    {
        if (Math.Abs(Value.Imaginary) < 1e-10)
            return Value.Real.ToString();
            
        if (Math.Abs(Value.Real) < 1e-10)
        {
            if (Math.Abs(Value.Imaginary - 1) < 1e-10)
                return "i";
            if (Math.Abs(Value.Imaginary + 1) < 1e-10)
                return "-i";
            return $"{Value.Imaginary}i";
        }
        
        if (Value.Imaginary > 0)
        {
            if (Math.Abs(Value.Imaginary - 1) < 1e-10)
                return $"{Value.Real} + i";
            return $"{Value.Real} + {Value.Imaginary}i";
        }
        else
        {
            if (Math.Abs(Value.Imaginary + 1) < 1e-10)
                return $"{Value.Real} - i";
            return $"{Value.Real} - {Math.Abs(Value.Imaginary)}i";
        }
    }
}