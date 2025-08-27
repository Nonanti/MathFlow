using MathFlow.Core.Expressions;
namespace MathFlow.Core.Extensions;
public static class MathExtensions
{
    public static double ToRadians(this double degrees) {
        return degrees * Math.PI / 180.0;
    }
    
    public static double ToDegrees(this double radians) {
        return radians * 180.0 / Math.PI;
    }
    
    /// <summary>
    /// Checks if a number is approximately equal to another
    /// </summary>
    public static bool ApproximatelyEquals(this double value, double other, double tolerance = 1e-10)
    {
        return Math.Abs(value - other) < tolerance;
    }
    
    /// <summary>
    /// Clamps a value between min and max
    /// </summary>
    public static double Clamp(this double value, double min, double max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }
    
    /// <summary>
    /// Linear interpolation between two values
    /// </summary>
    public static double Lerp(double from, double to, double t)
    {
        return from + (to - from) * t.Clamp(0, 1);
    }
    
    /// <summary>
    /// Maps a value from one range to another
    /// </summary>
    public static double Map(this double value, double fromMin, double fromMax, double toMin, double toMax)
    {
        var normalized = (value - fromMin) / (fromMax - fromMin);
        return toMin + normalized * (toMax - toMin);
    }
}