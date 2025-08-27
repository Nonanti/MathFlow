using System;
using System.Collections.Generic;
using System.Linq;
namespace MathFlow.Core.Plotting;
/// <summary>
/// Represents data for plotting a mathematical function
/// </summary>
public class PlotData
{
    public string Expression { get; set; }
    public List<PlotPoint> Points { get; set; }
    public double MinX { get; set; }
    public double MaxX { get; set; }
    public double MinY { get; set; }
    public double MaxY { get; set; }
    public string? Label { get; set; }
    public PlotStyle Style { get; set; }
    
    public PlotData(string expression)
    {
        Expression = expression;
        Points = new List<PlotPoint>();
        Style = new PlotStyle();
    }
    
    /// <summary>
    /// Auto-scale Y axis based on data
    /// </summary>
    public void AutoScale()
    {
        if (Points.Count == 0) return;
        
        MinY = Points.Min(p => p.Y);
        MaxY = Points.Max(p => p.Y);
        
        var range = MaxY - MinY;
        if (Math.Abs(range) < 0.0001)
        {
            MinY -= 1;
            MaxY += 1;
        }
        else
        {
            MinY -= range * 0.1;
            MaxY += range * 0.1;
        }
    }
}
/// <summary>
/// Represents a point in 2D space
/// </summary>
public struct PlotPoint
{
    public double X { get; set; }
    public double Y { get; set; }
    
    public PlotPoint(double x, double y)
    {
        X = x;
        Y = y;
    }
}
/// <summary>
/// Styling options for plots
/// </summary>
public class PlotStyle
{
    public string Color { get; set; } = "blue";
    public LineStyle LineStyle { get; set; } = LineStyle.Solid;
    public double LineWidth { get; set; } = 1.5;
    public bool ShowPoints { get; set; } = false;
    public MarkerStyle MarkerStyle { get; set; } = MarkerStyle.Circle;
}
public enum LineStyle
{
    Solid,
    Dashed,
    Dotted,
    DashDot,
    None
}
public enum MarkerStyle
{
    None,
    Circle,
    Square,
    Triangle,
    Cross,
    Plus,
    Star
}
/// <summary>
/// Configuration for the plot
/// </summary>
public class PlotConfig
{
    public int Width { get; set; } = 800;
    public int Height { get; set; } = 600;
    public string Title { get; set; } = "";
    public string XLabel { get; set; } = "x";
    public string YLabel { get; set; } = "y";
    public bool ShowGrid { get; set; } = true;
    public bool ShowLegend { get; set; } = true;
    public bool ShowAxes { get; set; } = true;
    public string BackgroundColor { get; set; } = "white";
    public string GridColor { get; set; } = "#e0e0e0";
    public string AxisColor { get; set; } = "black";
    public double? MinX { get; set; }
    public double? MaxX { get; set; }
    public double? MinY { get; set; }
    public double? MaxY { get; set; }
}