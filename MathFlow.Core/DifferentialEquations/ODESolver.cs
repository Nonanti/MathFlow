using System;
using System.Collections.Generic;
using MathFlow.Core.Interfaces;
using MathFlow.Core.Parser;
namespace MathFlow.Core.DifferentialEquations;
/// <summary>
/// Solves ordinary differential equations numerically
/// </summary>
public class ODESolver
{
    private readonly ExpressionParser parser;
    
    public ODESolver()
    {
        parser = new ExpressionParser();
    }
    
    /// <summary>
    /// Solve ODE dy/dx = f(x,y) with initial condition y(x0) = y0
    /// </summary>
    public ODEResult Solve(string equation, double x0, double y0, double xEnd, ODEMethod method = ODEMethod.RungeKutta4, int steps = 1000)
    {
        var expr = parser.Parse(equation);
        return Solve(expr, x0, y0, xEnd, method, steps);
    }
    
    public ODEResult Solve(IExpression equation, double x0, double y0, double xEnd, ODEMethod method = ODEMethod.RungeKutta4, int steps = 1000)
    {
        var points = new List<(double x, double y)>();
        double h = (xEnd - x0) / steps;
        double x = x0;
        double y = y0;
        
        points.Add((x, y));
        
        for (int i = 0; i < steps; i++)
        {
            switch (method)
            {
                case ODEMethod.Euler:
                    y = EulerStep(equation, x, y, h);
                    break;
                case ODEMethod.RungeKutta2:
                    y = RungeKutta2Step(equation, x, y, h);
                    break;
                case ODEMethod.RungeKutta4:
                    y = RungeKutta4Step(equation, x, y, h);
                    break;
                case ODEMethod.Midpoint:
                    y = MidpointStep(equation, x, y, h);
                    break;
                case ODEMethod.AdamsBashforth:
                    if (points.Count < 3)
                    {
                        y = RungeKutta4Step(equation, x, y, h);
                    }
                    else
                    {
                        y = AdamsBashforthStep(equation, points, x, y, h);
                    }
                    break;
                default:
                    throw new NotSupportedException($"Method {method} not implemented yet");
            }
            
            x += h;
            points.Add((x, y));
        }
        
        return new ODEResult
        {
            Points = points,
            Method = method,
            StepSize = h
        };
    }
    
    private double EulerStep(IExpression f, double x, double y, double h)
    {
        var vars = new Dictionary<string, double> { ["x"] = x, ["y"] = y };
        double dydx = f.Evaluate(vars);
        return y + h * dydx;
    }
    
    private double RungeKutta4Step(IExpression f, double x, double y, double h)
    {
        var vars = new Dictionary<string, double>();
        
        vars["x"] = x;
        vars["y"] = y;
        double k1 = f.Evaluate(vars);
        
        vars["x"] = x + h / 2;
        vars["y"] = y + h * k1 / 2;
        double k2 = f.Evaluate(vars);
        
        vars["x"] = x + h / 2;
        vars["y"] = y + h * k2 / 2;
        double k3 = f.Evaluate(vars);
        
        vars["x"] = x + h;
        vars["y"] = y + h * k3;
        double k4 = f.Evaluate(vars);
        
        return y + (h / 6) * (k1 + 2 * k2 + 2 * k3 + k4);
    }
    
    private double MidpointStep(IExpression f, double x, double y, double h)
    {
        var vars = new Dictionary<string, double>();
        
        vars["x"] = x;
        vars["y"] = y;
        double k1 = f.Evaluate(vars);
        
        vars["x"] = x + h / 2;
        vars["y"] = y + h * k1 / 2;
        double k2 = f.Evaluate(vars);
        
        return y + h * k2;
    }
    
    /// <summary>
    /// Runge-Kutta 2nd order (Heun's method)
    /// </summary>
    private double RungeKutta2Step(IExpression f, double x, double y, double h)
    {
        var vars = new Dictionary<string, double>();
        
        vars["x"] = x;
        vars["y"] = y;
        double k1 = f.Evaluate(vars);
        
        vars["x"] = x + h;
        vars["y"] = y + h * k1;
        double k2 = f.Evaluate(vars);
        
        return y + (h / 2) * (k1 + k2);
    }
    
    /// <summary>
    /// Adams-Bashforth 3-step method (3rd order)
    /// </summary>
    private double AdamsBashforthStep(IExpression f, List<(double x, double y)> previousPoints, double x, double y, double h)
    {
        var vars = new Dictionary<string, double>();
        int n = previousPoints.Count;
        
        if (n < 3)
        {
            throw new InvalidOperationException("Adams-Bashforth requires at least 3 previous points");
        }
        
        vars["x"] = x;
        vars["y"] = y;
        double f_n = f.Evaluate(vars);
        
        var prev1 = previousPoints[n - 1];
        vars["x"] = prev1.x;
        vars["y"] = prev1.y;
        double f_n1 = f.Evaluate(vars);
        
        var prev2 = previousPoints[n - 2];
        vars["x"] = prev2.x;
        vars["y"] = prev2.y;
        double f_n2 = f.Evaluate(vars);
        
        return y + (h / 12.0) * (23 * f_n - 16 * f_n1 + 5 * f_n2);
    }
    
    /// <summary>
    /// Solve system of ODEs
    /// </summary>
    public SystemODEResult SolveSystem(string[] equations, double t0, double[] y0, double tEnd, int steps = 1000)
    {
        int n = equations.Length;
        if (y0.Length != n)
            throw new ArgumentException("Number of initial conditions must match number of equations");
        
        var exprs = new IExpression[n];
        for (int i = 0; i < n; i++)
        {
            exprs[i] = parser.Parse(equations[i]);
        }
        
        var points = new List<(double t, double[] y)>();
        double h = (tEnd - t0) / steps;
        double t = t0;
        double[] y = (double[])y0.Clone();
        
        points.Add((t, (double[])y.Clone()));
        
        for (int step = 0; step < steps; step++)
        {
            y = RungeKutta4SystemStep(exprs, t, y, h);
            t += h;
            points.Add((t, (double[])y.Clone()));
        }
        
        return new SystemODEResult
        {
            Points = points,
            Dimensions = n,
            StepSize = h
        };
    }
    
    private double[] RungeKutta4SystemStep(IExpression[] equations, double t, double[] y, double h)
    {
        int n = equations.Length;
        double[] k1 = new double[n];
        double[] k2 = new double[n];
        double[] k3 = new double[n];
        double[] k4 = new double[n];
        
        var vars = new Dictionary<string, double> { ["t"] = t };
        
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                vars[$"y{j}"] = y[j];
            }
            k1[i] = equations[i].Evaluate(vars);
        }
        
        double[] y2 = new double[n];
        for (int i = 0; i < n; i++)
        {
            y2[i] = y[i] + h * k1[i] / 2;
        }
        
        vars["t"] = t + h / 2;
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                vars[$"y{j}"] = y2[j];
            }
            k2[i] = equations[i].Evaluate(vars);
        }
        
        double[] y3 = new double[n];
        for (int i = 0; i < n; i++)
        {
            y3[i] = y[i] + h * k2[i] / 2;
        }
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                vars[$"y{j}"] = y3[j];
            }
            k3[i] = equations[i].Evaluate(vars);
        }
        
        double[] y4 = new double[n];
        for (int i = 0; i < n; i++)
        {
            y4[i] = y[i] + h * k3[i];
        }
        
        vars["t"] = t + h;
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                vars[$"y{j}"] = y4[j];
            }
            k4[i] = equations[i].Evaluate(vars);
        }
        
        for (int i = 0; i < n; i++)
        {
            y[i] = y[i] + (h / 6) * (k1[i] + 2 * k2[i] + 2 * k3[i] + k4[i]);
        }
        
        return y;
    }
    
    public double InterpolateSolution(ODEResult result, double x)
    {
        var points = result.Points;
        
        if (x <= points[0].x)
            return points[0].y;
        
        if (x >= points[points.Count - 1].x)
            return points[points.Count - 1].y;
        
        for (int i = 0; i < points.Count - 1; i++)
        {
            if (x >= points[i].x && x <= points[i + 1].x)
            {
                double t = (x - points[i].x) / (points[i + 1].x - points[i].x);
                return points[i].y + t * (points[i + 1].y - points[i].y);
            }
        }
        
        if (x < points[0].x)
        {
            double slope = (points[1].y - points[0].y) / (points[1].x - points[0].x);
            return points[0].y + slope * (x - points[0].x);
        }
        else
        {
            int n = points.Count;
            double slope = (points[n - 1].y - points[n - 2].y) / (points[n - 1].x - points[n - 2].x);
            return points[n - 1].y + slope * (x - points[n - 1].x);
        }
    }
}
public class ODEResult
{
    public List<(double x, double y)> Points { get; set; } = new();
    public ODEMethod Method { get; set; }
    public double StepSize { get; set; }
}
public class SystemODEResult
{
    public List<(double t, double[] y)> Points { get; set; } = new();
    public int Dimensions { get; set; }
    public double StepSize { get; set; }
}
public enum ODEMethod
{
    Euler,
    RungeKutta2,
    RungeKutta4,
    Midpoint,
    AdamsBashforth
}