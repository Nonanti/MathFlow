using Xunit;
using MathFlow.Core.Precision;
using System;

namespace MathFlow.Tests;

public class BigDecimalTests
{
    [Fact]
    public void TestBasicArithmetic()
    {
        var a = new BigDecimal(10);
        var b = new BigDecimal(3);
        
        Assert.Equal(new BigDecimal(13), a + b);
        Assert.Equal(new BigDecimal(7), a - b);
        Assert.Equal(new BigDecimal(30), a * b);
    }
    
    [Fact]
    public void TestPrecisionWithLargeNumbers()
    {
        var hundred = new BigDecimal(100);
        var bigNum = BigDecimal.Pow(hundred, 100);
        var result = bigNum + BigDecimal.One - bigNum;
        
        Assert.Equal(BigDecimal.One, result);
    }
    
    [Fact]
    public void TestDivisionPrecision()
    {
        var one = BigDecimal.One;
        var three = new BigDecimal(3);
        var result = one / three;
        
        var str = result.ToString();
        Assert.StartsWith("0.3333333", str);
    }
    
    [Fact]
    public void TestParsingScientificNotation()
    {
        var parsed = BigDecimal.Parse("1.23E+5");
        var expected = new BigDecimal(123000);
        Assert.Equal(expected, parsed);
        
        var parsed2 = BigDecimal.Parse("5E-3");
        var str = parsed2.ToString();
        Assert.Equal("0.005", str);
    }
    
    [Fact]
    public void TestNegativeNumbers()
    {
        var neg = new BigDecimal(-5);
        var pos = new BigDecimal(3);
        
        Assert.Equal(new BigDecimal(-2), neg + pos);
        Assert.Equal(new BigDecimal(-8), neg - pos);
        Assert.Equal(new BigDecimal(-15), neg * pos);
    }
    
    [Fact]
    public void TestVerySmallNumbers()
    {
        var small = BigDecimal.Parse("0.000000000000000001");
        var one = BigDecimal.One;
        var sum = one + small;
        
        Assert.True(sum > one);
        Assert.NotEqual(one, sum);
    }
    
    [Fact]
    public void TestComparisons()
    {
        var a = new BigDecimal(10);
        var b = new BigDecimal(5);
        var c = new BigDecimal(10);
        
        Assert.True(a > b);
        Assert.True(b < a);
        Assert.True(a >= c);
        Assert.True(a <= c);
        Assert.Equal(a, c);
        Assert.NotEqual(a, b);
    }
    
    [Fact]
    public void TestZeroAndOne()
    {
        Assert.Equal(BigDecimal.Zero, new BigDecimal(0));
        Assert.Equal(BigDecimal.One, new BigDecimal(1));
        
        var zero = BigDecimal.Zero;
        var one = BigDecimal.One;
        
        Assert.Equal(one, zero + one);
        Assert.Equal(zero, one - one);
        Assert.Equal(zero, zero * one);
    }
    
    [Fact]
    public void TestPowerFunction()
    {
        var two = new BigDecimal(2);
        var result = BigDecimal.Pow(two, 10);
        Assert.Equal(new BigDecimal(1024), result);
        
        var ten = new BigDecimal(10);
        var hundred = BigDecimal.Pow(ten, 2);
        Assert.Equal(new BigDecimal(100), hundred);
    }
    
    [Fact]
    public void TestStringRepresentation()
    {
        var a = new BigDecimal(new System.Numerics.BigInteger(123456), 3);
        Assert.Equal("123.456", a.ToString());
        
        var b = new BigDecimal(new System.Numerics.BigInteger(100), 2);
        Assert.Equal("1", b.ToString());
        
        var c = new BigDecimal(new System.Numerics.BigInteger(5), 4);
        Assert.Equal("0.0005", c.ToString());
    }
}