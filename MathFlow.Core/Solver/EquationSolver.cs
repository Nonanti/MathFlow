using MathFlow.Core.Interfaces;
using MathFlow.Core.Expressions;
namespace MathFlow.Core.Solver;
public static class EquationSolver
{
    public static double FindRoot(IExpression expression, double initialGuess, double tolerance = 1e-10, int maxIterations = 100)
    {
        return NewtonRaphson(expression, initialGuess, tolerance, maxIterations);
    }
    
    public static double NewtonRaphson(IExpression expression, double x0, double tolerance = 1e-10, int maxIter = 100)
    {
        var var_name = expression.GetVariables().FirstOrDefault();
        if (var_name == null)
        {
            var_name = "x";
        }
        
        IExpression deriv;
        try
        {
            deriv = expression.Differentiate(var_name);
        }
        catch
        {
            var h = 1e-8;
            var v1 = new Dictionary<string, double> { [var_name] = x0 - h };
            var v2 = new Dictionary<string, double> { [var_name] = x0 + h };
            var f1 = expression.Evaluate(v1);
            var f2 = expression.Evaluate(v2);
            var numDeriv = (f2 - f1) / (2 * h);
            deriv = new ConstantExpression(numDeriv);
        }
        
        var vars = new Dictionary<string, double>();
        
        var x = x0;
        
        for (int i = 0; i < maxIter; i++)
        {
            vars[var_name] = x;
            var fx = expression.Evaluate(vars);
            
            if (Math.Abs(fx) < tolerance)
                return x;
            
            var fpx = deriv.Evaluate(vars);
            
            if (Math.Abs(fpx) < tolerance)
                throw new InvalidOperationException("Derivative is zero. Newton-Raphson method fails.");
            
            var next_x = x - fx / fpx;
            
            if (Math.Abs(next_x - x) < tolerance)
                return next_x;
            
            x = next_x;
        }
        
        throw new InvalidOperationException($"Newton-Raphson method did not converge within {maxIter} iterations.");
    }
    
    public static double Bisection(IExpression expression, double a, double b, double tolerance = 1e-10, int maxIterations = 100)
    {
        var variable = expression.GetVariables().FirstOrDefault() ?? "x";
        var variables = new Dictionary<string, double>();
        
        variables[variable] = a;
        var fa = expression.Evaluate(variables);
        
        variables[variable] = b;
        var fb = expression.Evaluate(variables);
        
        if (fa * fb > 0)
            throw new ArgumentException("Invalid interval");
        
        for (int i = 0; i < maxIterations; i++)
        {
            var mid = (a + b) / 2;
            variables[variable] = mid;
            var fmid = expression.Evaluate(variables);
            
            if (Math.Abs(fmid) < tolerance || (b - a) / 2 < tolerance)
                return mid;
            
            if (fa * fmid < 0)
            {
                b = mid;
                fb = fmid;
            }
            else
            {
                a = mid;
                fa = fmid;
            }
        }
        
        return (a + b) / 2;
    }
    
    public static double Secant(IExpression expression, double x0, double x1, double tolerance = 1e-10, int maxIterations = 100)
    {
        var variable = expression.GetVariables().FirstOrDefault() ?? "x";
        var variables = new Dictionary<string, double>();
        
        for (int i = 0; i < maxIterations; i++)
        {
            variables[variable] = x0;
            var f0 = expression.Evaluate(variables);
            
            variables[variable] = x1;
            var f1 = expression.Evaluate(variables);
            
            if (Math.Abs(f1) < tolerance)
                return x1;
            
            if (Math.Abs(f1 - f0) < tolerance)
                throw new InvalidOperationException("Secant method fails: denominator too small.");
            
            var x2 = x1 - f1 * (x1 - x0) / (f1 - f0);
            
            if (Math.Abs(x2 - x1) < tolerance)
                return x2;
            
            x0 = x1;
            x1 = x2;
        }
        
        throw new InvalidOperationException($"Secant method did not converge within {maxIterations} iterations.");
    }
    
    public static List<double> FindRoots(IExpression expression, double start, double end, int divisions = 100)
    {
        var roots = new List<double>();
        var step = (end - start) / divisions;
        var variable = expression.GetVariables().FirstOrDefault() ?? "x";
        var variables = new Dictionary<string, double>();
        
        variables[variable] = start;
        var prevValue = expression.Evaluate(variables);
        
        for (int i = 1; i <= divisions; i++)
        {
            var x = start + i * step;
            variables[variable] = x;
            var currentValue = expression.Evaluate(variables);
            
            if (Math.Abs(currentValue) < 1e-10)
            {
                roots.Add(x);
            }
            else if (prevValue * currentValue < 0)
            {
                try
                {
                    var root = Bisection(expression, x - step, x);
                    
                    if (!roots.Any(r => Math.Abs(r - root) < 1e-8))
                        roots.Add(root);
                }
                catch
                {
                }
            }
            
            prevValue = currentValue;
        }
        
        return roots;
    }
    
    public static double BrentMethod(IExpression expression, double a, double b, double tolerance = 1e-10, int maxIterations = 100)
    {
        var variable = expression.GetVariables().FirstOrDefault() ?? "x";
        var variables = new Dictionary<string, double>();
        
        variables[variable] = a;
        var fa = expression.Evaluate(variables);
        
        variables[variable] = b;
        var fb = expression.Evaluate(variables);
        
        if (fa * fb > 0)
            throw new ArgumentException("Invalid interval");
        
        if (Math.Abs(fa) < Math.Abs(fb))
        {
            (a, b) = (b, a);
            (fa, fb) = (fb, fa);
        }
        
        var c = a;
        var fc = fa;
        var d = 0.0;
        var mflag = true;
        
        for (int i = 0; i < maxIterations; i++)
        {
            if (Math.Abs(fb) < tolerance)
                return b;
            
            var s = 0.0;
            
            if (Math.Abs(fa - fc) > tolerance && Math.Abs(fb - fc) > tolerance)
            {
                s = a * fb * fc / ((fa - fb) * (fa - fc)) +
                    b * fa * fc / ((fb - fa) * (fb - fc)) +
                    c * fa * fb / ((fc - fa) * (fc - fb));
            }
            else
            {
                s = b - fb * (b - a) / (fb - fa);
            }
            
            var condition1 = (s < (3 * a + b) / 4 || s > b);
            var condition2 = mflag && Math.Abs(s - b) >= Math.Abs(b - c) / 2;
            var condition3 = !mflag && Math.Abs(s - b) >= Math.Abs(c - d) / 2;
            var condition4 = mflag && Math.Abs(b - c) < tolerance;
            var condition5 = !mflag && Math.Abs(c - d) < tolerance;
            
            if (condition1 || condition2 || condition3 || condition4 || condition5)
            {
                s = (a + b) / 2;
                mflag = true;
            }
            else
            {
                mflag = false;
            }
            
            variables[variable] = s;
            var fs = expression.Evaluate(variables);
            
            d = c;
            c = b;
            fc = fb;
            
            if (fa * fs < 0)
            {
                b = s;
                fb = fs;
            }
            else
            {
                a = s;
                fa = fs;
            }
            
            if (Math.Abs(fa) < Math.Abs(fb))
            {
                (a, b) = (b, a);
                (fa, fb) = (fb, fa);
            }
            
            if (Math.Abs(b - a) < tolerance)
                return b;
        }
        
        return b;
    }
}