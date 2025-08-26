using MathFlow.Core.Expressions;
using MathFlow.Core.Interfaces;

namespace MathFlow.Core;

public static class ExpressionManipulator
{
    public static Expression Expand(Expression expression)
    {
        return (Expression)ExpandInternal(expression);
    }
    
    private static IExpression ExpandInternal(IExpression expression)
    {
        if (expression is BinaryExpression binary)
        {
            var left = ExpandInternal(binary.Left);
            var right = ExpandInternal(binary.Right);
            
            if (binary.Operator == BinaryOperator.Multiply)
            {
                if (left is BinaryExpression leftBin && leftBin.Operator == BinaryOperator.Add)
                {
                    var term1 = new BinaryExpression(leftBin.Left, BinaryOperator.Multiply, right);
                    var term2 = new BinaryExpression(leftBin.Right, BinaryOperator.Multiply, right);
                    return ExpandInternal(new BinaryExpression(term1, BinaryOperator.Add, term2));
                }
                
                if (left is BinaryExpression leftBin2 && leftBin2.Operator == BinaryOperator.Subtract)
                {
                    var term1 = new BinaryExpression(leftBin2.Left, BinaryOperator.Multiply, right);
                    var term2 = new BinaryExpression(leftBin2.Right, BinaryOperator.Multiply, right);
                    return ExpandInternal(new BinaryExpression(term1, BinaryOperator.Subtract, term2));
                }
                
                if (right is BinaryExpression rightBin && rightBin.Operator == BinaryOperator.Add)
                {
                    var term1 = new BinaryExpression(left, BinaryOperator.Multiply, rightBin.Left);
                    var term2 = new BinaryExpression(left, BinaryOperator.Multiply, rightBin.Right);
                    return ExpandInternal(new BinaryExpression(term1, BinaryOperator.Add, term2));
                }
                
                if (right is BinaryExpression rightBin2 && rightBin2.Operator == BinaryOperator.Subtract)
                {
                    var term1 = new BinaryExpression(left, BinaryOperator.Multiply, rightBin2.Left);
                    var term2 = new BinaryExpression(left, BinaryOperator.Multiply, rightBin2.Right);
                    return ExpandInternal(new BinaryExpression(term1, BinaryOperator.Subtract, term2));
                }
            }
            else if (binary.Operator == BinaryOperator.Power)
            {
                if (right is ConstantExpression constExp && Math.Abs(constExp.Value - Math.Round(constExp.Value)) < 1e-10)
                {
                    var n = (int)Math.Round(constExp.Value);
                    
                    if (n == 2 && left is BinaryExpression leftBin && leftBin.Operator == BinaryOperator.Add)
                    {
                        var a = leftBin.Left;
                        var b = leftBin.Right;
                        var a2 = new BinaryExpression(a, BinaryOperator.Power, new ConstantExpression(2));
                        var ab2 = new BinaryExpression(
                            new BinaryExpression(new ConstantExpression(2), BinaryOperator.Multiply, a),
                            BinaryOperator.Multiply, b);
                        var b2 = new BinaryExpression(b, BinaryOperator.Power, new ConstantExpression(2));
                        
                        return ExpandInternal(new BinaryExpression(
                            new BinaryExpression(a2, BinaryOperator.Add, ab2),
                            BinaryOperator.Add, b2));
                    }
                    
                    if (n == 2 && left is BinaryExpression leftBin2 && leftBin2.Operator == BinaryOperator.Subtract)
                    {
                        var a = leftBin2.Left;
                        var b = leftBin2.Right;
                        var a2 = new BinaryExpression(a, BinaryOperator.Power, new ConstantExpression(2));
                        var ab2 = new BinaryExpression(
                            new BinaryExpression(new ConstantExpression(2), BinaryOperator.Multiply, a),
                            BinaryOperator.Multiply, b);
                        var b2 = new BinaryExpression(b, BinaryOperator.Power, new ConstantExpression(2));
                        
                        return ExpandInternal(new BinaryExpression(
                            new BinaryExpression(a2, BinaryOperator.Subtract, ab2),
                            BinaryOperator.Add, b2));
                    }
                }
            }
            
            return new BinaryExpression(left, binary.Operator, right);
        }
        else if (expression is UnaryExpression unary)
        {
            return new UnaryExpression(unary.Operator, ExpandInternal(unary.Operand));
        }
        
        return expression;
    }
    
    public static Expression Factor(Expression expression)
    {
        return (Expression)FactorInternal(expression.Simplify());
    }
    
    private static IExpression FactorInternal(IExpression expression)
    {
        if (expression is BinaryExpression binary && (binary.Operator == BinaryOperator.Add || binary.Operator == BinaryOperator.Subtract))
        {
            var terms = CollectTerms(expression);
            var commonFactor = FindCommonFactor(terms);
            
            if (commonFactor != null && !(commonFactor is ConstantExpression c && Math.Abs(c.Value - 1) < 1e-10))
            {
                var factoredTerms = terms.Select(t => DivideTerm(t, commonFactor)).ToList();
                var sum = CombineTerms(factoredTerms);
                return new BinaryExpression(commonFactor, BinaryOperator.Multiply, sum);
            }
        }
        
        return expression;
    }
    
    private static List<IExpression> CollectTerms(IExpression expression)
    {
        var terms = new List<IExpression>();
        CollectTermsRecursive(expression, terms, false);
        return terms;
    }
    
    private static void CollectTermsRecursive(IExpression expression, List<IExpression> terms, bool negate)
    {
        if (expression is BinaryExpression binary)
        {
            if (binary.Operator == BinaryOperator.Add)
            {
                CollectTermsRecursive(binary.Left, terms, negate);
                CollectTermsRecursive(binary.Right, terms, negate);
            }
            else if (binary.Operator == BinaryOperator.Subtract)
            {
                CollectTermsRecursive(binary.Left, terms, negate);
                CollectTermsRecursive(binary.Right, terms, !negate);
            }
            else
            {
                terms.Add(negate ? new UnaryExpression(UnaryOperator.Negate, expression) : expression);
            }
        }
        else
        {
            terms.Add(negate ? new UnaryExpression(UnaryOperator.Negate, expression) : expression);
        }
    }
    
    private static IExpression? FindCommonFactor(List<IExpression> terms)
    {
        if (terms.Count == 0) return null;
        
        var commonVars = new Dictionary<string, int>();
        var hasConstantFactor = true;
        double gcd = 0;
        
        foreach (var term in terms)
        {
            var factors = GetFactors(term);
            
            if (factors.ConstantFactor != null)
            {
                if (gcd == 0)
                    gcd = Math.Abs(factors.ConstantFactor.Value);
                else
                    gcd = GCD(gcd, Math.Abs(factors.ConstantFactor.Value));
            }
            else
            {
                hasConstantFactor = false;
            }
            
            if (commonVars.Count == 0 && factors.VariableFactors.Count > 0)
            {
                foreach (var kvp in factors.VariableFactors)
                {
                    commonVars[kvp.Key] = kvp.Value;
                }
            }
            else
            {
                var toRemove = new List<string>();
                foreach (var kvp in commonVars)
                {
                    if (!factors.VariableFactors.ContainsKey(kvp.Key))
                    {
                        toRemove.Add(kvp.Key);
                    }
                    else
                    {
                        commonVars[kvp.Key] = Math.Min(kvp.Value, factors.VariableFactors[kvp.Key]);
                    }
                }
                
                foreach (var key in toRemove)
                {
                    commonVars.Remove(key);
                }
            }
        }
        
        IExpression? result = hasConstantFactor && gcd > 1 ? new ConstantExpression(gcd) : null;
        
        foreach (var kvp in commonVars.Where(kvp => kvp.Value > 0))
        {
            IExpression varFactor = new VariableExpression(kvp.Key);
            
            if (kvp.Value > 1)
            {
                varFactor = new BinaryExpression(varFactor, BinaryOperator.Power, new ConstantExpression(kvp.Value));
            }
            
            result = result == null ? varFactor : new BinaryExpression(result, BinaryOperator.Multiply, varFactor);
        }
        
        return result;
    }
    
    private static (ConstantExpression? ConstantFactor, Dictionary<string, int> VariableFactors) GetFactors(IExpression expression)
    {
        var constantFactor = new ConstantExpression(1);
        var variableFactors = new Dictionary<string, int>();
        
        GetFactorsRecursive(expression, ref constantFactor, variableFactors);
        
        return (Math.Abs(constantFactor.Value - 1) > 1e-10 ? constantFactor : null, variableFactors);
    }
    
    private static void GetFactorsRecursive(IExpression expression, ref ConstantExpression constantFactor, Dictionary<string, int> variableFactors)
    {
        if (expression is ConstantExpression constant)
        {
            constantFactor = new ConstantExpression(constantFactor.Value * constant.Value);
        }
        else if (expression is VariableExpression variable)
        {
            variableFactors[variable.Name] = variableFactors.GetValueOrDefault(variable.Name, 0) + 1;
        }
        else if (expression is BinaryExpression binary)
        {
            if (binary.Operator == BinaryOperator.Multiply)
            {
                GetFactorsRecursive(binary.Left, ref constantFactor, variableFactors);
                GetFactorsRecursive(binary.Right, ref constantFactor, variableFactors);
            }
            else if (binary.Operator == BinaryOperator.Power && binary.Right is ConstantExpression powerConst)
            {
                var power = (int)Math.Round(powerConst.Value);
                if (Math.Abs(powerConst.Value - power) < 1e-10 && binary.Left is VariableExpression varExp)
                {
                    variableFactors[varExp.Name] = variableFactors.GetValueOrDefault(varExp.Name, 0) + power;
                }
            }
        }
        else if (expression is UnaryExpression unary && unary.Operator == UnaryOperator.Negate)
        {
            constantFactor = new ConstantExpression(-constantFactor.Value);
            GetFactorsRecursive(unary.Operand, ref constantFactor, variableFactors);
        }
    }
    
    private static IExpression DivideTerm(IExpression term, IExpression divisor)
    {
        if (divisor is ConstantExpression constDiv)
        {
            if (term is ConstantExpression constTerm)
            {
                return new ConstantExpression(constTerm.Value / constDiv.Value);
            }
            else if (term is BinaryExpression binary && binary.Operator == BinaryOperator.Multiply)
            {
                if (binary.Left is ConstantExpression leftConst)
                {
                    return new BinaryExpression(
                        new ConstantExpression(leftConst.Value / constDiv.Value),
                        BinaryOperator.Multiply,
                        binary.Right
                    ).Simplify();
                }
                else if (binary.Right is ConstantExpression rightConst)
                {
                    return new BinaryExpression(
                        binary.Left,
                        BinaryOperator.Multiply,
                        new ConstantExpression(rightConst.Value / constDiv.Value)
                    ).Simplify();
                }
            }
        }
        
        return new BinaryExpression(term, BinaryOperator.Divide, divisor).Simplify();
    }
    
    private static IExpression CombineTerms(List<IExpression> terms)
    {
        if (terms.Count == 0)
            return new ConstantExpression(0);
        
        if (terms.Count == 1)
            return terms[0];
        
        var result = terms[0];
        for (int i = 1; i < terms.Count; i++)
        {
            result = new BinaryExpression(result, BinaryOperator.Add, terms[i]);
        }
        
        return result;
    }
    
    private static double GCD(double a, double b)
    {
        a = Math.Abs(a);
        b = Math.Abs(b);
        
        if (Math.Abs(a - Math.Round(a)) > 1e-10 || Math.Abs(b - Math.Round(b)) > 1e-10)
            return 1;
        
        long ia = (long)Math.Round(a);
        long ib = (long)Math.Round(b);
        
        while (ib != 0)
        {
            var temp = ib;
            ib = ia % ib;
            ia = temp;
        }
        
        return ia;
    }
}