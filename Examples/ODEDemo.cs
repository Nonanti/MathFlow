using System;
using MathFlow.Core;
using MathFlow.Core.DifferentialEquations;
using MathFlow.Core.Plotting;

namespace MathFlow.Examples;

public class ODEDemo
{
    public static void Run()
    {
        var engine = new MathEngine();
        
        Console.WriteLine("=== MathFlow ODE Solver Demo ===\n");
        
        // 1. exponential decay
        Console.WriteLine("1. Exponential Decay: dy/dx = -y, y(0) = 1");
        Console.WriteLine("   Analytical solution: y = e^(-x)");
        Console.WriteLine("   ─────────────────────────────────────");
        
        var decay = engine.SolveODE("-y", 0, 1, 3);
        Console.WriteLine($"   y(1) = {decay.GetValueAt(1):F6} (exact: {Math.Exp(-1):F6})");
        Console.WriteLine($"   y(2) = {decay.GetValueAt(2):F6} (exact: {Math.Exp(-2):F6})");
        
        // plot it
        var plotter = engine.CreatePlotter(new PlotConfig { Title = "Exponential Decay" });
        var xVals = new System.Collections.Generic.List<double>();
        var yVals = new System.Collections.Generic.List<double>();
        
        foreach (var point in decay.Points)
        {
            if (point.x <= 3)
            {
                xVals.Add(point.x);
                yVals.Add(point.y);
            }
        }
        
        // kinda hacky but works for visualization
        Console.WriteLine("\n   Graph:");
        for (int row = 0; row < 10; row++)
        {
            Console.Write("   ");
            if (row == 0) Console.Write("1.0 │");
            else if (row == 5) Console.Write("0.5 │");
            else if (row == 9) Console.Write("0.0 │");
            else Console.Write("    │");
            
            for (int col = 0; col < 30; col++)
            {
                double x = col * 3.0 / 30;
                double y = Math.Exp(-x);
                int yPos = (int)((1 - y) * 10);
                
                if (yPos == row)
                    Console.Write("●");
                else if (row == 9)
                    Console.Write("─");
                else
                    Console.Write(" ");
            }
            Console.WriteLine();
        }
        Console.WriteLine("       └" + new string('─', 30));
        Console.WriteLine("        0                    3");
        
        // 2. harmonic oscillator
        Console.WriteLine("\n2. Simple Harmonic Motion (Spring-Mass System)");
        Console.WriteLine("   d²x/dt² = -x  (as system: x' = v, v' = -x)");
        Console.WriteLine("   ──────────────────────────────────────────");
        
        string[] harmonicEqs = { "y1", "-y0" };  // y0 = position, y1 = velocity
        double[] initial = { 1, 0 };  // start at x=1 with v=0
        
        var harmonic = engine.SolveSystemODE(harmonicEqs, 0, initial, 2 * Math.PI);
        
        Console.WriteLine($"   Initial: position = 1, velocity = 0");
        Console.WriteLine($"   After π/2: pos ≈ {harmonic.Points[harmonic.Points.Count / 4].y[0]:F3}, " +
                         $"vel ≈ {harmonic.Points[harmonic.Points.Count / 4].y[1]:F3}");
        Console.WriteLine($"   After π: pos ≈ {harmonic.Points[harmonic.Points.Count / 2].y[0]:F3}, " +
                         $"vel ≈ {harmonic.Points[harmonic.Points.Count / 2].y[1]:F3}");
        
        // 3. logistic growth
        Console.WriteLine("\n3. Logistic Growth: dy/dt = r*y*(1 - y/K)");
        Console.WriteLine("   With r=2, K=100, y(0)=10");
        Console.WriteLine("   ─────────────────────────────────────");
        
        // dy/dt = 2*y*(1 - y/100) = 2*y - 0.02*y^2
        var logistic = engine.SolveODE("2*y - 0.02*y^2", 0, 10, 5);
        
        Console.WriteLine($"   y(1) = {logistic.GetValueAt(1):F1} (population)");
        Console.WriteLine($"   y(3) = {logistic.GetValueAt(3):F1} (approaching capacity)");
        Console.WriteLine($"   y(5) = {logistic.GetValueAt(5):F1} (≈ carrying capacity of 100)");
        
        // 4. compare methods
        Console.WriteLine("\n4. Method Comparison (dy/dx = -2y, y(0) = 1)");
        Console.WriteLine("   ─────────────────────────────────────────");
        
        var euler = engine.SolveODE("-2*y", 0, 1, 1, ODEMethod.Euler, steps: 100);
        var midpoint = engine.SolveODE("-2*y", 0, 1, 1, ODEMethod.Midpoint, steps: 100);
        var rk4 = engine.SolveODE("-2*y", 0, 1, 1, ODEMethod.RungeKutta4, steps: 100);
        double exact = Math.Exp(-2);
        
        Console.WriteLine($"   Exact solution at x=1: {exact:F8}");
        Console.WriteLine($"   Euler method:          {euler.GetValueAt(1):F8} (error: {Math.Abs(euler.GetValueAt(1) - exact):E2})");
        Console.WriteLine($"   Midpoint method:       {midpoint.GetValueAt(1):F8} (error: {Math.Abs(midpoint.GetValueAt(1) - exact):E2})");
        Console.WriteLine($"   Runge-Kutta 4:         {rk4.GetValueAt(1):F8} (error: {Math.Abs(rk4.GetValueAt(1) - exact):E2})");
        
        // 5. stiff equation example
        Console.WriteLine("\n5. Nonlinear ODE: dy/dx = -y³ + sin(x)");
        Console.WriteLine("   ──────────────────────────────────────");
        
        var nonlinear = engine.SolveODE("-y^3 + sin(x)", 0, 0, 10, steps: 1000);
        
        Console.WriteLine($"   y(π/2) = {nonlinear.GetValueAt(Math.PI / 2):F4}");
        Console.WriteLine($"   y(π)   = {nonlinear.GetValueAt(Math.PI):F4}");
        Console.WriteLine($"   y(2π)  = {nonlinear.GetValueAt(2 * Math.PI):F4}");
        
        Console.WriteLine("\n✨ ODE demo complete!");
        Console.WriteLine("Note: RK4 is usually the best balance of accuracy and speed");
    }
}