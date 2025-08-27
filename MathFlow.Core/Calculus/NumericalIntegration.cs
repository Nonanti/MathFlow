using MathFlow.Core.Interfaces;
namespace MathFlow.Core.Calculus;
public static class NumericalIntegration
{
    public static double Integrate(IExpression expression, string variable, double lowerBound, double upperBound, int steps = 10000)
    {
        return Simpson(expression, variable, lowerBound, upperBound, steps);
    }
    
    public static double Simpson(IExpression expression, string variable, double a, double b, int n)
    {
        if (n % 2 != 0) n++;
        
        var h = (b - a) / n;
        var variables = new Dictionary<string, double>();
        
        variables[variable] = a;
        var sum = expression.Evaluate(variables);
        
        variables[variable] = b;
        sum += expression.Evaluate(variables);
        
        for (int i = 1; i < n; i++)
        {
            var x = a + i * h;
            variables[variable] = x;
            var value = expression.Evaluate(variables);
            
            sum += (i % 2 == 0 ? 2 : 4) * value;
        }
        
        return sum * h / 3;
    }
    
    public static double Trapezoidal(IExpression expression, string variable, double a, double b, int n)
    {
        var h = (b - a) / n;
        var variables = new Dictionary<string, double>();
        
        variables[variable] = a;
        var sum = expression.Evaluate(variables) / 2;
        
        for (int i = 1; i < n; i++)
        {
            var x = a + i * h;
            variables[variable] = x;
            sum += expression.Evaluate(variables);
        }
        
        variables[variable] = b;
        sum += expression.Evaluate(variables) / 2;
        
        return sum * h;
    }
    
    public static double MidpointRule(IExpression expression, string variable, double a, double b, int n)
    {
        var h = (b - a) / n;
        var sum = 0.0;
        var variables = new Dictionary<string, double>();
        
        for (int i = 0; i < n; i++)
        {
            var x = a + (i + 0.5) * h;
            variables[variable] = x;
            sum += expression.Evaluate(variables);
        }
        
        return sum * h;
    }
    
    public static double AdaptiveSimpson(IExpression expression, string variable, double a, double b, double tolerance = 1e-10)
    {
        return AdaptiveSimpsonRecursive(expression, variable, a, b, tolerance, Simpson(expression, variable, a, b, 2), 10);
    }
    
    private static double AdaptiveSimpsonRecursive(IExpression expression, string variable, double a, double b, 
        double tolerance, double wholeInterval, int depth)
    {
        if (depth <= 0)
            return wholeInterval;
        
        var mid = (a + b) / 2;
        var leftInterval = Simpson(expression, variable, a, mid, 2);
        var rightInterval = Simpson(expression, variable, mid, b, 2);
        var sum = leftInterval + rightInterval;
        
        if (Math.Abs(sum - wholeInterval) < 15 * tolerance)
            return sum + (sum - wholeInterval) / 15;
        
        return AdaptiveSimpsonRecursive(expression, variable, a, mid, tolerance / 2, leftInterval, depth - 1) +
               AdaptiveSimpsonRecursive(expression, variable, mid, b, tolerance / 2, rightInterval, depth - 1);
    }
    
    public static double GaussLegendre(IExpression expression, string variable, double a, double b, int n = 5)
    {
        double[] weights, nodes;
        
        switch (n)
        {
            case 2:
                weights = new[] { 1.0, 1.0 };
                nodes = new[] { -0.5773502691896257, 0.5773502691896257 };
                break;
            case 3:
                weights = new[] { 0.8888888888888888, 0.5555555555555556, 0.5555555555555556 };
                nodes = new[] { 0.0, -0.7745966692414834, 0.7745966692414834 };
                break;
            case 4:
                weights = new[] { 0.6521451548625461, 0.6521451548625461, 0.3478548451374538, 0.3478548451374538 };
                nodes = new[] { -0.3399810435848563, 0.3399810435848563, -0.8611363115940526, 0.8611363115940526 };
                break;
            case 5:
                weights = new[] { 0.5688888888888889, 0.4786286704993665, 0.4786286704993665, 0.2369268850561891, 0.2369268850561891 };
                nodes = new[] { 0.0, -0.5384693101056831, 0.5384693101056831, -0.9061798459386640, 0.9061798459386640 };
                break;
            default:
                return Simpson(expression, variable, a, b, n * 2);
        }
        
        var sum = 0.0;
        var variables = new Dictionary<string, double>();
        
        for (int i = 0; i < n; i++)
        {
            var x = ((b - a) * nodes[i] + b + a) / 2;
            variables[variable] = x;
            sum += weights[i] * expression.Evaluate(variables);
        }
        
        return sum * (b - a) / 2;
    }
}