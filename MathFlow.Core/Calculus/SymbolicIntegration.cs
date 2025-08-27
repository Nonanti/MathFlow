using MathFlow.Core.Expressions;
using MathFlow.Core.Interfaces;
using System.Collections.Generic;
namespace MathFlow.Core.Calculus;
/// <summary>
/// Symbolic integration implementation
/// </summary>
public static class SymbolicIntegration
{
    public static IExpression Integrate(IExpression expression, string variable)
    {
        if (expression.IsConstant() || !expression.GetVariables().Contains(variable))
        {
            return new BinaryExpression(
                expression,
                BinaryOperator.Multiply,
                new VariableExpression(variable)
            );
        }
        
        if (expression is VariableExpression varExpr && varExpr.Name == variable)
        {
            return new BinaryExpression(
                new BinaryExpression(
                    new VariableExpression(variable),
                    BinaryOperator.Power,
                    new ConstantExpression(2)
                ),
                BinaryOperator.Divide,
                new ConstantExpression(2)
            );
        }
        
        if (expression is BinaryExpression binExpr)
        {
            if (binExpr.Operator == BinaryOperator.Power && 
                binExpr.Left is ConstantExpression ce && Math.Abs(ce.Value - Math.E) < 1e-10 &&
                binExpr.Right is VariableExpression xVar && xVar.Name == variable)
            {
                return binExpr;
            }
            
            if (binExpr.Operator == BinaryOperator.Power && 
                binExpr.Left is VariableExpression v && v.Name == variable)
            {
                double n = 0;
                bool handled = false;
                
                if (binExpr.Right is UnaryExpression unaryExp && 
                    unaryExp.Operator == UnaryOperator.Negate &&
                    unaryExp.Operand is ConstantExpression negConst)
                {
                    n = -negConst.Value;
                    handled = true;
                }
                else if (binExpr.Right is ConstantExpression c)
                {
                    n = c.Value;
                    handled = true;
                }
                
                if (handled)
                {
                    if (Math.Abs(n + 1) < 1e-10)
                    {
                        return new UnaryExpression(UnaryOperator.Ln, new VariableExpression(variable));
                    }
                    
                    var newPower = n + 1;
                    return new BinaryExpression(
                        new BinaryExpression(
                            new VariableExpression(variable),
                            BinaryOperator.Power,
                            new ConstantExpression(newPower)
                        ),
                        BinaryOperator.Divide,
                        new ConstantExpression(newPower)
                    );
                }
            }
            
            if (binExpr.Operator == BinaryOperator.Divide &&
                binExpr.Left is ConstantExpression c1 && Math.Abs(c1.Value - 1) < 1e-10 &&
                binExpr.Right is VariableExpression divVar && divVar.Name == variable)
            {
                return new UnaryExpression(UnaryOperator.Ln, new VariableExpression(variable));
            }
            
            if (binExpr.Operator == BinaryOperator.Add)
            {
                var leftInt = Integrate(binExpr.Left, variable);
                var rightInt = Integrate(binExpr.Right, variable);
                return new BinaryExpression(leftInt, BinaryOperator.Add, rightInt);
            }
            
            if (binExpr.Operator == BinaryOperator.Subtract)
            {
                var leftInt = Integrate(binExpr.Left, variable);
                var rightInt = Integrate(binExpr.Right, variable);
                return new BinaryExpression(leftInt, BinaryOperator.Subtract, rightInt);
            }
            
            if (binExpr.Operator == BinaryOperator.Multiply)
            {
                if (binExpr.Left.IsConstant() || !binExpr.Left.GetVariables().Contains(variable))
                {
                    var rightInt = Integrate(binExpr.Right, variable);
                    return new BinaryExpression(binExpr.Left, BinaryOperator.Multiply, rightInt);
                }
                if (binExpr.Right.IsConstant() || !binExpr.Right.GetVariables().Contains(variable))
                {
                    var leftInt = Integrate(binExpr.Left, variable);
                    return new BinaryExpression(binExpr.Right, BinaryOperator.Multiply, leftInt);
                }
                
                return TrySubstitution(binExpr, variable) ?? ThrowUnsupported(expression);
            }
            
            if (binExpr.Operator == BinaryOperator.Divide)
            {
                var rationalResult = TryIntegrateRational(binExpr, variable);
                if (rationalResult != null)
                    return rationalResult;
            }
        }
        
        if (expression is FunctionExpression funcExpr)
        {
            return IntegrateFunction(funcExpr, variable);
        }
        
        if (expression is UnaryExpression unaryExpr)
        {
            return IntegrateUnary(unaryExpr, variable);
        }
        
        return ThrowUnsupported(expression);
    }
    
    private static IExpression IntegrateUnary(UnaryExpression unary, string variable)
    {
        var arg = unary.Operand;
        
        if (arg is VariableExpression v && v.Name == variable)
        {
            switch (unary.Operator)
            {
                case UnaryOperator.Sin:
                    return new UnaryExpression(UnaryOperator.Negate,
                        new UnaryExpression(UnaryOperator.Cos, new VariableExpression(variable)));
                    
                case UnaryOperator.Cos:
                    return new UnaryExpression(UnaryOperator.Sin, new VariableExpression(variable));
                    
                case UnaryOperator.Exp:
                    return new UnaryExpression(UnaryOperator.Exp, new VariableExpression(variable));
                    
                case UnaryOperator.Sinh:
                    return new UnaryExpression(UnaryOperator.Cosh, new VariableExpression(variable));
                    
                case UnaryOperator.Cosh:
                    return new UnaryExpression(UnaryOperator.Sinh, new VariableExpression(variable));
                    
                case UnaryOperator.Ln:
                    var xVar = new VariableExpression(variable);
                    var xlnx = new BinaryExpression(
                        xVar,
                        BinaryOperator.Multiply,
                        new UnaryExpression(UnaryOperator.Ln, xVar)
                    );
                    return new BinaryExpression(xlnx, BinaryOperator.Subtract, xVar);
                    
                case UnaryOperator.Tan:
                    return new UnaryExpression(
                        UnaryOperator.Negate,
                        new UnaryExpression(UnaryOperator.Ln,
                            new UnaryExpression(UnaryOperator.Abs,
                                new UnaryExpression(UnaryOperator.Cos, new VariableExpression(variable))
                            )
                        )
                    );
                    
                case UnaryOperator.Sqrt:
                    var x32 = new BinaryExpression(
                        new VariableExpression(variable),
                        BinaryOperator.Power,
                        new ConstantExpression(1.5)
                    );
                    return new BinaryExpression(
                        new BinaryExpression(new ConstantExpression(2), BinaryOperator.Divide, new ConstantExpression(3)),
                        BinaryOperator.Multiply,
                        x32
                    );
                    
                case UnaryOperator.Negate:
                    var inner = Integrate(unary.Operand, variable);
                    return new UnaryExpression(UnaryOperator.Negate, inner);
                    
                default:
                    break;
            }
        }
        else if (arg.GetVariables().Contains(variable))
        {
            if (IsLinearInVariable(arg, variable, out var a, out var b))
            {
                var simpleUnary = new UnaryExpression(unary.Operator, new VariableExpression("u"));
                var innerIntegral = IntegrateUnary(simpleUnary, "u");
                if (innerIntegral != null && !IsUnsupported(innerIntegral))
                {
                    var result = innerIntegral.Substitute("u", arg);
                    return new BinaryExpression(result, BinaryOperator.Divide, new ConstantExpression(a));
                }
            }
        }
        else if (!arg.GetVariables().Contains(variable))
        {
            return new BinaryExpression(
                unary,
                BinaryOperator.Multiply,
                new VariableExpression(variable)
            );
        }
        
        return ThrowUnsupported(unary);
    }
    
    private static IExpression IntegrateFunction(FunctionExpression func, string variable)
    {
        if (func.Arguments.Count != 1)
            return ThrowUnsupported(func);
            
        var arg = func.Arguments[0];
        
        if (arg is VariableExpression v && v.Name == variable)
        {
            switch (func.Name.ToLower())
            {
                case "sin":
                    return new UnaryExpression(
                        UnaryOperator.Negate,
                        new FunctionExpression("cos", new List<Expression> { new VariableExpression(variable) })
                    );
                    
                case "cos":
                    return new FunctionExpression("sin", new List<Expression> { new VariableExpression(variable) });
                    
                case "exp":
                case "e^x":
                    return new FunctionExpression("exp", new List<Expression> { new VariableExpression(variable) });
                    
                case "ln":
                    var xVar = new VariableExpression(variable);
                    var xlnx = new BinaryExpression(
                        xVar,
                        BinaryOperator.Multiply,
                        new FunctionExpression("ln", new List<Expression> { xVar })
                    );
                    return new BinaryExpression(xlnx, BinaryOperator.Subtract, xVar);
                    
                case "tan":
                    return new UnaryExpression(
                        UnaryOperator.Negate,
                        new FunctionExpression("ln", new List<Expression> { 
                            new FunctionExpression("abs", new List<Expression> {
                                new FunctionExpression("cos", new List<Expression> { new VariableExpression(variable) })
                            })
                        })
                    );
                    
                case "sec":
                    var secX = new FunctionExpression("sec", new List<Expression> { new VariableExpression(variable) });
                    var tanX = new FunctionExpression("tan", new List<Expression> { new VariableExpression(variable) });
                    var sum = new BinaryExpression(secX, BinaryOperator.Add, tanX);
                    return new FunctionExpression("ln", new List<Expression> { 
                        new FunctionExpression("abs", new List<Expression> { sum })
                    });
                    
                case "csc":
                    var cscX = new FunctionExpression("csc", new List<Expression> { new VariableExpression(variable) });
                    var cotX = new FunctionExpression("cot", new List<Expression> { new VariableExpression(variable) });
                    var sum2 = new BinaryExpression(cscX, BinaryOperator.Add, cotX);
                    return new UnaryExpression(
                        UnaryOperator.Negate,
                        new FunctionExpression("ln", new List<Expression> { 
                            new FunctionExpression("abs", new List<Expression> { sum2 })
                        })
                    );
                    
                case "sinh":
                    return new FunctionExpression("cosh", new List<Expression> { new VariableExpression(variable) });
                    
                case "cosh":
                    return new FunctionExpression("sinh", new List<Expression> { new VariableExpression(variable) });
                    
                default:
                    break;
            }
        }
        
        if (IsLinearInVariable(arg, variable, out var a, out var b))
        {
            var inner = Integrate(new FunctionExpression(func.Name, new List<Expression> { new VariableExpression("u") }), "u");
            if (inner != null)
            {
                var result = inner.Substitute("u", arg);
                return new BinaryExpression(result, BinaryOperator.Divide, new ConstantExpression(a));
            }
        }
        
        return ThrowUnsupported(func);
    }
    
    private static bool IsLinearInVariable(IExpression expr, string variable, out double coeff, out double constant)
    {
        coeff = 0;
        constant = 0;
        
        if (expr is BinaryExpression bin)
        {
            if (bin.Operator == BinaryOperator.Add || bin.Operator == BinaryOperator.Subtract)
            {
                var hasVar = bin.Left.GetVariables().Contains(variable);
                var isConst = bin.Right.IsConstant();
                
                if (hasVar && isConst)
                {
                    if (bin.Left is VariableExpression && ((VariableExpression)bin.Left).Name == variable)
                    {
                        coeff = 1;
                        constant = bin.Operator == BinaryOperator.Add ? 
                            bin.Right.Evaluate() : -bin.Right.Evaluate();
                        return true;
                    }
                    else if (bin.Left is BinaryExpression leftBin && 
                             leftBin.Operator == BinaryOperator.Multiply &&
                             leftBin.Right is VariableExpression v && v.Name == variable &&
                             leftBin.Left.IsConstant())
                    {
                        coeff = leftBin.Left.Evaluate();
                        constant = bin.Operator == BinaryOperator.Add ? 
                            bin.Right.Evaluate() : -bin.Right.Evaluate();
                        return true;
                    }
                }
            }
            else if (bin.Operator == BinaryOperator.Multiply && 
                     bin.Left.IsConstant() && 
                     bin.Right is VariableExpression v && v.Name == variable)
            {
                coeff = bin.Left.Evaluate();
                constant = 0;
                return true;
            }
        }
        else if (expr is VariableExpression varE && varE.Name == variable)
        {
            coeff = 1;
            constant = 0;
            return true;
        }
        
        return false;
    }
    
    private static IExpression? TrySubstitution(BinaryExpression expr, string variable)
    {
        
        if (expr.Left is VariableExpression v1 && v1.Name == variable &&
            expr.Right is UnaryExpression u1 && u1.Operator == UnaryOperator.Exp && 
            u1.Operand is VariableExpression v2 && v2.Name == variable)
        {
            var xMinus1 = new BinaryExpression(
                new VariableExpression(variable),
                BinaryOperator.Subtract,
                new ConstantExpression(1)
            );
            return new BinaryExpression(
                xMinus1,
                BinaryOperator.Multiply,
                new UnaryExpression(UnaryOperator.Exp, new VariableExpression(variable))
            );
        }
        
        if (expr.Left is VariableExpression v3 && v3.Name == variable &&
            expr.Right is UnaryExpression u2 && u2.Operator == UnaryOperator.Sin && 
            u2.Operand is VariableExpression v4 && v4.Name == variable)
        {
            var sinX = new UnaryExpression(UnaryOperator.Sin, new VariableExpression(variable));
            var xCosX = new BinaryExpression(
                new VariableExpression(variable),
                BinaryOperator.Multiply,
                new UnaryExpression(UnaryOperator.Cos, new VariableExpression(variable))
            );
            return new BinaryExpression(sinX, BinaryOperator.Subtract, xCosX);
        }
        
        if (expr.Left is VariableExpression v5 && v5.Name == variable &&
            expr.Right is UnaryExpression u3 && u3.Operator == UnaryOperator.Cos && 
            u3.Operand is VariableExpression v6 && v6.Name == variable)
        {
            var cosX = new UnaryExpression(UnaryOperator.Cos, new VariableExpression(variable));
            var xSinX = new BinaryExpression(
                new VariableExpression(variable),
                BinaryOperator.Multiply,
                new UnaryExpression(UnaryOperator.Sin, new VariableExpression(variable))
            );
            return new BinaryExpression(cosX, BinaryOperator.Add, xSinX);
        }
        
        return null;
    }
    
    private static IExpression ThrowUnsupported(IExpression expr)
    {
        return new FunctionExpression("INTEGRAL_UNSUPPORTED", new List<Expression> { (Expression)expr });
    }
    
    /// <summary>
    /// Try to integrate rational functions using partial fractions
    /// </summary>
    private static IExpression? TryIntegrateRational(BinaryExpression rational, string variable)
    {
        var numerator = rational.Left;
        var denominator = rational.Right;
        
        if (numerator is ConstantExpression numConst && Math.Abs(numConst.Value - 1) < 1e-10)
        {
            if (IsLinearFormula(denominator, variable, out double a, out double b))
            {
                var axPlusB = denominator;
                var lnTerm = new UnaryExpression(UnaryOperator.Ln, 
                    new UnaryExpression(UnaryOperator.Abs, axPlusB));
                    
                if (Math.Abs(a - 1) < 1e-10)
                {
                    return lnTerm;
                }
                else
                {
                    return new BinaryExpression(
                        new ConstantExpression(1 / a),
                        BinaryOperator.Multiply,
                        lnTerm
                    );
                }
            }
            
            if (IsQuadraticForm(denominator, variable, out double coeff, out double constant))
            {
                if (constant > 0)
                {
                    var sqrtConst = Math.Sqrt(constant);
                    var xVar = new VariableExpression(variable);
                    var xOverA = new BinaryExpression(xVar, BinaryOperator.Divide, new ConstantExpression(sqrtConst));
                    var arctanTerm = new UnaryExpression(UnaryOperator.Atan, xOverA);
                    
                    return new BinaryExpression(
                        new ConstantExpression(1 / sqrtConst),
                        BinaryOperator.Multiply,
                        arctanTerm
                    );
                }
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Check if expression is linear in variable: ax + b (different from existing IsLinearInVariable)
    /// </summary>
    private static bool IsLinearFormula(IExpression expr, string variable, out double a, out double b)
    {
        a = 0;
        b = 0;
        
        if (expr is VariableExpression v && v.Name == variable)
        {
            a = 1;
            b = 0;
            return true;
        }
        
        if (expr is ConstantExpression c)
        {
            a = 0;
            b = c.Value;
            return true;
        }
        
        if (expr is BinaryExpression bin)
        {
            if (bin.Operator == BinaryOperator.Add)
            {
                if (bin.Left is VariableExpression vl && vl.Name == variable && 
                    bin.Right is ConstantExpression cr)
                {
                    a = 1;
                    b = cr.Value;
                    return true;
                }
                if (bin.Right is VariableExpression vr && vr.Name == variable && 
                    bin.Left is ConstantExpression cl)
                {
                    a = 1;
                    b = cl.Value;
                    return true;
                }
                
                if (bin.Left is BinaryExpression mult && mult.Operator == BinaryOperator.Multiply)
                {
                    if (mult.Left is ConstantExpression mc && mult.Right is VariableExpression mv && 
                        mv.Name == variable && bin.Right is ConstantExpression br)
                    {
                        a = mc.Value;
                        b = br.Value;
                        return true;
                    }
                }
            }
            else if (bin.Operator == BinaryOperator.Multiply)
            {
                if (bin.Left is ConstantExpression cm && bin.Right is VariableExpression vm && 
                    vm.Name == variable)
                {
                    a = cm.Value;
                    b = 0;
                    return true;
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Check if expression is x^2 + c form
    /// </summary>
    private static bool IsQuadraticForm(IExpression expr, string variable, out double coefficient, out double constant)
    {
        coefficient = 0;
        constant = 0;
        
        if (expr is BinaryExpression bin && bin.Operator == BinaryOperator.Add)
        {
            if (bin.Left is BinaryExpression pow && pow.Operator == BinaryOperator.Power &&
                pow.Left is VariableExpression v && v.Name == variable &&
                pow.Right is ConstantExpression exp && Math.Abs(exp.Value - 2) < 1e-10 &&
                bin.Right is ConstantExpression c)
            {
                coefficient = 1;
                constant = c.Value;
                return true;
            }
            
            if (bin.Right is BinaryExpression pow2 && pow2.Operator == BinaryOperator.Power &&
                pow2.Left is VariableExpression v2 && v2.Name == variable &&
                pow2.Right is ConstantExpression exp2 && Math.Abs(exp2.Value - 2) < 1e-10 &&
                bin.Left is ConstantExpression c2)
            {
                coefficient = 1;
                constant = c2.Value;
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Checks if an integral result contains unsupported parts
    /// </summary>
    public static bool IsUnsupported(IExpression result)
    {
        if (result is FunctionExpression f && f.Name == "INTEGRAL_UNSUPPORTED")
            return true;
            
        if (result is BinaryExpression bin)
            return IsUnsupported(bin.Left) || IsUnsupported(bin.Right);
            
        if (result is UnaryExpression un)
            return IsUnsupported(un.Operand);
            
        if (result is FunctionExpression func)
            return func.Arguments.Any(IsUnsupported);
            
        return false;
    }
}