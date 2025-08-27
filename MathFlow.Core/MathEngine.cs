using MathFlow.Core.Expressions;
using MathFlow.Core.Interfaces;
using MathFlow.Core.Parser;
using MathFlow.Core.Solver;
using MathFlow.Core.Calculus;
using MathFlow.Core.Precision;
using MathFlow.Core.LinearAlgebra;
using MathFlow.Core.Plotting;
using MathFlow.Core.DifferentialEquations;
using MathFlow.Core.ComplexMath;
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
            ["tau"] = 2 * Math.PI,
            ["phi"] = (1 + Math.Sqrt(5)) / 2
        };
        
        _functions = new Dictionary<string, Func<double[], double>>();
        _precisionEvaluator = new PrecisionEvaluator(PrecisionDigits);
    }
    
    public double Calculate(string expression, Dictionary<string, double>? variables = null)
    {
        var expr = Parse(expression);
        
        if (UsePrecisionMode)
        {
            var precisionVars = variables?.ToDictionary(
                kvp => kvp.Key, 
                kvp => new BigDecimal(kvp.Value)
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
        return Parse(expression);
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
    
    /// <summary>
    /// Performs symbolic integration (indefinite integral)
    /// </summary>
    public Expression IntegrateSymbolic(string expression, string variable)
    {
        var expr = Parse(expression);
        var result = SymbolicIntegration.Integrate(expr, variable);
        return (Expression)result;
    }
    
    /// <summary>
    /// Alias for symbolic integration
    /// </summary>
    public Expression Antiderivative(string expression, string variable)
    {
        return IntegrateSymbolic(expression, variable);
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
    
    public Expression Factor(string expression, string? variable = null)
    {
        var expr = Parse(expression);
        
        // If no variable specified, try to detect the main variable
        if (string.IsNullOrEmpty(variable))
        {
            var vars = expr.GetVariables();
            if (vars.Count == 1)
            {
                variable = vars.First();
            }
            else if (vars.Count > 1)
            {
                // Try common variable names
                if (vars.Contains("x")) variable = "x";
                else if (vars.Contains("y")) variable = "y";
                else if (vars.Contains("z")) variable = "z";
                else variable = vars.First();
            }
        }
        
        // Use the new polynomial factoring if we have a variable
        if (!string.IsNullOrEmpty(variable))
        {
            var factored = PolynomialFactoring.Factor(expr, variable);
            return (Expression)factored;
        }
        
        // Fall back to the old simple factoring
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
    
    #region Matrix Operations
    
    /// <summary>
    /// Parse a matrix from string notation like "[[1,2],[3,4]]"
    /// </summary>
    public Matrix ParseMatrix(string matrixString)
    {
        return Matrix.Parse(matrixString);
    }
    
    /// <summary>
    /// Calculate determinant of a matrix
    /// </summary>
    public double Determinant(string matrixString)
    {
        var matrix = Matrix.Parse(matrixString);
        return matrix.Determinant();
    }
    
    /// <summary>
    /// Calculate inverse of a matrix
    /// </summary>
    public Matrix Inverse(string matrixString)
    {
        var matrix = Matrix.Parse(matrixString);
        return matrix.Inverse();
    }
    
    /// <summary>
    /// Calculate eigenvalues and eigenvectors
    /// </summary>
    public MatrixOperations.EigenResult Eigen(string matrixString)
    {
        var matrix = Matrix.Parse(matrixString);
        return MatrixOperations.Eigen(matrix);
    }
    
    /// <summary>
    /// Solve linear system Ax = b
    /// </summary>
    public double[] SolveLinearSystem(string matrixA, double[] vectorB)
    {
        var A = Matrix.Parse(matrixA);
        return MatrixOperations.Solve(A, vectorB);
    }
    
    /// <summary>
    /// Calculate matrix rank
    /// </summary>
    public int MatrixRank(string matrixString)
    {
        var matrix = Matrix.Parse(matrixString);
        return MatrixOperations.Rank(matrix);
    }
    
    /// <summary>
    /// Calculate matrix trace
    /// </summary>
    public double MatrixTrace(string matrixString)
    {
        var matrix = Matrix.Parse(matrixString);
        return MatrixOperations.Trace(matrix);
    }
    
    #endregion
    
    #region Plotting
    
    /// <summary>
    /// Plot a mathematical expression
    /// </summary>
    public Plotter Plot(string expression, double minX, double maxX, PlotConfig? config = null)
    {
        var plotter = new Plotter(config);
        return plotter.AddFunction(expression, minX, maxX);
    }
    
    /// <summary>
    /// Plot multiple expressions
    /// </summary>
    public Plotter PlotMultiple(string[] expressions, double minX, double maxX, PlotConfig? config = null)
    {
        var plotter = new Plotter(config);
        foreach (var expr in expressions)
        {
            plotter.AddFunction(expr, minX, maxX);
        }
        return plotter;
    }
    
    /// <summary>
    /// Create a new plotter instance
    /// </summary>
    public Plotter CreatePlotter(PlotConfig? config = null)
    {
        return new Plotter(config);
    }
    
    #endregion
    
    #region Differential Equations
    
    /// <summary>
    /// Solve ordinary differential equation dy/dx = f(x,y)
    /// </summary>
    public ODEResult SolveODE(string equation, double x0, double y0, double xEnd, ODEMethod method = ODEMethod.RungeKutta4, int steps = 1000)
    {
        var solver = new ODESolver();
        return solver.Solve(equation, x0, y0, xEnd, method, steps);
    }
    
    /// <summary>
    /// Solve ODE and get value at specific point
    /// </summary>
    public double SolveODEAt(string equation, double x0, double y0, double xTarget, ODEMethod method = ODEMethod.RungeKutta4)
    {
        var solver = new ODESolver();
        var result = solver.Solve(equation, x0, y0, xTarget, method);
        return solver.InterpolateSolution(result, xTarget);
    }
    
    /// <summary>
    /// Solve system of ODEs
    /// </summary>
    public SystemODEResult SolveSystemODE(string[] equations, double t0, double[] y0, double tEnd, int steps = 1000)
    {
        var solver = new ODESolver();
        return solver.SolveSystem(equations, t0, y0, tEnd, steps);
    }
    
    #endregion
    
    #region Complex Number Support
    
    /// <summary>
    /// Calculate expression with complex number support
    /// </summary>
    public ComplexNumber CalculateComplex(string expression, Dictionary<string, ComplexNumber>? variables = null)
    {
        var expr = Parse(expression);
        
        if (expr is ComplexExpression complexExpr)
        {
            return complexExpr.Value;
        }
        
        // Try to evaluate as complex
        return EvaluateAsComplex(expr, variables);
    }
    
    private ComplexNumber EvaluateAsComplex(IExpression expr, Dictionary<string, ComplexNumber>? variables)
    {
        // For now, try regular evaluation and convert to complex
        // This is a simplified version - full implementation would evaluate complex operations
        try
        {
            var realVars = variables?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Real) ?? new Dictionary<string, double>();
            var result = expr.Evaluate(MergeVariables(realVars));
            return new ComplexNumber(result, 0);
        }
        catch
        {
            // If contains complex parts, handle specially
            if (expr is ComplexExpression ce)
                return ce.Value;
                
            throw new NotSupportedException($"Complex evaluation of {expr} not yet fully supported");
        }
    }
    
    #endregion
    
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