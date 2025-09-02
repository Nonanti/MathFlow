namespace MathFlow.Core.Combinatorics;
public static class CombinatoricsFunctions
{
	/// <summary>
	/// Calculates the GCD.
	/// </summary>
	public static long GCD(long a, long b)
	{
		a = Math.Abs(a);
		b = Math.Abs(b);

		while (b != 0)
		{
			var temp = b;
			b = a % b;
			a = temp;
		}

		return a;
	}

	/// <summary>
	/// Calculates the LCM.
	/// </summary>
	public static long LCM(long a, long b)
	{
		return Math.Abs(a * b) / GCD(a, b);
	}

	/// <summary>
	/// Calculates the factorial.
	/// </summary>
	public static double Factorial(double n)
	{
		if (n < 0 || n != Math.Floor(n))
			throw new ArgumentException("Factorial is only defined for non-negative integers");

		if (n > 170)
			return double.PositiveInfinity;

		double result = 1;
		for (int i = 2; i <= (int)n; i++)
			result *= i;

		return result;
	}

	/// <summary>
	/// Calculates the binomial coefficient.
	/// </summary>
	public static double Binomial(double n, double k)
	{
		if (n < 0 || n != Math.Floor(n) || k < 0 || k != Math.Floor(k))
			throw new ArgumentException("Binomial is only defined for non-negative integers");

		if (n < 0 || k < 0 || k > n)
			return 0;

		if (k > n - k)
			k = n - k;

		double result = 1;
		for (int i = 1; i <= k; i++)
		{
			result *= n - (k - i);
			result /= i;
		}

		return result;
	}

	/// <summary>
	/// Calculates the number of permutations.
	/// </summary>
	public static double Permutation(double n, double k)
	{
		if (n < 0 || n != Math.Floor(n) || k < 0 || k != Math.Floor(k) || k > n)
			throw new ArgumentException("Permutation is only defined for non-negative integers with k <= n");

		double result = 1;
		for (int i = 0; i < k; i++)
			result *= n - i;

		return result;
	}
}