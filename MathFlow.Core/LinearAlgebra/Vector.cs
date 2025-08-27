namespace MathFlow.Core.LinearAlgebra;
/// <summary>
/// Represents a mathematical vector
/// </summary>
public class Vector
{
    public double[] Components { get; }
    public int Dimension => Components.Length;
    
    public Vector(params double[] components)
    {
        if (components == null || components.Length == 0)
            throw new ArgumentException("Vector must have at least one component");
        
        Components = (double[])components.Clone();
    }
    
    public double this[int index]
    {
        get => Components[index];
        set => Components[index] = value;
    }
    
    // length/magnitude of vector
    public double Magnitude => Math.Sqrt(Components.Sum(c => c * c));  // sqrt(sum of squares)
    
    /// <summary>
    /// Returns a normalized (unit) vector
    /// </summary>
    public Vector Normalized()
    {
        var mag = Magnitude;
        if (mag < 1e-10)
            throw new InvalidOperationException("Cannot normalize zero vector");
        
        return this / mag;
    }
    
    /// <summary>
    /// Dot product of two vectors
    /// </summary>
    public double Dot(Vector other)
    {
        if (Dimension != other.Dimension)
            throw new ArgumentException("Vectors must have same dimension");
        
        return Components.Zip(other.Components, (a, b) => a * b).Sum();
    }
    
    /// <summary>
    /// Cross product (only for 3D vectors)
    /// </summary>
    public Vector Cross(Vector other)
    {
        if (Dimension != 3 || other.Dimension != 3)
            throw new ArgumentException("Cross product is only defined for 3D vectors");
        
        return new Vector(
            this[1] * other[2] - this[2] * other[1],
            this[2] * other[0] - this[0] * other[2],
            this[0] * other[1] - this[1] * other[0]
        );
    }
    
    /// <summary>
    /// Angle between two vectors in radians
    /// </summary>
    public double AngleTo(Vector other)
    {
        var dot = Dot(other);
        var mag = Magnitude * other.Magnitude;
        
        if (mag < 1e-10)
            throw new InvalidOperationException("Cannot compute angle with zero vector");
        
        return Math.Acos(Math.Clamp(dot / mag, -1, 1));
    }
    
    // Operator overloads
    public static Vector operator +(Vector a, Vector b)
    {
        if (a.Dimension != b.Dimension)
            throw new ArgumentException("Vectors must have same dimension");
        
        return new Vector(a.Components.Zip(b.Components, (x, y) => x + y).ToArray());
    }
    
    public static Vector operator -(Vector a, Vector b)
    {
        if (a.Dimension != b.Dimension)
            throw new ArgumentException("Vectors must have same dimension");
        
        return new Vector(a.Components.Zip(b.Components, (x, y) => x - y).ToArray());
    }
    
    public static Vector operator *(Vector v, double scalar)
    {
        return new Vector(v.Components.Select(c => c * scalar).ToArray());
    }
    
    public static Vector operator *(double scalar, Vector v)
    {
        return v * scalar;
    }
    
    public static Vector operator /(Vector v, double scalar)
    {
        if (Math.Abs(scalar) < 1e-10)
            throw new DivideByZeroException();
        
        return new Vector(v.Components.Select(c => c / scalar).ToArray());
    }
    
    public static Vector operator -(Vector v)
    {
        return new Vector(v.Components.Select(c => -c).ToArray());
    }
    
    public override string ToString()
    {
        return $"[{string.Join(", ", Components.Select(c => c.ToString("G4")))}]";
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is not Vector other || Dimension != other.Dimension)
            return false;
        
        return Components.Zip(other.Components, (a, b) => Math.Abs(a - b) < 1e-10).All(x => x);
    }
    
    public override int GetHashCode()
    {
        return Components.Aggregate(Dimension, (hash, component) => hash ^ component.GetHashCode());
    }
}