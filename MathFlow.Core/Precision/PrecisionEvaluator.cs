using System;
using System.Collections.Generic;
using System.Linq;
using MathFlow.Core.Expressions;
using MathFlow.Core.Interfaces;

namespace MathFlow.Core.Precision;

/// <summary>
/// Evaluates mathematical expressions with precision mode
/// </summary>
public class PrecisionEvaluator
{
    private readonly Dictionary<string, Func<BigDecimal[], BigDecimal>> functions;
    private int precisionDigits = 100;
    
    public int PrecisionDigits 
    { 
        get => precisionDigits;
        set => precisionDigits = Math.Max(10, value); // min 10 digits
    }
    
    public PrecisionEvaluator(int precision = 100)
    {
        precisionDigits = precision;
        
        // setup function table
        functions = new Dictionary<string, Func<BigDecimal[], BigDecimal>>(StringComparer.OrdinalIgnoreCase)
        {
            ["sin"] = args => ArbitraryPrecisionMath.Sin(args[0], precisionDigits),
            ["cos"] = args => ArbitraryPrecisionMath.Cos(args[0], precisionDigits),
            ["tan"] = args => ArbitraryPrecisionMath.Tan(args[0], precisionDigits),
            ["asin"] = args => ArbitraryPrecisionMath.ArcSin(args[0], precisionDigits),
            ["acos"] = args => ArbitraryPrecisionMath.ArcCos(args[0], precisionDigits),
            ["atan"] = args => ArbitraryPrecisionMath.ArcTan(args[0], precisionDigits),
            ["sinh"] = args => ArbitraryPrecisionMath.Sinh(args[0], precisionDigits),
            ["cosh"] = args => ArbitraryPrecisionMath.Cosh(args[0], precisionDigits),
            ["tanh"] = args => ArbitraryPrecisionMath.Tanh(args[0], precisionDigits),
            ["exp"] = args => ArbitraryPrecisionMath.Exp(args[0], precisionDigits),
            ["ln"] = args => ArbitraryPrecisionMath.Ln(args[0], precisionDigits),
            ["log"] = args => ArbitraryPrecisionMath.Ln(args[0], precisionDigits), // same as ln
            ["log10"] = args => ArbitraryPrecisionMath.Log10(args[0], precisionDigits),
            ["sqrt"] = args => ArbitraryPrecisionMath.Sqrt(args[0], precisionDigits),
            ["abs"] = args => ArbitraryPrecisionMath.Abs(args[0]),
            ["floor"] = args => ArbitraryPrecisionMath.Floor(args[0]),
            ["ceiling"] = args => ArbitraryPrecisionMath.Ceiling(args[0]),
            ["ceil"] = args => ArbitraryPrecisionMath.Ceiling(args[0]), // alias
            ["round"] = args => ArbitraryPrecisionMath.Round(args[0]),
            ["sign"] = args => ArbitraryPrecisionMath.Sign(args[0]),
            // TODO: optimize min/max for more than 2 args
            ["min"] = args => args.Length == 2 ? (args[0] < args[1] ? args[0] : args[1]) : args.Min(),
            ["max"] = args => args.Length == 2 ? (args[0] > args[1] ? args[0] : args[1]) : args.Max(),
            ["pow"] = args => ArbitraryPrecisionMath.Pow(args[0], args[1], precisionDigits),
            ["factorial"] = args => ArbitraryPrecisionMath.Factorial(args[0])
        };
    }
    
    public BigDecimal Evaluate(IExpression expression, Dictionary<string, BigDecimal>? variables = null)
    {
        variables ??= new Dictionary<string, BigDecimal>(StringComparer.OrdinalIgnoreCase);
        
        switch (expression)
        {
            case ConstantExpression constant:
                return (BigDecimal)constant.Value;
            
            case VariableExpression variable:
                if (variables.TryGetValue(variable.Name, out var value))
                    return value;
                
                // built-in constants
                switch (variable.Name.ToLowerInvariant())
                {
                    case "pi":
                        return ArbitraryPrecisionMath.Pi(precisionDigits);
                    case "e":
                        return ArbitraryPrecisionMath.E(precisionDigits);
                    case "tau":
                        return new BigDecimal(2) * ArbitraryPrecisionMath.Pi(precisionDigits); // tau = 2*pi
                    case "phi":
                        // golden ratio
                        var five = new BigDecimal(5);
                        var sqrt5 = ArbitraryPrecisionMath.Sqrt(five, precisionDigits);
                        return (BigDecimal.One + sqrt5) / new BigDecimal(2);
                    default:
                        throw new InvalidOperationException($"Undefined variable: {variable.Name}");
                }
            
            case UnaryExpression unary:
                var operandValue = Evaluate(unary.Operand, variables);
                return unary.Operator switch
                {
                    UnaryOperator.Negate => -operandValue,
                    UnaryOperator.Sin => ArbitraryPrecisionMath.Sin(operandValue, precisionDigits),
                    UnaryOperator.Cos => ArbitraryPrecisionMath.Cos(operandValue, precisionDigits),
                    UnaryOperator.Tan => ArbitraryPrecisionMath.Tan(operandValue, precisionDigits),
                    UnaryOperator.Sqrt => ArbitraryPrecisionMath.Sqrt(operandValue, precisionDigits),
                    UnaryOperator.Abs => ArbitraryPrecisionMath.Abs(operandValue),
                    UnaryOperator.Ln => ArbitraryPrecisionMath.Ln(operandValue, precisionDigits),
                    UnaryOperator.Exp => ArbitraryPrecisionMath.Exp(operandValue, precisionDigits),
                    UnaryOperator.Asin => ArbitraryPrecisionMath.ArcSin(operandValue, precisionDigits),
                    UnaryOperator.Acos => ArbitraryPrecisionMath.ArcCos(operandValue, precisionDigits),
                    UnaryOperator.Atan => ArbitraryPrecisionMath.ArcTan(operandValue, precisionDigits),
                    UnaryOperator.Sinh => ArbitraryPrecisionMath.Sinh(operandValue, precisionDigits),
                    UnaryOperator.Cosh => ArbitraryPrecisionMath.Cosh(operandValue, precisionDigits),
                    UnaryOperator.Tanh => ArbitraryPrecisionMath.Tanh(operandValue, precisionDigits),
                    UnaryOperator.Log10 => ArbitraryPrecisionMath.Log10(operandValue, precisionDigits),
                    UnaryOperator.Floor => ArbitraryPrecisionMath.Floor(operandValue),
                    UnaryOperator.Ceiling => ArbitraryPrecisionMath.Ceiling(operandValue),
                    UnaryOperator.Round => ArbitraryPrecisionMath.Round(operandValue),
                    UnaryOperator.Sign => ArbitraryPrecisionMath.Sign(operandValue),
                    UnaryOperator.Factorial => ArbitraryPrecisionMath.Factorial(operandValue),
                    _ => throw new NotSupportedException($"Unsupported unary operator: {unary.Operator}")
                };
            
            case BinaryExpression binary:
                var leftValue = Evaluate(binary.Left, variables);
                var rightValue = Evaluate(binary.Right, variables);
                
                return binary.Operator switch
                {
                    BinaryOperator.Add => leftValue + rightValue,
                    BinaryOperator.Subtract => leftValue - rightValue,
                    BinaryOperator.Multiply => leftValue * rightValue,
                    BinaryOperator.Divide => leftValue / rightValue,
                    BinaryOperator.Power => ArbitraryPrecisionMath.Pow(leftValue, rightValue, precisionDigits),
                    BinaryOperator.Modulo => Modulo(leftValue, rightValue),
                    BinaryOperator.LogBase => ArbitraryPrecisionMath.LogBase(leftValue, rightValue, precisionDigits),
                    _ => throw new NotSupportedException($"Unsupported binary operator: {binary.Operator}")
                };
            
            case FunctionExpression function:
                if (!functions.TryGetValue(function.Name, out var func))
                    throw new InvalidOperationException($"Unknown function: {function.Name}");
                
                var args = function.Arguments.Select(arg => Evaluate(arg, variables)).ToArray();
                return func(args);
            
            default:
                throw new NotSupportedException($"Unsupported expression type: {expression.GetType().Name}");
        }
    }
    
    private BigDecimal Modulo(BigDecimal a, BigDecimal b)
    {
        // Simple modulo implementation
        var quotient = ArbitraryPrecisionMath.Floor(a / b);
        return a - (quotient * b);
    }
    
    public void RegisterFunction(string name, Func<BigDecimal[], BigDecimal> function)
    {
        functions[name] = function;
    }
}