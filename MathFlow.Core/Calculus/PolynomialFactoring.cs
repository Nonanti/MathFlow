using MathFlow.Core.Expressions;
using MathFlow.Core.Interfaces;
namespace MathFlow.Core.Calculus;
/// <summary>
/// Polynomial factoring utilities
/// </summary>
public static class PolynomialFactoring
{
    /// <summary>
    /// Factor a polynomial expression
    /// </summary>
    public static IExpression Factor(IExpression expression, string variable)
    {
        var coeffs = ExtractPolynomialCoefficients(expression, variable);
        if (coeffs == null)
            return expression;
        
        var result = TryFactorQuadratic(coeffs, variable);
        if (result != null)
            return result;
            
        result = TryFactorDifferenceOfSquares(coeffs, variable);
        if (result != null)
            return result;
            
        result = TryFactorCubic(coeffs, variable);
        if (result != null)
            return result;
            
        result = TryFactorByGrouping(expression, variable);
        if (result != null)
            return result;
        
        return expression;
    }
    
    /// <summary>
    /// Extract polynomial coefficients [a0, a1, a2, ...] for a0 + a1*x + a2*x^2 + ...
    /// </summary>
    private static List<double>? ExtractPolynomialCoefficients(IExpression expr, string variable)
    {
        var simplified = expr.Simplify();
        var coeffs = new Dictionary<int, double>();
        
        CollectPolynomialTerms(simplified, variable, coeffs, 1.0);
        
        if (coeffs.Count == 0)
            return null;
            
        var maxDegree = coeffs.Keys.Max();
        var result = new List<double>(maxDegree + 1);
        
        for (int i = 0; i <= maxDegree; i++)
        {
            result.Add(coeffs.ContainsKey(i) ? coeffs[i] : 0);
        }
        
        return result;
    }
    
    private static void CollectPolynomialTerms(IExpression expr, string variable, Dictionary<int, double> coeffs, double multiplier)
    {
        if (expr is BinaryExpression binExpr)
        {
            if (binExpr.Operator == BinaryOperator.Add)
            {
                CollectPolynomialTerms(binExpr.Left, variable, coeffs, multiplier);
                CollectPolynomialTerms(binExpr.Right, variable, coeffs, multiplier);
            }
            else if (binExpr.Operator == BinaryOperator.Subtract)
            {
                CollectPolynomialTerms(binExpr.Left, variable, coeffs, multiplier);
                CollectPolynomialTerms(binExpr.Right, variable, coeffs, -multiplier);
            }
            else if (binExpr.Operator == BinaryOperator.Multiply)
            {
                if (binExpr.Left is ConstantExpression c && IsVariablePower(binExpr.Right, variable, out int power))
                {
                    coeffs[power] = coeffs.GetValueOrDefault(power) + c.Value * multiplier;
                }
                else if (binExpr.Right is ConstantExpression c2 && IsVariablePower(binExpr.Left, variable, out int power2))
                {
                    coeffs[power2] = coeffs.GetValueOrDefault(power2) + c2.Value * multiplier;
                }
                else if (binExpr.Left is ConstantExpression c3 && binExpr.Right is ConstantExpression c4)
                {
                    coeffs[0] = coeffs.GetValueOrDefault(0) + c3.Value * c4.Value * multiplier;
                }
            }
            else if (binExpr.Operator == BinaryOperator.Power && 
                     binExpr.Left is VariableExpression v && v.Name == variable &&
                     binExpr.Right is ConstantExpression exp && exp.Value == (int)exp.Value)
            {
                coeffs[(int)exp.Value] = coeffs.GetValueOrDefault((int)exp.Value) + multiplier;
            }
        }
        else if (expr is VariableExpression varExpr && varExpr.Name == variable)
        {
            coeffs[1] = coeffs.GetValueOrDefault(1) + multiplier;
        }
        else if (expr is ConstantExpression constExpr)
        {
            coeffs[0] = coeffs.GetValueOrDefault(0) + constExpr.Value * multiplier;
        }
    }
    
    private static bool IsVariablePower(IExpression expr, string variable, out int power)
    {
        power = 0;
        
        if (expr is VariableExpression v && v.Name == variable)
        {
            power = 1;
            return true;
        }
        
        if (expr is BinaryExpression binExpr && 
            binExpr.Operator == BinaryOperator.Power &&
            binExpr.Left is VariableExpression vp && vp.Name == variable &&
            binExpr.Right is ConstantExpression exp && exp.Value == (int)exp.Value)
        {
            power = (int)exp.Value;
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Try to factor quadratic ax^2 + bx + c
    /// </summary>
    private static IExpression? TryFactorQuadratic(List<double> coeffs, string variable)
    {
        if (coeffs.Count != 3 || Math.Abs(coeffs[2]) < 1e-10)
            return null;
            
        var a = coeffs[2];
        var b = coeffs[1];
        var c = coeffs[0];
        
        var discriminant = b * b - 4 * a * c;
        
        if (discriminant < -1e-10)
            return null;
            
        if (Math.Abs(discriminant) < 1e-10)
        {
            var root = -b / (2 * a);
            var xVar = new VariableExpression(variable);
            
            if (Math.Abs(a - 1) < 1e-10)
            {
                var factor = new BinaryExpression(xVar, BinaryOperator.Add, new ConstantExpression(root));
                return new BinaryExpression(factor, BinaryOperator.Multiply, factor);
            }
            else
            {
                var factor = new BinaryExpression(xVar, BinaryOperator.Add, new ConstantExpression(root));
                var squared = new BinaryExpression(factor, BinaryOperator.Multiply, factor);
                return new BinaryExpression(new ConstantExpression(a), BinaryOperator.Multiply, squared);
            }
        }
        
        var sqrtDisc = Math.Sqrt(discriminant);
        var root1 = (-b + sqrtDisc) / (2 * a);
        var root2 = (-b - sqrtDisc) / (2 * a);
        
        if (IsRational(root1, out var num1, out var den1) && IsRational(root2, out var num2, out var den2))
        {
            var xVar = new VariableExpression(variable);
            IExpression factor1, factor2;
            
            if (den1 == 1)
            {
                factor1 = new BinaryExpression(xVar, BinaryOperator.Subtract, new ConstantExpression(num1));
            }
            else
            {
                var term = new BinaryExpression(new ConstantExpression(den1), BinaryOperator.Multiply, xVar);
                factor1 = new BinaryExpression(term, BinaryOperator.Subtract, new ConstantExpression(num1));
            }
            
            if (den2 == 1)
            {
                factor2 = new BinaryExpression(xVar, BinaryOperator.Subtract, new ConstantExpression(num2));
            }
            else
            {
                var term = new BinaryExpression(new ConstantExpression(den2), BinaryOperator.Multiply, xVar);
                factor2 = new BinaryExpression(term, BinaryOperator.Subtract, new ConstantExpression(num2));
            }
            
            var product = new BinaryExpression(factor1, BinaryOperator.Multiply, factor2);
            
            if (Math.Abs(a - 1) > 1e-10)
            {
                return new BinaryExpression(new ConstantExpression(a), BinaryOperator.Multiply, product);
            }
            
            return product;
        }
        
        return null;
    }
    
    /// <summary>
    /// Check if a number is close to a rational number with small denominator
    /// </summary>
    private static bool IsRational(double value, out double numerator, out double denominator)
    {
        numerator = 0;
        denominator = 1;
        
        var rounded = Math.Round(value);
        if (Math.Abs(value - rounded) < 1e-8)
        {
            numerator = rounded;
            denominator = 1;
            return true;
        }
        
        for (int den = 2; den <= 100; den++)
        {
            var num = Math.Round(value * den);
            if (Math.Abs(value - num / den) < 1e-8)
            {
                numerator = num;
                denominator = den;
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Try to factor difference of squares: a^2 - b^2 = (a+b)(a-b)
    /// </summary>
    private static IExpression? TryFactorDifferenceOfSquares(List<double> coeffs, string variable)
    {
        if (coeffs.Count != 3 || Math.Abs(coeffs[1]) > 1e-10)
            return null;
            
        var a = coeffs[2];
        var c = coeffs[0];
        
        if (a * c >= 0)
            return null;
            
        var sqrtA = Math.Sqrt(Math.Abs(a));
        var sqrtC = Math.Sqrt(Math.Abs(c));
        
        if (Math.Abs(sqrtA - Math.Round(sqrtA)) > 1e-8 || Math.Abs(sqrtC - Math.Round(sqrtC)) > 1e-8)
            return null;
            
        var xVar = new VariableExpression(variable);
        IExpression term1, term2;
        
        if (Math.Abs(sqrtA - 1) < 1e-10)
        {
            term1 = xVar;
        }
        else
        {
            term1 = new BinaryExpression(new ConstantExpression(sqrtA), BinaryOperator.Multiply, xVar);
        }
        
        term2 = new ConstantExpression(sqrtC);
        
        var factor1 = new BinaryExpression(term1, BinaryOperator.Add, term2);
        var factor2 = new BinaryExpression(term1, BinaryOperator.Subtract, term2);
        
        return new BinaryExpression(factor1, BinaryOperator.Multiply, factor2);
    }
    
    /// <summary>
    /// Try to factor cubic polynomials
    /// </summary>
    private static IExpression? TryFactorCubic(List<double> coeffs, string variable)
    {
        if (coeffs.Count != 4 || Math.Abs(coeffs[3]) < 1e-10)
            return null;
            
        var a = coeffs[3];
        var d = coeffs[0];
        
        if (Math.Abs(d) < 1e-10)
        {
            var xVar = new VariableExpression(variable);
            var remainingCoeffs = new List<double> { coeffs[1], coeffs[2], coeffs[3] };
            var quadFactor = TryFactorQuadratic(remainingCoeffs, variable);
            
            if (quadFactor != null)
            {
                return new BinaryExpression(xVar, BinaryOperator.Multiply, quadFactor);
            }
        }
        
        for (int num = -10; num <= 10; num++)
        {
            for (int den = 1; den <= 5; den++)
            {
                var root = (double)num / den;
                var value = coeffs[0] + coeffs[1] * root + coeffs[2] * root * root + coeffs[3] * root * root * root;
                
                if (Math.Abs(value) < 1e-8)
                {
                    var quotient = SyntheticDivision(coeffs, root);
                    
                    var xVar = new VariableExpression(variable);
                    var linearFactor = new BinaryExpression(xVar, BinaryOperator.Subtract, new ConstantExpression(root));
                    
                    var quadFactor = TryFactorQuadratic(quotient, variable);
                    if (quadFactor != null)
                    {
                        return new BinaryExpression(linearFactor, BinaryOperator.Multiply, quadFactor);
                    }
                    
                    var quotientExpr = BuildPolynomialExpression(quotient, variable);
                    return new BinaryExpression(linearFactor, BinaryOperator.Multiply, quotientExpr);
                }
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Perform synthetic division
    /// </summary>
    private static List<double> SyntheticDivision(List<double> coeffs, double root)
    {
        var result = new List<double>();
        var current = coeffs[coeffs.Count - 1];
        
        for (int i = coeffs.Count - 2; i >= 0; i--)
        {
            result.Insert(0, current);
            current = coeffs[i] + current * root;
        }
        
        return result;
    }
    
    /// <summary>
    /// Build polynomial expression from coefficients
    /// </summary>
    private static IExpression BuildPolynomialExpression(List<double> coeffs, string variable)
    {
        IExpression? result = null;
        var xVar = new VariableExpression(variable);
        
        for (int i = 0; i < coeffs.Count; i++)
        {
            if (Math.Abs(coeffs[i]) < 1e-10)
                continue;
                
            IExpression term;
            
            if (i == 0)
            {
                term = new ConstantExpression(coeffs[i]);
            }
            else if (i == 1)
            {
                if (Math.Abs(coeffs[i] - 1) < 1e-10)
                {
                    term = xVar;
                }
                else
                {
                    term = new BinaryExpression(new ConstantExpression(coeffs[i]), BinaryOperator.Multiply, xVar);
                }
            }
            else
            {
                var power = new BinaryExpression(xVar, BinaryOperator.Power, new ConstantExpression(i));
                if (Math.Abs(coeffs[i] - 1) < 1e-10)
                {
                    term = power;
                }
                else
                {
                    term = new BinaryExpression(new ConstantExpression(coeffs[i]), BinaryOperator.Multiply, power);
                }
            }
            
            if (result == null)
            {
                result = term;
            }
            else
            {
                result = new BinaryExpression(result, BinaryOperator.Add, term);
            }
        }
        
        return result ?? new ConstantExpression(0);
    }
    
    /// <summary>
    /// Try to factor by grouping
    /// </summary>
    private static IExpression? TryFactorByGrouping(IExpression expression, string variable)
    {
        return null;
    }
}