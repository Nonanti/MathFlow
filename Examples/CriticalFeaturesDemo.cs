using System;
using MathFlow.Core;
using MathFlow.Core.ComplexMath;
using MathFlow.Core.DifferentialEquations;

namespace MathFlow.Examples;

/// <summary>
/// Demonstrates the critical features that were previously missing
/// </summary>
public static class CriticalFeaturesDemo
{
    public static void Run()
    {
        var engine = new MathEngine();
        
        Console.WriteLine("\n=== Critical Features Demo ===\n");
        
        Console.WriteLine("## 1. Complex Number Support");
        TestComplexNumbers(engine);
        
        Console.WriteLine("\n## 2. Enhanced Polynomial Factoring");
        TestPolynomialFactoring(engine);
        
        Console.WriteLine("\n## 3. ODE Solver Methods (RungeKutta2 & AdamsBashforth)");
        TestODESolvers();
        
        Console.WriteLine("\n## 4. Rational Function Integration");
        TestRationalIntegration(engine);
    }
    
    private static void TestComplexNumbers(MathEngine engine)
    {
        var expr1 = engine.Parse("3 + 4*i");
        Console.WriteLine($"Parsed: 3 + 4*i = {expr1}");
        
        var expr2 = engine.Parse("(2 + 3*i) * (1 - i)");
        Console.WriteLine($"Complex multiplication: (2+3i)*(1-i) = {expr2.Simplify()}");
        
        var z1 = new ComplexNumber(3, 4);
        var z2 = new ComplexNumber(1, -2);
        Console.WriteLine($"z1 = {z1}, |z1| = {z1.Magnitude:F2}");
        Console.WriteLine($"z1 * z2 = {z1 * z2}");
        Console.WriteLine($"z1 / z2 = {z1 / z2}");
        
        try
        {
            var complexResult = engine.CalculateComplex("e^(i*pi)", null);
            Console.WriteLine($"e^(iπ) = {complexResult.Real:F2} + {complexResult.Imaginary:F2}i (Euler's identity)");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Complex calculation note: {ex.Message}");
        }
    }
    
    private static void TestPolynomialFactoring(MathEngine engine)
    {
        string[] polynomials = 
        {
            "x^2 - 4",
            "x^2 + 5*x + 6",
            "x^2 - 2*x + 1",
            "x^3 - 8",
            "2*x^2 + 7*x + 3"
        };
        
        foreach (var poly in polynomials)
        {
            try
            {
                var factored = engine.Factor(poly, "x");
                Console.WriteLine($"{poly} = {factored}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{poly} -> Error: {ex.Message}");
            }
        }
    }
    
    private static void TestODESolvers()
    {
        var solver = new ODESolver();
        
        string equation = "-2*y";
        double x0 = 0;
        double y0 = 1;
        double xEnd = 2;
        int steps = 20;
        
        var rk2Result = solver.Solve(equation, x0, y0, xEnd, ODEMethod.RungeKutta2, steps);
        var rk2Final = rk2Result.Points[^1];
        var rk2Exact = Math.Exp(-2 * xEnd);
        Console.WriteLine($"RungeKutta2 (Heun's method):");
        Console.WriteLine($"  Final value at x=2: {rk2Final.y:F6}");
        Console.WriteLine($"  Exact value: {rk2Exact:F6}");
        Console.WriteLine($"  Error: {Math.Abs(rk2Final.y - rk2Exact):F6}");
        
        var abResult = solver.Solve(equation, x0, y0, xEnd, ODEMethod.AdamsBashforth, steps);
        var abFinal = abResult.Points[^1];
        Console.WriteLine($"\nAdamsBashforth (3-step):");
        Console.WriteLine($"  Final value at x=2: {abFinal.y:F6}");
        Console.WriteLine($"  Exact value: {rk2Exact:F6}");
        Console.WriteLine($"  Error: {Math.Abs(abFinal.y - rk2Exact):F6}");
        
        var rk4Result = solver.Solve(equation, x0, y0, xEnd, ODEMethod.RungeKutta4, steps);
        var rk4Final = rk4Result.Points[^1];
        Console.WriteLine($"\nRungeKutta4 (reference):");
        Console.WriteLine($"  Final value at x=2: {rk4Final.y:F6}");
        Console.WriteLine($"  Error: {Math.Abs(rk4Final.y - rk2Exact):F6}");
    }
    
    private static void TestRationalIntegration(MathEngine engine)
    {
        string[] rationals = 
        {
            "1/(x + 2)",
            "1/(3*x + 5)",
            "1/(x^2 + 4)",
            "1/(x^2 + 1)",
            "(x + 1)/(x^2 + 2*x)"
        };
        
        Console.WriteLine("Symbolic integration of rational functions:");
        foreach (var rational in rationals)
        {
            try
            {
                var integral = engine.IntegrateSymbolic(rational, "x");
                Console.WriteLine($"∫({rational})dx = {integral}");
                
                var derivative = engine.Differentiate(integral.ToString(), "x");
                var simplified = derivative.Simplify();
                Console.WriteLine($"  Verification d/dx: {simplified}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"∫({rational})dx -> {ex.Message}");
            }
        }
    }
}