using System;
using MathFlow.Core;
using MathFlow.Core.Plotting;

namespace MathFlow.Examples;

public class PlottingDemo
{
    public static void Run()
    {
        var engine = new MathEngine();
        
        Console.WriteLine("=== MathFlow Plotting Demo ===\n");
        
        // 1. Simple sine wave in ASCII
        Console.WriteLine("1. Sine Wave (ASCII):");
        Console.WriteLine("─────────────────────");
        var sinePlot = engine.Plot("sin(x)", -Math.PI, Math.PI);
        Console.WriteLine(sinePlot.ToAsciiChart(70, 20));
        
        // 2. Multiple trigonometric functions
        Console.WriteLine("\n2. Trigonometric Functions Comparison:");
        Console.WriteLine("──────────────────────────────────────");
        var trigPlot = engine.CreatePlotter(new PlotConfig { Title = "Trigonometric Functions" })
            .AddFunction("sin(x)", -Math.PI, Math.PI, label: "sin(x)")
            .AddFunction("cos(x)", -Math.PI, Math.PI, label: "cos(x)")
            .AddFunction("sin(x) + cos(x)", -Math.PI, Math.PI, label: "sin + cos");
        Console.WriteLine(trigPlot.ToAsciiChart(70, 20));
        
        // 3. Polynomial comparison
        Console.WriteLine("\n3. Polynomial Functions:");
        Console.WriteLine("────────────────────────");
        var polyPlot = engine.PlotMultiple(
            new[] { "x", "x^2/10", "x^3/100" },
            -10, 10,
            new PlotConfig { Title = "Polynomial Comparison" }
        );
        Console.WriteLine(polyPlot.ToAsciiChart(70, 20));
        
        // 4. Exponential decay with oscillation
        Console.WriteLine("\n4. Damped Oscillation:");
        Console.WriteLine("──────────────────────");
        var dampedPlot = engine.Plot("sin(2*x) * e^(-x/5)", 0, 20,
            new PlotConfig { Title = "Damped Sine Wave" });
        Console.WriteLine(dampedPlot.ToAsciiChart(70, 20));
        
        // 5. Mathematical Functions Display
        Console.WriteLine("\n5. Mathematical Functions:");
        Console.WriteLine("──────────────────────────");
        
        var mathPlot = engine.CreatePlotter(new PlotConfig 
        { 
            Title = "Common Mathematical Functions",
            ShowLegend = true
        })
        .AddFunction("sin(x)", -2 * Math.PI, 2 * Math.PI, label: "Sine")
        .AddFunction("cos(x)", -2 * Math.PI, 2 * Math.PI, label: "Cosine")
        .AddFunction("x/5", -2 * Math.PI, 2 * Math.PI, label: "Linear");
        
        Console.WriteLine(mathPlot.ToAsciiChart(70, 20));
        
        // 6. Complex function
        Console.WriteLine("\n6. Complex Function: f(x) = x * sin(1/x):");
        Console.WriteLine("──────────────────────────────────────────");
        var complexPlot = engine.Plot("x * sin(1/x)", 0.01, 2,
            new PlotConfig { Title = "x * sin(1/x)" });
        Console.WriteLine(complexPlot.ToAsciiChart(70, 20));
        
        // 7. Logarithmic and Exponential
        Console.WriteLine("\n7. Log vs Exp:");
        Console.WriteLine("──────────────");
        var logExpPlot = engine.CreatePlotter(new PlotConfig { Title = "Logarithm vs Exponential" })
            .AddFunction("ln(x)", 0.1, 5, label: "ln(x)")
            .AddFunction("e^x", -2, 2, label: "e^x")
            .AddFunction("x", -2, 5, label: "y=x");
        Console.WriteLine(logExpPlot.ToAsciiChart(70, 20));
        
        Console.WriteLine("\n✨ Plotting demo complete!");
        Console.WriteLine("All graphs displayed in terminal using ASCII art.");
    }
}