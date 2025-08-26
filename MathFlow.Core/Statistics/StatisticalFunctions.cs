namespace MathFlow.Core.Statistics;

// Basic stats functions
public static class StatisticalFunctions
{
    /// <summary>
    /// Calculates the mean (average) of values
    /// </summary>
    public static double Mean(IEnumerable<double> values)
    {
        var enumerable = values.ToList();
        if (!enumerable.Any())
            throw new ArgumentException("Cannot calculate mean of empty collection");
        
        return enumerable.Average();
    }
    
    /// <summary>
    /// Calculates the median of values
    /// </summary>
    public static double Median(IEnumerable<double> values)
    {
        var sorted = values.OrderBy(x => x).ToList();
        if (!sorted.Any())
            throw new ArgumentException("Cannot calculate median of empty collection");
        
        int n = sorted.Count;
        if (n % 2 == 1)
            return sorted[n / 2];
        
        return (sorted[n / 2 - 1] + sorted[n / 2]) / 2.0;
    }
    
    /// <summary>
    /// Calculates the mode (most frequent value)
    /// </summary>
    public static double Mode(IEnumerable<double> values)
    {
        var groups = values.GroupBy(x => x)
            .OrderByDescending(g => g.Count())
            .ThenBy(g => g.Key);
        
        var firstGroup = groups.FirstOrDefault();
        if (firstGroup == null)
            throw new ArgumentException("Cannot calculate mode of empty collection");
        
        return firstGroup.Key;
    }
    
    /// <summary>
    /// Calculates the variance
    /// </summary>
    public static double Variance(IEnumerable<double> values, bool population = false)
    {
        var data = values.ToList();
        if (!data.Any())
            throw new ArgumentException("Cannot calculate variance of empty collection");
        
        double mean = Mean(data);
        double sumSquaredDiff = data.Sum(x => Math.Pow(x - mean, 2));
        
        int denominator = population ? data.Count : data.Count - 1;
        if (denominator == 0)
            return 0;
        
        return sumSquaredDiff / denominator;
    }
    
    /// <summary>
    /// Calculates the standard deviation
    /// </summary>
    public static double StandardDeviation(IEnumerable<double> values, bool population = false)
    {
        return Math.Sqrt(Variance(values, population));
    }
    
    /// <summary>
    /// Calculates the covariance between two datasets
    /// </summary>
    public static double Covariance(IEnumerable<double> x, IEnumerable<double> y, bool population = false)
    {
        var xList = x.ToList();
        var yList = y.ToList();
        
        if (xList.Count != yList.Count)
            throw new ArgumentException("Datasets must have same length");
        
        if (!xList.Any())
            throw new ArgumentException("Cannot calculate covariance of empty collections");
        
        double xMean = Mean(xList);
        double yMean = Mean(yList);
        
        double sum = xList.Zip(yList, (xi, yi) => (xi - xMean) * (yi - yMean)).Sum();
        
        int denominator = population ? xList.Count : xList.Count - 1;
        if (denominator == 0)
            return 0;
        
        return sum / denominator;
    }
    
    /// <summary>
    /// Calculates the Pearson correlation coefficient
    /// </summary>
    public static double Correlation(IEnumerable<double> x, IEnumerable<double> y)
    {
        var xList = x.ToList();
        var yList = y.ToList();
        
        double xStdDev = StandardDeviation(xList);
        double yStdDev = StandardDeviation(yList);
        
        if (xStdDev < 1e-10 || yStdDev < 1e-10)
            return 0;
        
        return Covariance(xList, yList) / (xStdDev * yStdDev);
    }
    
    /// <summary>
    /// Calculates percentile
    /// </summary>
    public static double Percentile(IEnumerable<double> values, double percentile)
    {
        if (percentile < 0 || percentile > 100)
            throw new ArgumentException("Percentile must be between 0 and 100");
        
        var sorted = values.OrderBy(x => x).ToList();
        if (!sorted.Any())
            throw new ArgumentException("Cannot calculate percentile of empty collection");
        
        if (percentile == 0) return sorted[0];
        if (percentile == 100) return sorted[^1];
        
        double index = (sorted.Count - 1) * percentile / 100.0;
        int lower = (int)Math.Floor(index);
        int upper = (int)Math.Ceiling(index);
        
        if (lower == upper)
            return sorted[lower];
        
        double weight = index - lower;
        return sorted[lower] * (1 - weight) + sorted[upper] * weight;
    }
    
    /// <summary>
    /// Calculates quartiles (Q1, Q2/median, Q3)
    /// </summary>
    public static (double Q1, double Q2, double Q3) Quartiles(IEnumerable<double> values)
    {
        var data = values.ToList();
        return (
            Percentile(data, 25),
            Percentile(data, 50),
            Percentile(data, 75)
        );
    }
    
    /// <summary>
    /// Linear regression - returns (slope, intercept)
    /// </summary>
    public static (double Slope, double Intercept) LinearRegression(IEnumerable<double> x, IEnumerable<double> y)
    {
        var xList = x.ToList();
        var yList = y.ToList();
        
        if (xList.Count != yList.Count)
            throw new ArgumentException("Datasets must have same length");
        
        if (xList.Count < 2)
            throw new ArgumentException("Need at least 2 points for linear regression");
        
        double xMean = Mean(xList);
        double yMean = Mean(yList);
        
        double numerator = xList.Zip(yList, (xi, yi) => (xi - xMean) * (yi - yMean)).Sum();
        double denominator = xList.Sum(xi => Math.Pow(xi - xMean, 2));
        
        if (Math.Abs(denominator) < 1e-10)
            throw new InvalidOperationException("Cannot perform regression on vertical line");
        
        double slope = numerator / denominator;
        double intercept = yMean - slope * xMean;
        
        return (slope, intercept);
    }
}