namespace MathFlow.Core.ComplexMath;
public struct ComplexNumber  // a + bi format
{
    public double Real { get; }
    public double Imaginary { get; }
    
    public ComplexNumber(double real, double imaginary = 0)
    {
        Real = real;
        Imaginary = imaginary;
    }
    
    /// <summary>
    /// Magnitude (absolute value) of the complex number
    /// </summary>
    public double Magnitude => Math.Sqrt(Real * Real + Imaginary * Imaginary);
    
    /// <summary>
    /// Phase angle in radians
    /// </summary>
    public double Phase => Math.Atan2(Imaginary, Real);
    
    /// <summary>
    /// Complex conjugate
    /// </summary>
    public ComplexNumber Conjugate => new ComplexNumber(Real, -Imaginary);
    
    /// <summary>
    /// Creates a complex number from polar coordinates
    /// </summary>
    public static ComplexNumber FromPolar(double magnitude, double phase)
    {
        return new ComplexNumber(
            magnitude * Math.Cos(phase),
            magnitude * Math.Sin(phase)
        );
    }
    
    // Arithmetic operators
    public static ComplexNumber operator +(ComplexNumber a, ComplexNumber b)
    {
        return new ComplexNumber(a.Real + b.Real, a.Imaginary + b.Imaginary);
    }
    
    public static ComplexNumber operator -(ComplexNumber a, ComplexNumber b)
    {
        return new ComplexNumber(a.Real - b.Real, a.Imaginary - b.Imaginary);
    }
    
    public static ComplexNumber operator *(ComplexNumber a, ComplexNumber b)
    {
        return new ComplexNumber(
            a.Real * b.Real - a.Imaginary * b.Imaginary,
            a.Real * b.Imaginary + a.Imaginary * b.Real
        );
    }
    
    public static ComplexNumber operator /(ComplexNumber a, ComplexNumber b)
    {
        var denominator = b.Real * b.Real + b.Imaginary * b.Imaginary;
        if (denominator < 1e-10)
            throw new DivideByZeroException();
        
        return new ComplexNumber(
            (a.Real * b.Real + a.Imaginary * b.Imaginary) / denominator,
            (a.Imaginary * b.Real - a.Real * b.Imaginary) / denominator
        );
    }
    
    public static ComplexNumber operator -(ComplexNumber a)
    {
        return new ComplexNumber(-a.Real, -a.Imaginary);
    }
    
    // Implicit conversion from real number
    public static implicit operator ComplexNumber(double real)
    {
        return new ComplexNumber(real);
    }
    
    /// <summary>
    /// Exponential of complex number
    /// </summary>
    public ComplexNumber Exp()
    {
        var expReal = Math.Exp(Real);
        return new ComplexNumber(
            expReal * Math.Cos(Imaginary),
            expReal * Math.Sin(Imaginary)
        );
    }
    
    /// <summary>
    /// Natural logarithm of complex number
    /// </summary>
    public ComplexNumber Log()
    {
        return new ComplexNumber(Math.Log(Magnitude), Phase);
    }
    
    /// <summary>
    /// Complex number raised to a power
    /// </summary>
    public ComplexNumber Pow(ComplexNumber exponent)
    {
        if (Magnitude < 1e-10)
            return new ComplexNumber(0);
        
        var logThis = Log();
        var result = exponent * logThis;
        return result.Exp();
    }
    
    /// <summary>
    /// Square root of complex number
    /// </summary>
    public ComplexNumber Sqrt()
    {
        var magnitude = Math.Sqrt(Magnitude);
        var phase = Phase / 2;
        return FromPolar(magnitude, phase);
    }
    
    // Trigonometric functions
    public ComplexNumber Sin()
    {
        return new ComplexNumber(
            Math.Sin(Real) * Math.Cosh(Imaginary),
            Math.Cos(Real) * Math.Sinh(Imaginary)
        );
    }
    
    public ComplexNumber Cos()
    {
        return new ComplexNumber(
            Math.Cos(Real) * Math.Cosh(Imaginary),
            -Math.Sin(Real) * Math.Sinh(Imaginary)
        );
    }
    
    public ComplexNumber Tan()
    {
        return Sin() / Cos();
    }
    
    public override string ToString()
    {
        if (Math.Abs(Imaginary) < 1e-10)
            return Real.ToString("G4");
        
        if (Math.Abs(Real) < 1e-10)
            return $"{Imaginary:G4}i";
        
        return Imaginary >= 0 
            ? $"{Real:G4} + {Imaginary:G4}i" 
            : $"{Real:G4} - {-Imaginary:G4}i";
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is not ComplexNumber other)
            return false;
        
        return Math.Abs(Real - other.Real) < 1e-10 && 
               Math.Abs(Imaginary - other.Imaginary) < 1e-10;
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(Real, Imaginary);
    }
    
    // Common complex constants
    public static readonly ComplexNumber Zero = new(0);
    public static readonly ComplexNumber One = new(1);
    public static readonly ComplexNumber I = new(0, 1);
    public static readonly ComplexNumber MinusI = new(0, -1);
}