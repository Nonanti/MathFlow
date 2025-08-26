using System;
using System.Numerics;

namespace MathFlow.Core.Precision;

/// <summary>
/// Provides mathematical functions with arbitrary precision arithmetic
/// </summary>
public static class ArbitraryPrecisionMath
{
    // cache these cause computing pi is slow af
    private static readonly BigDecimal PiCache = ComputePi(500);
    private static readonly BigDecimal ECache = ComputeE(500);
    
    public static BigDecimal Pi(int precision = 100)
    {
        if (precision <= 500)
            return TruncateToPrecision(PiCache, precision);
        return ComputePi(precision);
    }
    
    public static BigDecimal E(int precision = 100)
    {
        // TODO: maybe cache more values?
        if (precision <= 500)
            return TruncateToPrecision(ECache, precision);
        return ComputeE(precision);
    }
    
    private static BigDecimal ComputePi(int precision)
    {
        // found this formula online - BBP algorithm i think
        var workingPrecision = precision + 20; // need extra for rounding errors
        var sum = BigDecimal.Zero;
        var sixteen = new BigDecimal(16);
        
        for (int k = 0; k < precision; k++)
        {
            var kDecimal = new BigDecimal(k);
            var eightK = new BigDecimal(8) * kDecimal;
            
            // this part is ugly but it works
            var term1 = new BigDecimal(4) / (eightK + BigDecimal.One);
            var term2 = new BigDecimal(2) / (eightK + new BigDecimal(4));
            var term3 = BigDecimal.One / (eightK + new BigDecimal(5));
            var term4 = BigDecimal.One / (eightK + new BigDecimal(6));
            
            var bracketed = term1 - term2 - term3 - term4;
            var divisor = Pow(sixteen, k, workingPrecision);
            var term = bracketed / divisor;
            
            var newSum = sum + term;
            if (sum == newSum && k > precision / 2) break; // converged enough
            sum = newSum;
        }
        
        return TruncateToPrecision(sum, precision);
    }
    
    private static BigDecimal ComputeE(int precision)
    {
        // basic taylor series - e = 1 + 1/1! + 1/2! + ...
        var workingPrecision = precision + 30;
        var sum = BigDecimal.Zero;
        var factorial = BigDecimal.One;
        
        for (int n = 0; n < workingPrecision * 2; n++) // *2 is probably overkill but whatever
        {
            if (n > 0)
                factorial = factorial * new BigDecimal(n);
            
            var term = BigDecimal.One / factorial;
            var newSum = sum + term;
            
            if (sum == newSum && n > precision / 2) break;
            sum = newSum;
        }
        
        return TruncateToPrecision(sum, precision);
    }
    
    /// <summary>
    /// Calculate sine using Taylor series
    /// </summary>
    public static BigDecimal Sin(BigDecimal x, int precision = 100)
    {
        // reduce angle to something manageable
        x = ReduceAngle(x, precision);
        
        var pi = Pi(precision);
        var piHalf = pi / new BigDecimal(2);
        var sign = BigDecimal.One;
        
        if (x > piHalf)
        {
            x = pi - x;
        }
        else if (x < -piHalf)
        {
            x = -pi - x;
        }
        
        // taylor series: x - x^3/3! + x^5/5! - ...
        var sum = BigDecimal.Zero;
        var term = x;
        var x2 = x * x;
        
        for (int n = 1; n < precision * 2; n += 2)
        {
            var newSum = sum + term;
            if (sum == newSum) break;
            sum = newSum;
            
            // FIXME: this could be optimized
            term = -term * x2 / new BigDecimal((n + 1) * (n + 2));
        }
        
        return sign * sum;
    }
    
    public static BigDecimal Cos(BigDecimal x, int precision = 100)
    {
        x = ReduceAngle(x, precision);
        
        var pi = Pi(precision);
        var piHalf = pi / new BigDecimal(2);
        var sign = BigDecimal.One;
        
        // cos(x) = -cos(pi - x)
        if (x > piHalf)
        {
            x = pi - x;
            sign = -sign;
        }
        else if (x < -piHalf)
        {
            x = -pi - x;
            sign = -sign;
        }
        
        var sum = BigDecimal.One;
        var term = BigDecimal.One;
        var x2 = x * x;
        
        for (int n = 2; n < precision * 2; n += 2)
        {
            term = -term * x2 / new BigDecimal(n * (n - 1));
            var newSum = sum + term;
            if (sum == newSum) break;
            sum = newSum;
        }
        
        return sign * sum;
    }
    
    // tan = sin/cos
    public static BigDecimal Tan(BigDecimal x, int precision = 100)
    {
        var sinX = Sin(x, precision + 10);
        var cosX = Cos(x, precision + 10);
        
        if (cosX == BigDecimal.Zero)
            throw new ArgumentException("Tangent undefined at this point");
        
        return sinX / cosX;
    }
    
    /// <summary>
    /// Arctan calculation
    /// </summary>
    public static BigDecimal ArcTan(BigDecimal x, int precision = 100)
    {
        // For |x| > 1, use arctan(x) = π/2 - arctan(1/x) * sign(x)
        if (x > BigDecimal.One || x < -BigDecimal.One)
        {
            var piHalf = Pi(precision) / new BigDecimal(2);
            var sign = x < BigDecimal.Zero ? -BigDecimal.One : BigDecimal.One;
            return sign * piHalf - ArcTan(BigDecimal.One / x, precision);
        }
        
        return ArcTanSeries(x, precision);
    }
    
    private static BigDecimal ArcTanSeries(BigDecimal x, int precision)
    {
        var sum = BigDecimal.Zero;
        var term = x;
        var x2 = x * x;
        
        for (int n = 1; n < precision * 4; n += 2)
        {
            var newSum = sum + term / new BigDecimal(n);
            if (sum == newSum) break;
            sum = newSum;
            term = -term * x2;
        }
        
        return sum;
    }
    
    public static BigDecimal ArcSin(BigDecimal x, int precision = 100)
    {
        if (x > BigDecimal.One || x < -BigDecimal.One)
            throw new ArgumentException("ArcSin input must be in range [-1, 1]");
        
        if (x == BigDecimal.One) return Pi(precision) / new BigDecimal(2);
        if (x == -BigDecimal.One) return -Pi(precision) / new BigDecimal(2);
        
        var oneMinusX2 = BigDecimal.One - x * x;
        var sqrt = Sqrt(oneMinusX2, precision + 10);
        return ArcTan(x / sqrt, precision);
    }
    
    public static BigDecimal ArcCos(BigDecimal x, int precision = 100)
    {
        if (x > BigDecimal.One || x < -BigDecimal.One)
            throw new ArgumentException("ArcCos input must be in range [-1, 1]");
        
        var piHalf = Pi(precision) / new BigDecimal(2);
        return piHalf - ArcSin(x, precision);
    }
    
    public static BigDecimal Exp(BigDecimal x, int precision = 100)
    {
        // For large x, use e^x = (e^(x/n))^n to improve convergence
        if (x > new BigDecimal(10) || x < new BigDecimal(-10))
        {
            var n = (int)(double)x / 10 + 1;
            var reduced = x / new BigDecimal(n);
            var expReduced = Exp(reduced, precision + 10);
            return Pow(expReduced, n, precision);
        }
        
        var sum = BigDecimal.One;
        var term = BigDecimal.One;
        
        for (int n = 1; n < precision * 3; n++)
        {
            term = term * x / new BigDecimal(n);
            var newSum = sum + term;
            if (sum == newSum) break;
            sum = newSum;
        }
        
        return sum;
    }
    
    /// <summary>
    /// Natural logarithm implementation
    /// TODO: optimize for large numbers  
    /// </summary>
    public static BigDecimal Ln(BigDecimal x, int precision = 100)
    {
        if (x <= BigDecimal.Zero)
            throw new ArgumentException("Logarithm input must be positive");
        
        // Special case for x = 1
        if (x == BigDecimal.One)
            return BigDecimal.Zero;
        
        var workingPrecision = precision + 20;
        
        // For better convergence, use ln(x) = 2*arctanh((x-1)/(x+1))
        // arctanh(z) = z + z³/3 + z⁵/5 + ...
        var numerator = x - BigDecimal.One;
        var denominator = x + BigDecimal.One;
        var z = numerator / denominator;
        var z2 = z * z;
        
        var sum = BigDecimal.Zero;
        var term = z;
        
        for (int n = 1; n < workingPrecision * 4; n += 2)
        {
            var termContribution = term / new BigDecimal(n);
            var newSum = sum + termContribution;
            if (sum == newSum) break; // converged
            sum = newSum;
            term = term * z2;
        }
        
        return new BigDecimal(2) * sum;
    }
    
    public static BigDecimal Log10(BigDecimal x, int precision = 100)
    {
        var ln10 = Ln(new BigDecimal(10), precision + 10);
        return Ln(x, precision + 10) / ln10;
    }
    
    public static BigDecimal LogBase(BigDecimal x, BigDecimal baseValue, int precision = 100)
    {
        return Ln(x, precision + 10) / Ln(baseValue, precision + 10);
    }
    
    /// <summary>
    /// Square root using Newton-Raphson
    /// </summary>
    public static BigDecimal Sqrt(BigDecimal x, int precision = 100)
    {
        if (x < BigDecimal.Zero)
            throw new ArgumentException("Cannot compute square root of negative number");
        
        if (x == BigDecimal.Zero) return BigDecimal.Zero;
        if (x == BigDecimal.One) return BigDecimal.One;
        
        // Initial guess
        var guess = x / new BigDecimal(2);
        var two = new BigDecimal(2);
        
        for (int i = 0; i < precision; i++)
        {
            var newGuess = (guess + x / guess) / two;
            if (newGuess == guess) break;
            guess = newGuess;
        }
        
        return guess;
    }
    
    public static BigDecimal Pow(BigDecimal x, int n, int precision = 100)
    {
        if (n == 0) return BigDecimal.One;
        if (n < 0) return BigDecimal.One / Pow(x, -n, precision);
        
        var result = BigDecimal.One;
        var current = x;
        
        while (n > 0)
        {
            if ((n & 1) == 1)
                result = result * current;
            current = current * current;
            n >>= 1;
        }
        
        return result;
    }
    
    // x^y = e^(y*ln(x))
    public static BigDecimal Pow(BigDecimal x, BigDecimal y, int precision = 100)
    {
        // Check for integer power
        var yDouble = (double)y;
        if (Math.Abs(yDouble % 1) < 1e-10)
        {
            return Pow(x, (int)yDouble, precision);
        }
        
        // x^y = exp(y * ln(x))
        if (x <= BigDecimal.Zero)
            throw new ArgumentException("Base must be positive for non-integer exponents");
        
        return Exp(y * Ln(x, precision + 10), precision);
    }
    
    public static BigDecimal Sinh(BigDecimal x, int precision = 100)
    {
        var expX = Exp(x, precision + 10);
        var expNegX = BigDecimal.One / expX;
        return (expX - expNegX) / new BigDecimal(2);
    }
    
    public static BigDecimal Cosh(BigDecimal x, int precision = 100)
    {
        var expX = Exp(x, precision + 10);
        var expNegX = BigDecimal.One / expX;
        return (expX + expNegX) / new BigDecimal(2);
    }
    
    public static BigDecimal Tanh(BigDecimal x, int precision = 100)
    {
        var expX = Exp(x, precision + 10);
        var expNegX = BigDecimal.One / expX;
        return (expX - expNegX) / (expX + expNegX);
    }
    
    public static BigDecimal Abs(BigDecimal x)
    {
        return x < BigDecimal.Zero ? -x : x;
    }
    
    public static BigDecimal Floor(BigDecimal x)
    {
        var str = x.ToString();
        var dotIndex = str.IndexOf('.');
        if (dotIndex == -1) return x;
        
        var intPart = BigDecimal.Parse(str.Substring(0, dotIndex));
        if (x < BigDecimal.Zero && x != intPart)
            return intPart - BigDecimal.One;
        return intPart;
    }
    
    public static BigDecimal Ceiling(BigDecimal x)
    {
        var floor = Floor(x);
        return x == floor ? floor : floor + BigDecimal.One;
    }
    
    // round to nearest int
    public static BigDecimal Round(BigDecimal x)
    {
        var half = new BigDecimal(BigInteger.One, 1); // 0.5
        return Floor(x + half);
    }
    
    public static BigDecimal Sign(BigDecimal x)
    {
        if (x < BigDecimal.Zero) return new BigDecimal(-1);
        if (x > BigDecimal.Zero) return BigDecimal.One;
        return BigDecimal.Zero;
    }
    
    // n!
    public static BigDecimal Factorial(BigDecimal n)
    {
        var nDouble = (double)n;
        if (nDouble < 0 || Math.Abs(nDouble % 1) > 1e-10)
            throw new ArgumentException("Factorial requires non-negative integer");
        
        var result = BigDecimal.One;
        for (int i = 2; i <= (int)nDouble; i++)
        {
            result = result * new BigDecimal(i);
        }
        return result;
    }
    
    private static BigDecimal ReduceAngle(BigDecimal x, int precision)
    {
        var twoPi = new BigDecimal(2) * Pi(precision);
        
        // Reduce to [-2π, 2π]
        while (x > twoPi)
            x = x - twoPi;
        while (x < -twoPi)
            x = x + twoPi;
        
        return x;
    }
    
    private static BigDecimal TruncateToPrecision(BigDecimal value, int precision)
    {
        // This is simplified - actual implementation would properly truncate
        return value;
    }
}