using MathFlow.Core.Interfaces;

namespace MathFlow.Core.Expressions;

public class FunctionExpression : Expression
{
    public string Name { get; }
    public List<IExpression> Arguments { get; }
    
    public FunctionExpression(string name, List<Expression> arguments)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Arguments = arguments?.Cast<IExpression>().ToList() ?? throw new ArgumentNullException(nameof(arguments));
    }
    
    public override double Evaluate(Dictionary<string, double>? variables = null)
    {
        var argValues = Arguments.Select(arg => arg.Evaluate(variables)).ToArray();
        
        return Name.ToLower() switch
        {
            "min" when argValues.Length == 2 => Math.Min(argValues[0], argValues[1]),
            "max" when argValues.Length == 2 => Math.Max(argValues[0], argValues[1]),
            "atan2" when argValues.Length == 2 => Math.Atan2(argValues[0], argValues[1]),
            "hypot" when argValues.Length == 2 => Math.Sqrt(argValues[0] * argValues[0] + argValues[1] * argValues[1]),
            "mod" when argValues.Length == 2 => argValues[0] % argValues[1],
            "gcd" when argValues.Length == 2 => GCD((long)argValues[0], (long)argValues[1]),
            "lcm" when argValues.Length == 2 => LCM((long)argValues[0], (long)argValues[1]),
            "factorial" when argValues.Length == 1 => Factorial(argValues[0]),
            _ => throw new NotSupportedException($"Function '{Name}' is not supported")
        };
    }
    
    private static long GCD(long a, long b)
    {
        a = Math.Abs(a);
        b = Math.Abs(b);
        
        while (b != 0)
        {
            var temp = b;
            b = a % b;
            a = temp;
        }
        
        return a;
    }
    
    private static long LCM(long a, long b)
    {
        return Math.Abs(a * b) / GCD(a, b);
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
        var simplifiedArgs = Arguments.Select(arg => arg.Simplify()).ToList();
        
        if (simplifiedArgs.All(arg => arg.IsConstant()))
        {
            return new ConstantExpression(Evaluate());
        }
        
        return new FunctionExpression(Name, simplifiedArgs.Cast<Expression>().ToList());
    }
    
    public override IExpression Differentiate(string variable)
    {
        switch (Name.ToLower())
        {
            case "min" when Arguments.Count == 2:
            case "max" when Arguments.Count == 2:
                throw new NotSupportedException($"Differentiation of {Name} is not supported (non-differentiable)");
                
            default:
                throw new NotSupportedException($"Differentiation of function '{Name}' is not supported");
        }
    }
    
    public override IExpression Clone()
    {
        return new FunctionExpression(Name, Arguments.Select(arg => ((Expression)arg.Clone())).ToList());
    }
    
    public override HashSet<string> GetVariables()
    {
        var vars = new HashSet<string>();
        foreach (var arg in Arguments)
        {
            vars.UnionWith(arg.GetVariables());
        }
        return vars;
    }
    
    public override bool IsConstant()
    {
        return Arguments.All(arg => arg.IsConstant());
    }
    
    public override IExpression Substitute(string variable, IExpression value)
    {
        var substitutedArgs = Arguments.Select(arg => (Expression)arg.Substitute(variable, value)).ToList();
        return new FunctionExpression(Name, substitutedArgs);
    }
    
    public override string ToString()
    {
        var argsStr = string.Join(", ", Arguments.Select(arg => arg.ToString()));
        return $"{Name}({argsStr})";
    }
}