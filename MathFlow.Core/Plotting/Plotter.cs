using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using MathFlow.Core.Interfaces;
namespace MathFlow.Core.Plotting;
/// <summary>
/// Terminal-based plotting engine for mathematical expressions
/// </summary>
public class Plotter
{
    private readonly List<PlotData> plots;
    private readonly PlotConfig config;
    
    public Plotter(PlotConfig? config = null)
    {
        this.config = config ?? new PlotConfig();
        this.plots = new List<PlotData>();
    }
    
    /// <summary>
    /// Add a function to plot
    /// </summary>
    public Plotter AddFunction(IExpression expression, double minX, double maxX, int points = 500, string? label = null)
    {
        var plot = new PlotData(expression.ToString())
        {
            MinX = minX,
            MaxX = maxX,
            Label = label ?? expression.ToString()
        };
        
        var step = (maxX - minX) / (points - 1);
        var variables = new Dictionary<string, double>();
        
        for (int i = 0; i < points; i++)
        {
            var x = minX + i * step;
            variables["x"] = x;
            
            try
            {
                var y = expression.Evaluate(variables);
                
                if (!double.IsNaN(y) && !double.IsInfinity(y))
                {
                    plot.Points.Add(new PlotPoint(x, y));
                }
            }
            catch
            {
            }
        }
        
        plot.AutoScale();
        plots.Add(plot);
        
        return this;
    }
    
    /// <summary>
    /// Add a function from string expression
    /// </summary>
    public Plotter AddFunction(string expression, double minX, double maxX, int points = 500, string? label = null)
    {
        var parser = new Parser.ExpressionParser();
        var expr = parser.Parse(expression);
        return AddFunction(expr, minX, maxX, points, label ?? expression);
    }
    
    /// <summary>
    /// Generate ASCII art chart for terminal display
    /// </summary>
    public string ToAsciiChart(int width = 80, int height = 24)
    {
        if (plots.Count == 0) return "No data to plot";
        
        var sb = new StringBuilder();
        
        double minX = plots.Min(p => p.MinX);
        double maxX = plots.Max(p => p.MaxX);
        double minY = config.MinY ?? plots.Min(p => p.MinY);
        double maxY = config.MaxY ?? plots.Max(p => p.MaxY);
        
        char[,] grid = new char[height, width];
        for (int i = 0; i < height; i++)
            for (int j = 0; j < width; j++)
                grid[i, j] = ' ';
        
        if (config.ShowAxes)
        {
            int xAxisPos = (int)((0 - minX) / (maxX - minX) * (width - 1));
            if (xAxisPos >= 0 && xAxisPos < width)
            {
                for (int i = 0; i < height; i++)
                    grid[i, xAxisPos] = '│';
            }
            
            int yAxisPos = (int)((maxY - 0) / (maxY - minY) * (height - 1));
            if (yAxisPos >= 0 && yAxisPos < height)
            {
                for (int j = 0; j < width; j++)
                    grid[yAxisPos, j] = '─';
                    
                if (xAxisPos >= 0 && xAxisPos < width)
                    grid[yAxisPos, xAxisPos] = '┼';
            }
        }
        
        char[] markers = { '●', '○', '■', '□', '▲', '△', '▼', '▽' };
        int markerIndex = 0;
        
        foreach (var plot in plots)
        {
            char marker = markers[markerIndex % markers.Length];
            markerIndex++;
            
            foreach (var point in plot.Points)
            {
                int x = (int)((point.X - minX) / (maxX - minX) * (width - 1));
                int y = (int)((maxY - point.Y) / (maxY - minY) * (height - 1));
                
                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    if (grid[y, x] == ' ' || grid[y, x] == '─' || grid[y, x] == '│')
                        grid[y, x] = marker;
                }
            }
        }
        
        if (!string.IsNullOrEmpty(config.Title))
        {
            sb.AppendLine(config.Title.PadLeft(width / 2 + config.Title.Length / 2));
            sb.AppendLine();
        }
        
        sb.AppendLine($"{maxY.ToString("F2", CultureInfo.InvariantCulture),8} ┤");
        
        for (int i = 0; i < height; i++)
        {
            if (i == height / 2)
                sb.Append($"{((maxY + minY) / 2).ToString("F2", CultureInfo.InvariantCulture),8} ┤");
            else if (i == height - 1)
                sb.Append($"{minY.ToString("F2", CultureInfo.InvariantCulture),8} ┤");
            else
                sb.Append("         │");
            
            for (int j = 0; j < width; j++)
            {
                sb.Append(grid[i, j]);
            }
            sb.AppendLine();
        }
        
        sb.Append("         └");
        sb.AppendLine(new string('─', width));
        
        sb.Append("          ");
        sb.Append(minX.ToString("F2", CultureInfo.InvariantCulture));
        sb.Append(new string(' ', width / 2 - 10));
        sb.Append(((minX + maxX) / 2).ToString("F2", CultureInfo.InvariantCulture));
        sb.Append(new string(' ', width / 2 - 10));
        sb.AppendLine(maxX.ToString("F2", CultureInfo.InvariantCulture));
        
        if (config.ShowLegend && plots.Any(p => !string.IsNullOrEmpty(p.Label)))
        {
            sb.AppendLine();
            sb.AppendLine("Legend:");
            markerIndex = 0;
            foreach (var plot in plots)
            {
                char marker = markers[markerIndex % markers.Length];
                markerIndex++;
                sb.AppendLine($"  {marker} {plot.Label}");
            }
        }
        
        return sb.ToString();
    }
    
    /// <summary>
    /// Display the chart in terminal (convenience method)
    /// </summary>
    public void Display(int width = 80, int height = 24)
    {
        Console.WriteLine(ToAsciiChart(width, height));
    }
    
    /// <summary>
    /// Clear all plots
    /// </summary>
    public Plotter Clear()
    {
        plots.Clear();
        return this;
    }
}