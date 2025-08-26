using MathFlow.Core.Expressions;
using MathFlow.Core.Interfaces;
using MathFlow.Core.Parser;
using MathFlow.Core.Solver;
using MathFlow.Core.Calculus;
using MathFlow.Core.Precision;

namespace MathFlow.Core;

public class MathEngine
{
    private readonly Dictionary<string, double> _variables;
    private readonly Dictionary<string, Func<double[], double>> _functions;
    private readonly PrecisionEvaluator _precisionEvaluator;
    
    public bool UsePrecisionMode { get; set; } = false;
    
    private int _precisionDigits = 50;
    public int PrecisionDigits 
    { 
        get => _precisionDigits;
        set
        {
            _precisionDigits = value;
            _precisionEvaluator.PrecisionDigits = value;
        }
    }
    
    public MathEngine()
    {
        _variables = new Dictionary<string, double>
        {
            ["pi"] = Math.PI,
            ["e"] = Math.E,
            // tau is 2*pi, some people prefer it
            ["tau"] = 2 * Math.PI,
            ["phi"] = (1 + Math.Sqrt(5)) / 2  // golden ratio
        };
        
        _functions = new Dictionary<string, Func<double[], double>>();
        _precisionEvaluator = new PrecisionEvaluator(PrecisionDigits);
    }
    
    public double Calculate(string expression, Dictionary<string, double>? variables = null)
    {
        var expr = Parse(expression);
        
        if (UsePrecisionMode)
        {
            // Use arbitrary precision
            var precisionVars = variables?.ToDictionary(
                kvp => kvp.Key, 
                kvp => new BigDecimal(kvp.Value, PrecisionDigits)
            ) ?? new Dictionary<string, BigDecimal>();
            
            var result = _precisionEvaluator.Evaluate(expr, precisionVars);
            return result.ToDouble();
        }
        
        var allVars = MergeVariables(variables);
        return expr.Evaluate(allVars);
    }
    
    public string CalculatePrecise(string expression, Dictionary<string, double>? variables = null)
    {
        var expr = Parse(expression);
        
        var precisionVars = variables?.ToDictionary(
            kvp => kvp.Key, 
            kvp => new BigDecimal(kvp.Value, PrecisionDigits)
        ) ?? new Dictionary<string, BigDecimal>();
        
        var result = _precisionEvaluator.Evaluate(expr, precisionVars);
        return result.ToString();
    }
    
    public Expression Parse(string expression)
    {
        var parser = new ExpressionParser();
        return parser.Parse(expression);
    }
    
    public Expression Build(string expression)
    {
        return Parse(expression);  // kept for backward compat
    }
    
    public double Evaluate(Expression expression, Dictionary<string, double>? variables = null)
    {
        var allVars = MergeVariables(variables);
        return expression.Evaluate(allVars);
    }
    
    public Expression Simplify(string expression)
    {
        var expr = Parse(expression);
        return (Expression)expr.Simplify();
    }
    
    public Expression Differentiate(string expression, string variable)
    {
        var expr = Parse(expression);
        return (Expression)expr.Differentiate(variable);
    }
    
    public double Integrate(string expression, string variable, double lowerBound, double upperBound, int steps = 10000)
    {
        var expr = Parse(expression);
        return NumericalIntegration.Integrate(expr, variable, lowerBound, upperBound, steps);
    }
    
    public double FindRoot(string equation, double initialGuess, double tolerance = 1e-10, int maxIterations = 100)
    {
        var expr = Parse(equation);
        return EquationSolver.FindRoot(expr, initialGuess, tolerance, maxIterations);
    }
    
    public double[] FindRoots(string equation, double start, double end, int divisions = 100)
    {
        var expr = Parse(equation);
        return EquationSolver.FindRoots(expr, start, end, divisions).ToArray();
    }
    
    public Expression Substitute(string expression, string variable, string replacement)
    {
        var expr = Parse(expression);
        var replExpr = Parse(replacement);
        return (Expression)expr.Substitute(variable, replExpr);
    }
    
    public Expression Expand(string expression)
    {
        var expr = Parse(expression);
        return ExpressionManipulator.Expand(expr);
    }
    
    public Expression Factor(string expression)
    {
        var expr = Parse(expression);
        return ExpressionManipulator.Factor(expr);
    }
    
    public void SetVariable(string name, double value)
    {
        _variables[name] = value;
    }
    
    public void RemoveVariable(string name)
    {
        _variables.Remove(name);
    }
    
    public void ClearVariables()
    {
        _variables.Clear();
        _variables["pi"] = Math.PI;
        _variables["e"] = Math.E;
        _variables["tau"] = 2 * Math.PI;
        _variables["phi"] = (1 + Math.Sqrt(5)) / 2;
    }
    
    public void RegisterFunction(string name, Func<double[], double> function)
    {
        _functions[name] = function ?? throw new ArgumentNullException(nameof(function));
    }
    
    public void UnregisterFunction(string name)
    {
        _functions.Remove(name);
    }
    
    public HashSet<string> GetVariables(string expression)
    {
        var expr = Parse(expression);
        return expr.GetVariables();
    }
    
    public bool IsValid(string expression)
    {
        try
        {
            Parse(expression);
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    public string ToLatex(string expression)
    {
        var expr = Parse(expression);
        return LatexConverter.ToLatex(expr);
    }
    
    public string ToMathML(string expression)
    {
        var expr = Parse(expression);
        return MathMLConverter.ToMathML(expr);
    }
    
    private Dictionary<string, double> MergeVariables(Dictionary<string, double>? userVariables)
    {
        var result = new Dictionary<string, double>(_variables);
        
        if (userVariables != null)
        {
            foreach (var kvp in userVariables)
            {
                result[kvp.Key] = kvp.Value;
            }
        }
        
        return result;
    }
}