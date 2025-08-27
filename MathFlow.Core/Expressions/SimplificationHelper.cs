using MathFlow.Core.Interfaces;
namespace MathFlow.Core.Expressions;
public static class SimplificationHelper
{
    public static IExpression SimplifyAddition(IExpression left, IExpression right)
    {
        var leftTerms = CollectTerms(left);
        var rightTerms = CollectTerms(right);
        
        var allTerms = new Dictionary<string, (double coefficient, IExpression? variable)>();
        
        foreach (var term in leftTerms.Concat(rightTerms))
        {
            var key = GetTermKey(term.variable);
            if (allTerms.ContainsKey(key))
            {
                allTerms[key] = (allTerms[key].coefficient + term.coefficient, term.variable);
            }
            else
            {
                allTerms[key] = term;
            }
        }
        
        return BuildExpressionFromTerms(allTerms);
    }
    
    public static IExpression SimplifyMultiplication(IExpression left, IExpression right)
    {
        if (left is BinaryExpression leftBin && leftBin.Operator == BinaryOperator.Power &&
            right is BinaryExpression rightBin && rightBin.Operator == BinaryOperator.Power)
        {
            if (leftBin.Left.Equals(rightBin.Left))
            {
                var newPower = new BinaryExpression(leftBin.Right, BinaryOperator.Add, rightBin.Right).Simplify();
                return new BinaryExpression(leftBin.Left, BinaryOperator.Power, newPower);
            }
        }
        
        if (right is BinaryExpression rightPow && rightPow.Operator == BinaryOperator.Power)
        {
            if (left.Equals(rightPow.Left))
            {
                var newPower = new BinaryExpression(rightPow.Right, BinaryOperator.Add, new ConstantExpression(1)).Simplify();
                return new BinaryExpression(left, BinaryOperator.Power, newPower);
            }
        }
        
        if (left is BinaryExpression leftPow && leftPow.Operator == BinaryOperator.Power)
        {
            if (right.Equals(leftPow.Left))
            {
                var newPower = new BinaryExpression(leftPow.Right, BinaryOperator.Add, new ConstantExpression(1)).Simplify();
                return new BinaryExpression(right, BinaryOperator.Power, newPower);
            }
        }
        
        if (left.Equals(right) && left is VariableExpression)
        {
            return new BinaryExpression(left, BinaryOperator.Power, new ConstantExpression(2));
        }
        
        return new BinaryExpression(left, BinaryOperator.Multiply, right);
    }
    
    private static List<(double coefficient, IExpression? variable)> CollectTerms(IExpression expr)
    {
        var terms = new List<(double coefficient, IExpression? variable)>();
        
        if (expr is ConstantExpression constant)
        {
            terms.Add((constant.Value, null));
        }
        else if (expr is VariableExpression variable)
        {
            terms.Add((1, variable));
        }
        else if (expr is BinaryExpression binary)
        {
            if (binary.Operator == BinaryOperator.Add)
            {
                terms.AddRange(CollectTerms(binary.Left));
                terms.AddRange(CollectTerms(binary.Right));
            }
            else if (binary.Operator == BinaryOperator.Subtract)
            {
                terms.AddRange(CollectTerms(binary.Left));
                var rightTerms = CollectTerms(binary.Right);
                foreach (var term in rightTerms)
                {
                    terms.Add((-term.coefficient, term.variable));
                }
            }
            else if (binary.Operator == BinaryOperator.Multiply)
            {
                if (binary.Left is ConstantExpression leftConst)
                {
                    if (binary.Right is VariableExpression rightVar)
                    {
                        terms.Add((leftConst.Value, rightVar));
                    }
                    else if (binary.Right is BinaryExpression rightBin && rightBin.Operator == BinaryOperator.Multiply)
                    {
                        var rightTerms = CollectTerms(binary.Right);
                        foreach (var term in rightTerms)
                        {
                            terms.Add((leftConst.Value * term.coefficient, term.variable));
                        }
                    }
                    else
                    {
                        terms.Add((leftConst.Value, binary.Right));
                    }
                }
                else if (binary.Right is ConstantExpression rightConst)
                {
                    if (binary.Left is VariableExpression leftVar)
                    {
                        terms.Add((rightConst.Value, leftVar));
                    }
                    else
                    {
                        terms.Add((rightConst.Value, binary.Left));
                    }
                }
                else
                {
                    terms.Add((1, binary));
                }
            }
            else
            {
                terms.Add((1, binary));
            }
        }
        else
        {
            terms.Add((1, expr));
        }
        
        return terms;
    }
    
    private static string GetTermKey(IExpression? expr)
    {
        if (expr == null) return "const";
        if (expr is VariableExpression var) return var.Name;
        return expr.ToString();
    }
    
    private static IExpression BuildExpressionFromTerms(Dictionary<string, (double coefficient, IExpression? variable)> terms)
    {
        var nonZeroTerms = terms.Where(t => Math.Abs(t.Value.coefficient) > 1e-10).ToList();
        
        if (nonZeroTerms.Count == 0)
            return new ConstantExpression(0);
        
        IExpression? result = null;
        
        foreach (var term in nonZeroTerms)
        {
            IExpression termExpr;
            
            if (term.Value.variable == null)
            {
                termExpr = new ConstantExpression(term.Value.coefficient);
            }
            else if (Math.Abs(term.Value.coefficient - 1) < 1e-10)
            {
                termExpr = term.Value.variable;
            }
            else if (Math.Abs(term.Value.coefficient + 1) < 1e-10)
            {
                termExpr = new UnaryExpression(UnaryOperator.Negate, term.Value.variable);
            }
            else
            {
                termExpr = new BinaryExpression(
                    new ConstantExpression(Math.Abs(term.Value.coefficient)),
                    BinaryOperator.Multiply,
                    term.Value.variable
                );
                
                if (term.Value.coefficient < 0)
                    termExpr = new UnaryExpression(UnaryOperator.Negate, termExpr);
            }
            
            if (result == null)
            {
                result = termExpr;
            }
            else
            {
                if (term.Value.coefficient < 0 && termExpr is UnaryExpression unary && unary.Operator == UnaryOperator.Negate)
                {
                    result = new BinaryExpression(result, BinaryOperator.Subtract, unary.Operand);
                }
                else
                {
                    result = new BinaryExpression(result, BinaryOperator.Add, termExpr);
                }
            }
        }
        
        return result ?? new ConstantExpression(0);
    }
}