using MathFlow.Core;
using MathFlow.Core.Calculus;
using Xunit;

namespace MathFlow.Tests;

public class SymbolicIntegrationTests
{
    private readonly MathEngine engine = new();
    
    [Theory]
    [InlineData("5", "x", "5*x")]  // constant
    [InlineData("x", "x", "x^2 / 2")]  // x
    [InlineData("x^2", "x", "x^3 / 3")]  // power rule
    [InlineData("x^3", "x", "x^4 / 4")]
    [InlineData("x^(-2)", "x", "-(1 / x)")]  // negative power
    [InlineData("1/x", "x", "ln(x)")]  // special case x^(-1)
    public void IntegrateSymbolic_BasicPowerRule_ReturnsCorrectAntiderivative(string expr, string var, string expected)
    {
        var result = engine.IntegrateSymbolic(expr, var);
        var simplified = result.Simplify();
        
        // Test by differentiating back
        var derivative = engine.Differentiate(result.ToString(), var);
        var original = engine.Parse(expr);
        
        var testVals = new[] { 0.5, 1.0, 2.0, 3.0 };
        foreach (var val in testVals)
        {
            if (val == 0 && expr.Contains("/x")) continue; // avoid division by zero
            
            var vars = new Dictionary<string, double> { [var] = val };
            var origValue = original.Evaluate(vars);
            var derivValue = derivative.Evaluate(vars);
            
            Assert.True(Math.Abs(origValue - derivValue) < 1e-6, 
                $"Failed for {expr} at x={val}: original={origValue}, derivative={derivValue}");
        }
    }
    
    [Theory]
    [InlineData("sin(x)", "x", "-cos(x)")]
    [InlineData("cos(x)", "x", "sin(x)")]
    [InlineData("exp(x)", "x", "exp(x)")]
    [InlineData("e^x", "x", "e^x")]
    [InlineData("sinh(x)", "x", "cosh(x)")]
    [InlineData("cosh(x)", "x", "sinh(x)")]
    public void IntegrateSymbolic_TrigAndExp_ReturnsCorrectResult(string expr, string var, string expected)
    {
        var result = engine.IntegrateSymbolic(expr, var);
        
        // Verify by differentiation
        var deriv = engine.Differentiate(result.ToString(), var);
        var orig = engine.Parse(expr);
        
        var testVals = new[] { 0, Math.PI/6, Math.PI/4, Math.PI/3, Math.PI/2 };
        foreach (var val in testVals)
        {
            var vars = new Dictionary<string, double> { [var] = val };
            var origVal = orig.Evaluate(vars);
            var derivVal = deriv.Evaluate(vars);
            
            Assert.True(Math.Abs(origVal - derivVal) < 1e-10);
        }
    }
    
    [Theory]
    [InlineData("2*x + 3", "x", "x^2 + 3*x")]
    [InlineData("x^2 + x", "x", "x^3/3 + x^2/2")]
    [InlineData("sin(x) + cos(x)", "x", "-cos(x) + sin(x)")]
    public void IntegrateSymbolic_LinearCombinations_ReturnsCorrectResult(string expr, string var, string expected)
    {
        var result = engine.IntegrateSymbolic(expr, var);
        
        // Test by differentiation
        var deriv = engine.Differentiate(result.ToString(), var);
        var orig = engine.Parse(expr).Simplify();
        
        for (double val = 0.1; val <= 2; val += 0.3)
        {
            var vars = new Dictionary<string, double> { [var] = val };
            var origVal = orig.Evaluate(vars);
            var derivVal = deriv.Evaluate(vars);
            
            Assert.True(Math.Abs(origVal - derivVal) < 1e-9, 
                $"Failed at x={val}: expected={origVal}, got={derivVal}");
        }
    }
    
    [Theory]
    [InlineData("3*x^2", "x", "x^3")]  // constant multiple
    [InlineData("5*sin(x)", "x", "-5*cos(x)")]
    [InlineData("2*exp(x)", "x", "2*exp(x)")]
    public void IntegrateSymbolic_ConstantMultiple_ReturnsCorrectResult(string expr, string var, string expected)
    {
        var result = engine.IntegrateSymbolic(expr, var);
        
        var deriv = engine.Differentiate(result.ToString(), var).Simplify();
        var orig = engine.Parse(expr).Simplify();
        
        for (double val = 0.5; val <= 2; val += 0.5)
        {
            var vars = new Dictionary<string, double> { [var] = val };
            var origVal = orig.Evaluate(vars);
            var derivVal = deriv.Evaluate(vars);
            
            Assert.True(Math.Abs(origVal - derivVal) < 1e-9);
        }
    }
    
    [Fact]
    public void IntegrateSymbolic_ChainRule_SimpleLinearSubstitution()
    {
        // ∫sin(2x) dx = -cos(2x)/2
        var result = engine.IntegrateSymbolic("sin(2*x)", "x");
        
        // Can't test exact form, but test by differentiation
        var deriv = engine.Differentiate(result.ToString(), "x");
        var orig = engine.Parse("sin(2*x)");
        
        for (double x = 0; x < 2 * Math.PI; x += Math.PI / 6)
        {
            var vars = new Dictionary<string, double> { ["x"] = x };
            var origVal = orig.Evaluate(vars);
            var derivVal = deriv.Evaluate(vars);
            
            Assert.True(Math.Abs(origVal - derivVal) < 1e-9);
        }
    }
    
    [Fact]
    public void IntegrateSymbolic_IntegrationByParts_SimpleCase()
    {
        // ∫x*e^x dx = (x-1)*e^x
        var result = engine.IntegrateSymbolic("x*exp(x)", "x");
        
        var deriv = engine.Differentiate(result.ToString(), "x");
        var orig = engine.Parse("x*exp(x)");
        
        for (double x = -1; x <= 2; x += 0.5)
        {
            var vars = new Dictionary<string, double> { ["x"] = x };
            var origVal = orig.Evaluate(vars);
            var derivVal = deriv.Evaluate(vars);
            
            Assert.True(Math.Abs(origVal - derivVal) < 1e-9,
                $"Failed at x={x}: expected={origVal}, got={derivVal}");
        }
    }
    
    [Fact]
    public void IntegrateSymbolic_DefiniteIntegral_UsingFundamentalTheorem()
    {
        // Test fundamental theorem: ∫[a,b] f(x)dx = F(b) - F(a)
        var expr = "x^2";
        var antideriv = engine.IntegrateSymbolic(expr, "x");
        
        double a = 0, b = 2;
        var vars_a = new Dictionary<string, double> { ["x"] = a };
        var vars_b = new Dictionary<string, double> { ["x"] = b };
        
        var F_a = antideriv.Evaluate(vars_a);
        var F_b = antideriv.Evaluate(vars_b);
        var definiteIntegral = F_b - F_a;
        
        // Compare with numerical integration
        var numericalResult = engine.Integrate(expr, "x", a, b);
        
        Assert.True(Math.Abs(definiteIntegral - numericalResult) < 1e-6);
    }
    
    [Theory]
    [InlineData("tan(x)", "x")]
    [InlineData("ln(x)", "x")]
    public void IntegrateSymbolic_SpecialFunctions_ReturnsResult(string expr, string var)
    {
        var result = engine.IntegrateSymbolic(expr, var);
        Assert.NotNull(result);
        
        // For complex integrals, just check it doesn't throw
        // and returns something that differentiates back correctly (where possible)
        if (!SymbolicIntegration.IsUnsupported(result))
        {
            var deriv = engine.Differentiate(result.ToString(), var);
            var orig = engine.Parse(expr);
            
            // Test at safe points
            var testPoint = expr == "ln(x)" ? 2.0 : Math.PI / 6;
            var vars = new Dictionary<string, double> { [var] = testPoint };
            
            var origVal = orig.Evaluate(vars);
            var derivVal = deriv.Evaluate(vars);
            
            Assert.True(Math.Abs(origVal - derivVal) < 1e-6);
        }
    }
    
    [Fact]
    public void IntegrateSymbolic_WithRespectToDifferentVariable()
    {
        // ∫(x^2 + y) dy = x^2*y + y^2/2
        var result = engine.IntegrateSymbolic("x^2 + y", "y");
        
        // Test at various points
        for (double x = 1; x <= 3; x++)
        {
            for (double y = 1; y <= 3; y++)
            {
                var vars = new Dictionary<string, double> { ["x"] = x, ["y"] = y };
                
                // Differentiate back
                var deriv = engine.Differentiate(result.ToString(), "y");
                var orig = engine.Parse("x^2 + y");
                
                var origVal = orig.Evaluate(vars);
                var derivVal = deriv.Evaluate(vars);
                
                Assert.True(Math.Abs(origVal - derivVal) < 1e-9);
            }
        }
    }
    
    [Fact]
    public void Antiderivative_AliasWorks()
    {
        var result1 = engine.IntegrateSymbolic("x^2", "x");
        var result2 = engine.Antiderivative("x^2", "x");
        
        Assert.Equal(result1.ToString(), result2.ToString());
    }
}