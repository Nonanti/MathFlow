using MathFlow.Core;

namespace MathFlow.Tests;

public class MathEngineTests
{
	private readonly MathEngine _engine;

	public MathEngineTests()
	{
		_engine = new MathEngine();
	}

	[Theory]
	[InlineData("2 + 3", 5)]
	[InlineData("10 - 4", 6)]
	[InlineData("5 * 6", 30)]
	[InlineData("15 / 3", 5)]
	[InlineData("2^3", 8)]
	[InlineData("10 % 3", 1)]
	public void Calculate_BasicArithmetic_ReturnsCorrectResult(string expression, double expected)
	{
		var result = _engine.Calculate(expression);
		Assert.Equal(expected, result, 10);
	}

	[Theory]
	[InlineData("sin(0)", 0)]
	[InlineData("cos(0)", 1)]
	[InlineData("tan(0)", 0)]
	[InlineData("sqrt(16)", 4)]
	[InlineData("abs(-5)", 5)]
	[InlineData("exp(0)", 1)]
	[InlineData("ln(e)", 1)]
	[InlineData("log10(100)", 2)]
	public void Calculate_MathFunctions_ReturnsCorrectResult(string expression, double expected)
	{
		var result = _engine.Calculate(expression);
		Assert.Equal(expected, result, 10);
	}

	[Theory]
	[InlineData("pi", Math.PI)]
	[InlineData("e", Math.E)]
	[InlineData("tau", 2 * Math.PI)]
	public void Calculate_Constants_ReturnsCorrectValue(string expression, double expected)
	{
		var result = _engine.Calculate(expression);
		Assert.Equal(expected, result, 10);
	}

	[Theory]
	[InlineData("2 * (3 + 4)", 14)]
	[InlineData("(5 + 3) / 2", 4)]
	[InlineData("2^(1 + 2)", 8)]
	[InlineData("sin(pi/2)", 1)]
	public void Calculate_ComplexExpressions_ReturnsCorrectResult(string expression, double expected)
	{
		var result = _engine.Calculate(expression);
		Assert.Equal(expected, result, 10);
	}

	[Fact]
	public void Calculate_WithVariables_ReturnsCorrectResult()
	{
		var variables = new Dictionary<string, double>
		{
			["x"] = 5,
			["y"] = 3
		};

		var result = _engine.Calculate("x^2 + 2*x*y + y^2", variables);
		Assert.Equal(64, result, 10);
	}

	[Theory]
	[InlineData("2*x + 3*x", "5*x")]
	[InlineData("x^2 * x^3", "x^5")]
	[InlineData("0 + x", "x")]
	[InlineData("x * 1", "x")]
	[InlineData("x - x", "0")]
	public void Simplify_Expressions_ReturnsSimplifiedForm(string expression, string expected)
	{
		var simplified = _engine.Simplify(expression);
		Assert.Equal(expected.Replace(" ", ""), simplified.ToString().Replace(" ", ""));
	}

	[Theory]
	[InlineData("x^2", "x", "2 * x")]
	[InlineData("sin(x)", "x", "cos(x) * 1")]
	[InlineData("e^x", "x", "e^x * 1")]
	[InlineData("ln(x)", "x", "1 / x")]
	[InlineData("x^3 + x^2", "x", "(3 * x^2) + (2 * x)")]
	public void Differentiate_Expressions_ReturnsDerivative(string expression, string variable, string expected)
	{
		var derivative = _engine.Differentiate(expression, variable);
		var simplifiedDerivative = derivative.Simplify().ToString().Replace(" ", "");
		var expectedSimplified = expected.Replace(" ", "");

		// Compare by evaluating at a test point
		var testPoint = new Dictionary<string, double> { [variable] = 2.0 };
		var expectedExpr = _engine.Parse(expected);

		var actualValue = derivative.Evaluate(testPoint);
		var expectedValue = expectedExpr.Evaluate(testPoint);

		Assert.Equal(expectedValue, actualValue, 10);
	}

	[Fact]
	public void Integrate_SimpleFunction_ReturnsCorrectArea()
	{
		// Integrate x^2 from 0 to 1, should be 1/3
		var result = _engine.Integrate("x^2", "x", 0, 1);
		Assert.Equal(1.0 / 3.0, result, 4);
	}

	[Fact]
	public void FindRoot_LinearEquation_ReturnsCorrectRoot()
	{
		// Solve 2x - 4 = 0, root should be 2
		var root = _engine.FindRoot("2*x - 4", 0);
		Assert.Equal(2, root, 10);
	}

	[Fact]
	public void FindRoots_QuadraticEquation_ReturnsAllRoots()
	{
		// Solve x^2 - 5x + 6 = 0, roots should be 2 and 3
		var roots = _engine.FindRoots("x^2 - 5*x + 6", 0, 5);
		Assert.Contains(roots, r => Math.Abs(r - 2) < 0.01);
		Assert.Contains(roots, r => Math.Abs(r - 3) < 0.01);
	}

	[Theory]
	[InlineData("(x + 2)^2", "x^2 + (4 * x) + 4")]
	[InlineData("(x - 3)^2", "x^2 - (6 * x) + 9")]
	public void Expand_Expressions_ReturnsExpandedForm(string expression, string expected)
	{
		var expanded = _engine.Expand(expression);

		// Compare by evaluating at test points
		var testValues = new[] { 0, 1, 2, -1, 3.5 };
		var expectedExpr = _engine.Parse(expected);

		foreach (var value in testValues)
		{
			var variables = new Dictionary<string, double> { ["x"] = value };
			var actualValue = expanded.Evaluate(variables);
			var expectedValue = expectedExpr.Evaluate(variables);
			Assert.Equal(expectedValue, actualValue, 10);
		}
	}

	[Fact]
	public void Substitute_Variable_ReturnsSubstitutedExpression()
	{
		var result = _engine.Substitute("x^2 + y", "x", "2*z");
		var variables = new Dictionary<string, double> { ["z"] = 3, ["y"] = 5 };

		Assert.Equal(41, result.Evaluate(variables), 10); // (2*3)^2 + 5 = 36 + 5 = 41
	}

	[Theory]
	[InlineData("sin(x)^2", "\\sin^{2}\\left(x\\right)")]
	[InlineData("sqrt(x + 1)", "\\sqrt{x + 1}")]
	[InlineData("x/y", "\\frac{x}{y}")]
	public void ToLatex_Expressions_ReturnsLatexFormat(string expression, string expectedContains)
	{
		var latex = _engine.ToLatex(expression);
		Assert.Contains(expectedContains.Replace("\\", @"\"), latex);
	}

	[Fact]
	public void IsValid_ValidExpression_ReturnsTrue()
	{
		Assert.True(_engine.IsValid("x^2 + 2*x + 1"));
	}

	[Fact]
	public void IsValid_InvalidExpression_ReturnsFalse()
	{
		Assert.False(_engine.IsValid("x + + 2"));
	}

	[Fact]
	public void GetVariables_Expression_ReturnsAllVariables()
	{
		var variables = _engine.GetVariables("x^2 + y*z - w");

		Assert.Contains("x", variables);
		Assert.Contains("y", variables);
		Assert.Contains("z", variables);
		Assert.Contains("w", variables);
		Assert.Equal(4, variables.Count);
	}

	[Fact]
	public void SetVariable_GlobalVariable_UsedInCalculation()
	{
		_engine.SetVariable("a", 5);
		var result = _engine.Calculate("a * 2");
		Assert.Equal(10, result);
	}

	[Theory]
	[InlineData("min(3, 5)", 3)]
	[InlineData("max(3, 5)", 5)]
	[InlineData("min(-2, -5)", -5)]
	[InlineData("max(-2, -5)", -2)]
	public void Calculate_MinMaxFunctions_ReturnsCorrectResult(string expression, double expected)
	{
		var result = _engine.Calculate(expression);
		Assert.Equal(expected, result, 10);
	}

	[Theory]
	[InlineData("floor(3.5)", 3)]
	[InlineData("ceil(3.5)", 4)]
	[InlineData("ceiling(3.5)", 4)]
	public void Calculate_FloorCielingFunctions_ReturnsCorrectResult(string expression, double expected)
	{
		var result = _engine.Calculate(expression);
		Assert.Equal(expected, result, 10);
	}

	[Fact]
	public void Calculate_ChainedOperations_ReturnsCorrectResult()
	{
		var result = _engine.Calculate("sin(cos(tan(pi/4)))");
		var expected = Math.Sin(Math.Cos(Math.Tan(Math.PI / 4)));
		Assert.Equal(expected, result, 10);
	}
}