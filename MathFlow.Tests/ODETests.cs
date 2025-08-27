using Xunit;
using MathFlow.Core;
using MathFlow.Core.DifferentialEquations;
using System;

namespace MathFlow.Tests;

public class ODETests
{
    private readonly MathEngine engine;
    
    public ODETests()
    {
        engine = new MathEngine();
    }
    
    [Fact]
    public void TestSimpleExponentialDecay()
    {
        // dy/dx = -y, solution is y = e^(-x)
        var result = engine.SolveODE("-y", 0, 1, 2, ODEMethod.RungeKutta4);
        
        // at x=1, y should be approximately e^(-1) = 0.3678...
        double expected = Math.Exp(-1);
        var solver = new ODESolver();
        double actual = solver.InterpolateSolution(result, 1);
        
        Assert.Equal(expected, actual, 3); // 3 decimal places accuracy
    }
    
    [Fact]
    public void TestExponentialGrowth()
    {
        // dy/dx = y, y(0) = 1, solution is y = e^x
        var result = engine.SolveODE("y", 0, 1, 3);
        
        double expected = Math.Exp(2);
        var solver = new ODESolver();
        double actual = solver.InterpolateSolution(result, 2);
        
        Assert.Equal(expected, actual, 2);
    }
    
    [Fact]
    public void TestLinearODE()
    {
        // dy/dx = 2x, y(0) = 0, solution is y = x^2
        var result = engine.SolveODE("2*x", 0, 0, 5);
        
        // at x=3, y should be 9
        var solver = new ODESolver();
        double actual = solver.InterpolateSolution(result, 3);
        Assert.Equal(9, actual, 1);
    }
    
    [Fact]
    public void TestSinusoidalODE()
    {
        // dy/dx = cos(x), y(0) = 0, solution is y = sin(x)
        var result = engine.SolveODE("cos(x)", 0, 0, Math.PI);
        
        // at x=pi/2, y should be 1
        var solver = new ODESolver();
        double actual = solver.InterpolateSolution(result, Math.PI / 2);
        Assert.Equal(1, actual, 2);
    }
    
    [Fact]
    public void TestEulerMethod()
    {
        // test with euler method (less accurate)
        var result = engine.SolveODE("-y", 0, 1, 1, ODEMethod.Euler, steps: 100);
        
        double expected = Math.Exp(-1);
        var solver = new ODESolver();
        double actual = solver.InterpolateSolution(result, 1);
        
        // euler is less accurate, so we allow more error
        Assert.Equal(expected, actual, 1);
    }
    
    [Fact]
    public void TestMidpointMethod()
    {
        var result = engine.SolveODE("-y", 0, 1, 1, ODEMethod.Midpoint);
        
        double expected = Math.Exp(-1);
        var solver = new ODESolver();
        double actual = solver.InterpolateSolution(result, 1);
        
        // midpoint is between euler and rk4 in accuracy
        Assert.Equal(expected, actual, 2);
    }
    
    [Fact]
    public void TestComplexODE()
    {
        // dy/dx = x*y, y(0) = 1, solution is y = e^(x^2/2)
        var result = engine.SolveODE("x*y", 0, 1, 2);
        
        double x = 1.5;
        double expected = Math.Exp(x * x / 2);
        var solver = new ODESolver();
        double actual = solver.InterpolateSolution(result, x);
        
        Assert.Equal(expected, actual, 2);
    }
    
    [Fact]
    public void TestSystemOfODEs()
    {
        // simple harmonic oscillator
        // y0' = y1 (velocity)
        // y1' = -y0 (acceleration)
        // solution: y0 = cos(t), y1 = -sin(t) for y0(0)=1, y1(0)=0
        
        string[] equations = { "y1", "-y0" };
        double[] initial = { 1, 0 };
        
        var result = engine.SolveSystemODE(equations, 0, initial, Math.PI);
        
        Assert.Equal(2, result.Dimensions);
        Assert.True(result.Points.Count > 100);
        
        // at t=pi/2, should have y0 ≈ 0, y1 ≈ -1
        var midPoint = result.Points[result.Points.Count / 2];
        Assert.Equal(0, midPoint.y[0], 1); // cos(pi/2) = 0
        Assert.Equal(-1, midPoint.y[1], 1); // -sin(pi/2) = -1
    }
    
    [Fact]
    public void TestODEWithVariables()
    {
        // dy/dx = a*y where a is a parameter
        // for now just test with constant value
        var result = engine.SolveODE("-2*y", 0, 1, 1);
        
        double expected = Math.Exp(-2);
        var solver = new ODESolver();
        double actual = solver.InterpolateSolution(result, 1);
        
        Assert.Equal(expected, actual, 2);
    }
    
    [Fact]
    public void TestNonlinearODE()
    {
        // dy/dx = y^2, y(0) = 1
        // solution: y = 1/(1-x) which blows up at x=1
        
        var result = engine.SolveODE("y^2", 0, 1, 0.9, steps: 1000);
        
        // at x=0.5, y should be 2
        var solver = new ODESolver();
        double actual = solver.InterpolateSolution(result, 0.5);
        Assert.Equal(2, actual, 1);
    }
}