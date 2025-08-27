using System;
using System.Numerics;
using System.Globalization;
namespace MathFlow.Core.Precision;
/// <summary>
/// Arbitrary precision decimal implementation
/// </summary>
public struct BigDecimal : IComparable<BigDecimal>, IEquatable<BigDecimal>
{
    private readonly BigInteger mantissa;
    private readonly int scale; // number of decimal places
    
    public static readonly BigDecimal Zero = new(0);
    public static readonly BigDecimal One = new(1);
    
    public BigDecimal(BigInteger mantissa, int scale)
    {
        this.mantissa = mantissa;
        this.scale = scale;
    }
    
    public BigDecimal(double value, int precision = 50)
    {
        if (double.IsNaN(value) || double.IsInfinity(value))
            throw new ArgumentException("Cannot convert NaN or Infinity to BigDecimal");
        
        // Use higher precision format to capture small differences
        var str = value.ToString($"G{precision}", CultureInfo.InvariantCulture);
        
        // Check for very small or very large numbers that need special handling
        if (Math.Abs(value) < 1e-10 && value != 0)
        {
            // For very small numbers, use decimal representation to avoid precision loss
            str = value.ToString($"F{precision}", CultureInfo.InvariantCulture).TrimEnd('0');
            if (str.EndsWith(".")) str = str.Substring(0, str.Length - 1);
        }
        
        var parts = str.Split('E', 'e');
        
        if (parts.Length == 2)
        {
            // Scientific notation
            var mantissaStr = parts[0];
            var exponent = int.Parse(parts[1]);
            var decimalPos = mantissaStr.IndexOf('.');
            
            if (decimalPos == -1)
            {
                mantissa = BigInteger.Parse(mantissaStr);
                scale = -exponent;
            }
            else
            {
                var wholePart = mantissaStr.Substring(0, decimalPos);
                var fracPart = mantissaStr.Substring(decimalPos + 1);
                mantissa = BigInteger.Parse(wholePart + fracPart);
                scale = fracPart.Length - exponent;
            }
        }
        else
        {
            // Regular notation
            var decimalPos = str.IndexOf('.');
            if (decimalPos == -1)
            {
                mantissa = BigInteger.Parse(str);
                scale = 0;
            }
            else
            {
                var wholePart = str.Substring(0, decimalPos);
                var fracPart = str.Substring(decimalPos + 1);
                
                // Handle sign
                var isNegative = wholePart.StartsWith("-");
                if (isNegative)
                {
                    wholePart = wholePart.Substring(1);
                    mantissa = -BigInteger.Parse(wholePart + fracPart);
                }
                else
                {
                    mantissa = BigInteger.Parse(wholePart + fracPart);
                }
                scale = fracPart.Length;
            }
        }
    }
    
    public BigDecimal(int value) : this((BigInteger)value, 0) { }
    public BigDecimal(long value) : this((BigInteger)value, 0) { }
    
    public static BigDecimal Parse(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            throw new ArgumentException("Invalid number format");
        
        s = s.Trim();
        var eIndex = s.IndexOfAny(new[] { 'e', 'E' });
        
        if (eIndex != -1)
        {
            // Scientific notation: 1.23E+10
            var mantissaStr = s.Substring(0, eIndex);
            var exponentStr = s.Substring(eIndex + 1);
            var exponent = int.Parse(exponentStr);
            
            var decimalPoint = mantissaStr.IndexOf('.');
            if (decimalPoint == -1)
            {
                var mantissa = BigInteger.Parse(mantissaStr);
                return new BigDecimal(mantissa * BigInteger.Pow(10, Math.Max(0, exponent)), -Math.Min(0, exponent));
            }
            else
            {
                var wholePart = mantissaStr.Substring(0, decimalPoint);
                var fracPart = mantissaStr.Substring(decimalPoint + 1);
                var mantissa = BigInteger.Parse(wholePart + fracPart);
                var scale = fracPart.Length - exponent;
                return new BigDecimal(mantissa, scale);
            }
        }
        else
        {
            // Regular number: 123.456 or 0.00001
            var decimalPoint = s.IndexOf('.');
            if (decimalPoint == -1)
            {
                return new BigDecimal(BigInteger.Parse(s), 0);
            }
            
            var wholePart = s.Substring(0, decimalPoint);
            var fracPart = s.Substring(decimalPoint + 1);
            
            // Handle negative numbers
            var isNegative = wholePart.StartsWith("-");
            if (isNegative)
            {
                wholePart = wholePart.Substring(1);
            }
            
            // Combine whole and fractional parts
            if (string.IsNullOrEmpty(wholePart)) wholePart = "0";
            var combined = wholePart + fracPart;
            
            // Remove leading zeros but keep at least one digit
            while (combined.Length > 1 && combined[0] == '0')
            {
                combined = combined.Substring(1);
            }
            
            var mantissa = BigInteger.Parse(combined);
            if (isNegative) mantissa = -mantissa;
            
            return new BigDecimal(mantissa, fracPart.Length);
        }
    }
    
    private static BigDecimal Align(BigDecimal a, BigDecimal b, out BigInteger aMantissa, out BigInteger bMantissa)
    {
        if (a.scale == b.scale)
        {
            aMantissa = a.mantissa;
            bMantissa = b.mantissa;
            return new BigDecimal(BigInteger.Zero, a.scale);
        }
        
        if (a.scale > b.scale)
        {
            aMantissa = a.mantissa;
            bMantissa = b.mantissa * BigInteger.Pow(10, a.scale - b.scale);
            return new BigDecimal(BigInteger.Zero, a.scale);
        }
        else
        {
            aMantissa = a.mantissa * BigInteger.Pow(10, b.scale - a.scale);
            bMantissa = b.mantissa;
            return new BigDecimal(BigInteger.Zero, b.scale);
        }
    }
    
    public static BigDecimal operator +(BigDecimal a, BigDecimal b)
    {
        var result = Align(a, b, out var aMantissa, out var bMantissa);
        return new BigDecimal(aMantissa + bMantissa, result.scale);
    }
    
    public static BigDecimal operator -(BigDecimal a, BigDecimal b)
    {
        var result = Align(a, b, out var aMantissa, out var bMantissa);
        return new BigDecimal(aMantissa - bMantissa, result.scale);
    }
    
    public static BigDecimal operator *(BigDecimal a, BigDecimal b)
    {
        return new BigDecimal(a.mantissa * b.mantissa, a.scale + b.scale);
    }
    
    public static BigDecimal operator /(BigDecimal a, BigDecimal b)
    {
        if (b.mantissa == 0)
            throw new DivideByZeroException();
        
        // Increase precision for division
        var extraPrecision = 50; // extra decimal places
        var scaledDividend = a.mantissa * BigInteger.Pow(10, extraPrecision + b.scale);
        var quotient = scaledDividend / b.mantissa;
        return new BigDecimal(quotient, a.scale + extraPrecision);
    }
    
    public static BigDecimal Pow(BigDecimal baseNum, int exponent)
    {
        if (exponent == 0) return One;
        if (exponent < 0) return One / Pow(baseNum, -exponent);
        
        var result = One;
        var current = baseNum;
        
        while (exponent > 0)
        {
            if ((exponent & 1) == 1)
                result *= current;
            current *= current;
            exponent >>= 1;
        }
        
        return result;
    }
    
    public static BigDecimal operator -(BigDecimal a)
    {
        return new BigDecimal(-a.mantissa, a.scale);
    }
    
    public static bool operator ==(BigDecimal a, BigDecimal b)
    {
        Align(a, b, out var aMantissa, out var bMantissa);
        return aMantissa == bMantissa;
    }
    
    public static bool operator !=(BigDecimal a, BigDecimal b)
    {
        return !(a == b);
    }
    
    public static bool operator <(BigDecimal a, BigDecimal b)
    {
        Align(a, b, out var aMantissa, out var bMantissa);
        return aMantissa < bMantissa;
    }
    
    public static bool operator >(BigDecimal a, BigDecimal b)
    {
        Align(a, b, out var aMantissa, out var bMantissa);
        return aMantissa > bMantissa;
    }
    
    public static bool operator <=(BigDecimal a, BigDecimal b)
    {
        return !(a > b);
    }
    
    public static bool operator >=(BigDecimal a, BigDecimal b)
    {
        return !(a < b);
    }
    
    public double ToDouble()
    {
        if (scale == 0)
            return (double)mantissa;
        
        return (double)mantissa / Math.Pow(10, scale);
    }
    
    public override string ToString()
    {
        if (scale == 0)
            return mantissa.ToString();
        
        var mantissaStr = mantissa.ToString();
        var isNegative = mantissaStr.StartsWith("-");
        if (isNegative) mantissaStr = mantissaStr.Substring(1);
        
        if (mantissaStr.Length <= scale)
        {
            mantissaStr = new string('0', scale - mantissaStr.Length + 1) + mantissaStr;
        }
        
        var insertPos = mantissaStr.Length - scale;
        var result = mantissaStr.Insert(insertPos, ".");
        
        // trim trailing zeros
        result = result.TrimEnd('0').TrimEnd('.');
        
        return isNegative ? "-" + result : result;
    }
    
    public bool Equals(BigDecimal other)
    {
        return this == other;
    }
    
    public override bool Equals(object? obj)
    {
        return obj is BigDecimal other && Equals(other);
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(mantissa, scale);
    }
    
    public int CompareTo(BigDecimal other)
    {
        if (this < other) return -1;
        if (this > other) return 1;
        return 0;
    }
    
    public static implicit operator BigDecimal(int value)
    {
        return new BigDecimal(value);
    }
    
    public static implicit operator BigDecimal(long value)
    {
        return new BigDecimal(value);
    }
    
    public static explicit operator BigDecimal(double value)
    {
        return new BigDecimal(value);
    }
    
    public static explicit operator double(BigDecimal value)
    {
        return value.ToDouble();
    }
}