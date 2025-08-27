using MathFlow.Core;
using MathFlow.Core.ComplexMath;
using MathFlow.Core.Extensions;
using MathFlow.Core.LinearAlgebra;
using MathFlow.Core.Statistics;

namespace MathFlow.Examples;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== MathFlow Examples ===\n");
        
        if (args.Length > 0)
        {
            switch (args[0].ToLower())
            {
                case "plotting":
                    PlottingDemo.Run();
                    return;
                case "integration":
                case "test":
                    TestIntegration.RunTest();
                    return;
                case "critical":
                case "features":
                    CriticalFeaturesDemo.Run();
                    return;
            }
        }
        
        BasicExamples();
        
        SymbolicMathExamples();
        
        ComplexNumberExamples();
        
        VectorExamples();
        
        StatisticsExamples();
    }
    
    static void BasicExamples()
    {
        Console.WriteLine("## Basic Expression Evaluation");
        var engine = new MathEngine();
        
        Console.WriteLine($"2 + 3 * 4 = {engine.Calculate("2 + 3 * 4")}");
        Console.WriteLine($"sin(pi/2) = {engine.Calculate("sin(pi/2)")}");
        Console.WriteLine($"sqrt(16) + log10(100) = {engine.Calculate("sqrt(16) + log10(100)")}");
        
        var vars = new Dictionary<string, double> { ["x"] = 3, ["y"] = 4 };
        Console.WriteLine($"x^2 + y^2 where x=3, y=4 = {engine.Calculate("x^2 + y^2", vars)}");
        Console.WriteLine();
    }
    
    static void SymbolicMathExamples()
    {
        Console.WriteLine("## Symbolic Mathematics");
        var engine = new MathEngine();
        
        var derivative = engine.Differentiate("x^3 - 2*x^2 + x - 1", "x");
        Console.WriteLine($"d/dx(x^3 - 2x^2 + x - 1) = {derivative}");
        
        var simplified = engine.Simplify("2*x + 3*x - x");
        Console.WriteLine($"Simplify: 2x + 3x - x = {simplified}");
        
        var integral = engine.Integrate("x^2", "x", 0, 1);
        Console.WriteLine($"∫[0,1] x^2 dx = {integral:F6}");
        
        var root = engine.FindRoot("x^2 - 2", 1);
        Console.WriteLine($"Root of x^2 - 2 = {root:F6} (√2)");
        Console.WriteLine();
    }
    
    static void ComplexNumberExamples()
    {
        Console.WriteLine("## Complex Numbers");
        
        var z1 = new ComplexNumber(3, 4);
        var z2 = new ComplexNumber(1, -2);
        
        Console.WriteLine($"z1 = {z1}");
        Console.WriteLine($"z2 = {z2}");
        Console.WriteLine($"|z1| = {z1.Magnitude:F2}");
        Console.WriteLine($"z1 + z2 = {z1 + z2}");
        Console.WriteLine($"z1 * z2 = {z1 * z2}");
        Console.WriteLine($"z1 / z2 = {z1 / z2}");
        Console.WriteLine($"z1* (conjugate) = {z1.Conjugate}");
        
        var polar = ComplexNumber.FromPolar(5, Math.PI / 4);
        Console.WriteLine($"From polar(5, π/4) = {polar}");
        Console.WriteLine();
    }
    
    static void VectorExamples()
    {
        Console.WriteLine("## Vector Operations");
        
        var v1 = new Vector(3, 4, 0);
        var v2 = new Vector(1, 2, 2);
        
        Console.WriteLine($"v1 = {v1}");
        Console.WriteLine($"v2 = {v2}");
        Console.WriteLine($"|v1| = {v1.Magnitude:F2}");
        Console.WriteLine($"v1 + v2 = {v1 + v2}");
        Console.WriteLine($"v1 · v2 = {v1.Dot(v2)}");
        Console.WriteLine($"v1 × v2 = {v1.Cross(v2)}");
        Console.WriteLine($"Angle = {v1.AngleTo(v2).ToDegrees():F1}°");
        Console.WriteLine($"v1 normalized = {v1.Normalized()}");
        Console.WriteLine();
    }
    
    static void StatisticsExamples()
    {
        Console.WriteLine("## Statistical Functions");
        
        var data = new[] { 2.5, 3.1, 4.2, 3.8, 5.1, 3.9, 4.5, 3.3 };
        var data2 = new[] { 5.2, 6.1, 7.8, 7.2, 9.3, 7.5, 8.1, 6.8 };
        
        Console.WriteLine($"Data: [{string.Join(", ", data)}]");
        Console.WriteLine($"Mean = {StatisticalFunctions.Mean(data):F2}");
        Console.WriteLine($"Median = {StatisticalFunctions.Median(data):F2}");
        Console.WriteLine($"StdDev = {StatisticalFunctions.StandardDeviation(data):F2}");
        
        var (q1, q2, q3) = StatisticalFunctions.Quartiles(data);
        Console.WriteLine($"Quartiles: Q1={q1:F2}, Q2={q2:F2}, Q3={q3:F2}");
        
        Console.WriteLine($"Correlation with data2 = {StatisticalFunctions.Correlation(data, data2):F3}");
        
        var (slope, intercept) = StatisticalFunctions.LinearRegression(data, data2);
        Console.WriteLine($"Linear regression: y = {slope:F2}x + {intercept:F2}");
    }
}