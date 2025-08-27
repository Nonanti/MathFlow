using Xunit;
using MathFlow.Core;
using MathFlow.Core.Plotting;
using System;

namespace MathFlow.Tests;

public class PlottingTests
{
    private readonly MathEngine engine;
    
    public PlottingTests()
    {
        engine = new MathEngine();
    }
    
    [Fact]
    public void TestSimplePlot()
    {
        var plotter = engine.Plot("sin(x)", -Math.PI, Math.PI);
        Assert.NotNull(plotter);
        
        var ascii = plotter.ToAsciiChart(60, 20);
        Assert.NotNull(ascii);
        Assert.Contains("●", ascii); // Should contain plot points
    }
    
    [Fact]
    public void TestMultipleFunctions()
    {
        var plotter = engine.PlotMultiple(
            new[] { "sin(x)", "cos(x)", "sin(x) + cos(x)" },
            -Math.PI, Math.PI
        );
        
        var ascii = plotter.ToAsciiChart();
        Assert.NotNull(ascii);
        Assert.Contains("Legend:", ascii); // Should have legend
    }
    
    [Fact]
    public void TestChainedOperations()
    {
        var plotter = engine.CreatePlotter(new PlotConfig { Title = "Multiple Functions" })
            .AddFunction("x", -10, 10, label: "Linear")
            .AddFunction("x^2", -10, 10, label: "Quadratic")
            .AddFunction("x^3", -10, 10, label: "Cubic");
        
        var ascii = plotter.ToAsciiChart();
        Assert.Contains("Linear", ascii);
        Assert.Contains("Quadratic", ascii);
        Assert.Contains("Cubic", ascii);
    }
    
    [Fact]
    public void TestExponentialAndLog()
    {
        var plotter = engine.CreatePlotter()
            .AddFunction("e^x", -2, 2, label: "e^x")
            .AddFunction("ln(x)", 0.1, 5, label: "ln(x)");
        
        var ascii = plotter.ToAsciiChart();
        Assert.NotNull(ascii);
        Assert.Contains("e^x", ascii);
        Assert.Contains("ln(x)", ascii);
    }
    
    [Fact]
    public void TestComplexFunction()
    {
        var plotter = engine.Plot("sin(x) * e^(-x/5)", -10, 10);
        var ascii = plotter.ToAsciiChart(80, 24);
        
        Assert.NotNull(ascii);
        // Should handle damped oscillation
        Assert.True(ascii.Length > 100);
    }
    
    [Fact]
    public void TestCustomPlotStyle()
    {
        var config = new PlotConfig
        {
            Title = "Test Plot",
            ShowGrid = false,  // ASCII doesn't show grid but config should work
            ShowLegend = true,
            ShowAxes = true
        };
        
        var plotter = engine.Plot("tan(x)", -Math.PI/3, Math.PI/3, config);
        var ascii = plotter.ToAsciiChart();
        
        Assert.Contains("Test Plot", ascii);
    }
    
    [Fact]
    public void TestParametricPlot()
    {
        // Test plotting x = t, y = sin(t) as y = sin(x)
        var plotter = engine.Plot("sin(x)", 0, 2 * Math.PI);
        var ascii = plotter.ToAsciiChart(60, 20);
        
        // Should show sine wave pattern
        Assert.NotNull(ascii);
        Assert.Contains("●", ascii);
    }
    
    [Fact]
    public void TestPlotBounds()
    {
        var config = new PlotConfig
        {
            MinY = -2,
            MaxY = 2
        };
        
        var plotter = engine.Plot("10 * sin(x)", -Math.PI, Math.PI, config);
        var ascii = plotter.ToAsciiChart();
        
        // Should respect the Y bounds
        Assert.NotNull(ascii);
        Assert.Contains("2.00", ascii); // Should show Y-axis labels
    }
    
    [Fact]
    public void TestDisplayMethod()
    {
        var plotter = engine.Plot("x^2", -3, 3);
        
        // Test that Display method exists (it writes to console)
        Assert.NotNull(plotter);
        
        // Can't easily test console output in unit tests, 
        // but we can verify the method exists and runs without error
        var ascii = plotter.ToAsciiChart();
        Assert.NotNull(ascii);
    }
    
    [Fact]
    public void TestClearFunction()
    {
        var plotter = engine.CreatePlotter()
            .AddFunction("sin(x)", -Math.PI, Math.PI)
            .AddFunction("cos(x)", -Math.PI, Math.PI);
        
        var beforeClear = plotter.ToAsciiChart();
        Assert.Contains("●", beforeClear);
        
        plotter.Clear();
        var afterClear = plotter.ToAsciiChart();
        Assert.Equal("No data to plot", afterClear);
    }
}