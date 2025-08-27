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
            // Trigonometric
            "sin" when argValues.Length == 1 => Math.Sin(argValues[0]),
            "cos" when argValues.Length == 1 => Math.Cos(argValues[0]),
            "tan" when argValues.Length == 1 => Math.Tan(argValues[0]),
            "asin" when argValues.Length == 1 => Math.Asin(argValues[0]),
            "acos" when argValues.Length == 1 => Math.Acos(argValues[0]),
            "atan" when argValues.Length == 1 => Math.Atan(argValues[0]),
            "sec" when argValues.Length == 1 => 1 / Math.Cos(argValues[0]),
            "csc" when argValues.Length == 1 => 1 / Math.Sin(argValues[0]),
            "cot" when argValues.Length == 1 => 1 / Math.Tan(argValues[0]),
            
            // Hyperbolic
            "sinh" when argValues.Length == 1 => Math.Sinh(argValues[0]),
            "cosh" when argValues.Length == 1 => Math.Cosh(argValues[0]),
            "tanh" when argValues.Length == 1 => Math.Tanh(argValues[0]),
            
            // Exponential and logarithmic
            "exp" when argValues.Length == 1 => Math.Exp(argValues[0]),
            "ln" when argValues.Length == 1 => Math.Log(argValues[0]),
            "log" when argValues.Length == 1 => Math.Log10(argValues[0]),
            "log10" when argValues.Length == 1 => Math.Log10(argValues[0]),
            "log2" when argValues.Length == 1 => Math.Log2(argValues[0]),
            
            // Other mathematical functions
            "sqrt" when argValues.Length == 1 => Math.Sqrt(argValues[0]),
            "abs" when argValues.Length == 1 => Math.Abs(argValues[0]),
            "sign" when argValues.Length == 1 => Math.Sign(argValues[0]),
            "floor" when argValues.Length == 1 => Math.Floor(argValues[0]),
            "ceil" when argValues.Length == 1 => Math.Ceiling(argValues[0]),
            "ceiling" when argValues.Length == 1 => Math.Ceiling(argValues[0]),
            "round" when argValues.Length == 1 => Math.Round(argValues[0]),
            
            // Multi-argument functions
            "min" when argValues.Length == 2 => Math.Min(argValues[0], argValues[1]),
            "max" when argValues.Length == 2 => Math.Max(argValues[0], argValues[1]),
            "atan2" when argValues.Length == 2 => Math.Atan2(argValues[0], argValues[1]),
            "hypot" when argValues.Length == 2 => Math.Sqrt(argValues[0] * argValues[0] + argValues[1] * argValues[1]),
            "mod" when argValues.Length == 2 => argValues[0] % argValues[1],
            "pow" when argValues.Length == 2 => Math.Pow(argValues[0], argValues[1]),
            
            // Custom functions
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
        if (Arguments.Count != 1)
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
        
        var arg = Arguments[0];
        var argDeriv = arg.Differentiate(variable);
        
        // Chain rule: d/dx[f(g(x))] = f'(g(x)) * g'(x)
        IExpression funcDeriv = Name.ToLower() switch
        {
            "sin" => new FunctionExpression("cos", Arguments.Cast<Expression>().ToList()),
            "cos" => new UnaryExpression(UnaryOperator.Negate, 
                        new FunctionExpression("sin", Arguments.Cast<Expression>().ToList())),
            "tan" => new BinaryExpression(
                        new ConstantExpression(1),
                        BinaryOperator.Divide,
                        new BinaryExpression(
                            new FunctionExpression("cos", Arguments.Cast<Expression>().ToList()),
                            BinaryOperator.Power,
                            new ConstantExpression(2))),
            "exp" => new FunctionExpression("exp", Arguments.Cast<Expression>().ToList()),
            "ln" => new BinaryExpression(new ConstantExpression(1), BinaryOperator.Divide, arg),
            "sqrt" => new BinaryExpression(
                        new ConstantExpression(0.5),
                        BinaryOperator.Divide,
                        new FunctionExpression("sqrt", Arguments.Cast<Expression>().ToList())),
            "sinh" => new FunctionExpression("cosh", Arguments.Cast<Expression>().ToList()),
            "cosh" => new FunctionExpression("sinh", Arguments.Cast<Expression>().ToList()),
            "tanh" => new BinaryExpression(
                        new ConstantExpression(1),
                        BinaryOperator.Subtract,
                        new BinaryExpression(
                            new FunctionExpression("tanh", Arguments.Cast<Expression>().ToList()),
                            BinaryOperator.Power,
                            new ConstantExpression(2))),
            _ => throw new NotSupportedException($"Differentiation of function '{Name}' is not supported")
        };
        
        return new BinaryExpression(funcDeriv, BinaryOperator.Multiply, argDeriv);
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