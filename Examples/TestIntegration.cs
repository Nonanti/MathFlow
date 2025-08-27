using MathFlow.Core;
using MathFlow.Core.Expressions;
using System;

public class TestIntegration
{
    public static void RunTest()
    {
        var engine = new MathEngine();
        
        Console.WriteLine("\n## Testing Symbolic Integration");
        Console.WriteLine("\n### Parsing Analysis:");
        
        // First, let's see how these expressions are being parsed
        var testExprs = new[] { "1/x", "x^(-2)", "e^x", "exp(x)", "x*exp(x)" };
        foreach (var expr in testExprs)
        {
            var parsed = engine.Parse(expr);
            Console.WriteLine($"\n{expr}:");
            Console.WriteLine($"  Type: {parsed.GetType().Name}");
            if (parsed is BinaryExpression bin)
            {
                Console.WriteLine($"  Left: {bin.Left} ({bin.Left.GetType().Name})");
                Console.WriteLine($"  Op: {bin.Operator}");
                Console.WriteLine($"  Right: {bin.Right} ({bin.Right.GetType().Name})");
            }
            else if (parsed is UnaryExpression unary)
            {
                Console.WriteLine($"  Op: {unary.Operator}");
                Console.WriteLine($"  Operand: {unary.Operand} ({unary.Operand.GetType().Name})");
            }
        }
        
        Console.WriteLine("\n### Integration Results:");
        
        // Test 1/x
        try
        {
            var result1 = engine.IntegrateSymbolic("1/x", "x");
            Console.WriteLine($"∫(1/x)dx = {result1}");
            Console.WriteLine($"  Type: {result1.GetType().Name}");
            Console.WriteLine($"  IsUnsupported: {MathFlow.Core.Calculus.SymbolicIntegration.IsUnsupported(result1)}");
            
            // Now try to differentiate it back
            var deriv = engine.Differentiate(result1.ToString(), "x");
            Console.WriteLine($"  Derivative: {deriv}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error with 1/x: {e.Message}");
        }
        
        // Test x^(-2)
        try
        {
            var result2 = engine.IntegrateSymbolic("x^(-2)", "x");
            Console.WriteLine($"∫x^(-2)dx = {result2}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error with x^(-2): {e.Message}");
        }
        
        // Test e^x
        try
        {
            var result3 = engine.IntegrateSymbolic("e^x", "x");
            Console.WriteLine($"∫e^x dx = {result3}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error with e^x: {e.Message}");
        }
        
        // Test x*exp(x)
        try
        {
            var result4 = engine.IntegrateSymbolic("x*exp(x)", "x");
            Console.WriteLine($"∫x*exp(x)dx = {result4}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error with x*exp(x): {e.Message}");
        }
    }
}