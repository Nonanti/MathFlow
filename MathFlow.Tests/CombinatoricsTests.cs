using MathFlow.Core;

namespace MathFlow.Tests;
public class CombinatoricsTests
{
	private readonly MathEngine _engine;

	public CombinatoricsTests()
	{
		_engine = new MathEngine();
	}

	[Theory]
	[InlineData("gcd(12, 18)", 6)]
	[InlineData("lcm(12, 18)", 36)]
	[InlineData("factorial(5)", 120)]
	[InlineData("perm(5, 3)", 60)]
	[InlineData("Permutation(5, 3)", 60)]
	[InlineData("perm(4, 2)", 12)]
	[InlineData("perm(6, 0)", 1)]
	[InlineData("perm(0, 0)", 1)]
	[InlineData("binomial(5, 3)", 10)]
	[InlineData("binomial(6, 0)", 1)]
	[InlineData("ncr(6, 6)", 1)]
	[InlineData("choose(6, 2)", 15)]
	public void Calculate_CombinatoricsFunctions_ReturnsCorrectResult(string expression, double expected)
	{
		var result = _engine.Calculate(expression);
		Assert.Equal(expected, result, 10);
	}
}