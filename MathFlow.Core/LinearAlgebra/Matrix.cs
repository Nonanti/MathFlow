using System;
using System.Text;
using System.Linq;
namespace MathFlow.Core.LinearAlgebra;
/// <summary>
/// Represents a mathematical matrix with various operations
/// </summary>
public class Matrix : IEquatable<Matrix>
{
    private readonly double[,] data;
    
    public int Rows { get; }
    public int Columns { get; }
    
    /// <summary>
    /// Creates a new matrix from a 2D array
    /// </summary>
    public Matrix(double[,] data)
    {
        if (data == null)
            throw new ArgumentNullException(nameof(data));
            
        Rows = data.GetLength(0);
        Columns = data.GetLength(1);
        
        if (Rows == 0 || Columns == 0)
            throw new ArgumentException("Matrix cannot be empty");
            
        this.data = (double[,])data.Clone();
    }
    
    /// <summary>
    /// Creates a new matrix with specified dimensions
    /// </summary>
    public Matrix(int rows, int columns)
    {
        if (rows <= 0 || columns <= 0)
            throw new ArgumentException("Matrix dimensions must be positive");
            
        Rows = rows;
        Columns = columns;
        data = new double[rows, columns];
    }
    
    /// <summary>
    /// Gets or sets matrix element at specified position
    /// </summary>
    public double this[int row, int col]
    {
        get
        {
            if (row < 0 || row >= Rows || col < 0 || col >= Columns)
                throw new IndexOutOfRangeException();
            return data[row, col];
        }
        set
        {
            if (row < 0 || row >= Rows || col < 0 || col >= Columns)
                throw new IndexOutOfRangeException();
            data[row, col] = value;
        }
    }
    
    #region Basic Operations
    
    /// <summary>
    /// Adds two matrices
    /// </summary>
    public static Matrix operator +(Matrix a, Matrix b)
    {
        if (a.Rows != b.Rows || a.Columns != b.Columns)
            throw new InvalidOperationException("Matrices must have same dimensions for addition");
            
        var result = new Matrix(a.Rows, a.Columns);
        for (int i = 0; i < a.Rows; i++)
        {
            for (int j = 0; j < a.Columns; j++)
            {
                result[i, j] = a[i, j] + b[i, j];
            }
        }
        return result;
    }
    
    /// <summary>
    /// Subtracts two matrices
    /// </summary>
    public static Matrix operator -(Matrix a, Matrix b)
    {
        if (a.Rows != b.Rows || a.Columns != b.Columns)
            throw new InvalidOperationException("Matrices must have same dimensions for subtraction");
            
        var result = new Matrix(a.Rows, a.Columns);
        for (int i = 0; i < a.Rows; i++)
        {
            for (int j = 0; j < a.Columns; j++)
            {
                result[i, j] = a[i, j] - b[i, j];
            }
        }
        return result;
    }
    
    /// <summary>
    /// Multiplies two matrices
    /// </summary>
    public static Matrix operator *(Matrix a, Matrix b)
    {
        if (a.Columns != b.Rows)
            throw new InvalidOperationException($"Cannot multiply {a.Rows}x{a.Columns} matrix with {b.Rows}x{b.Columns} matrix");
            
        var result = new Matrix(a.Rows, b.Columns);
        for (int i = 0; i < a.Rows; i++)
        {
            for (int j = 0; j < b.Columns; j++)
            {
                double sum = 0;
                for (int k = 0; k < a.Columns; k++)
                {
                    sum += a[i, k] * b[k, j];
                }
                result[i, j] = sum;
            }
        }
        return result;
    }
    
    /// <summary>
    /// Scalar multiplication
    /// </summary>
    public static Matrix operator *(Matrix m, double scalar)
    {
        var result = new Matrix(m.Rows, m.Columns);
        for (int i = 0; i < m.Rows; i++)
        {
            for (int j = 0; j < m.Columns; j++)
            {
                result[i, j] = m[i, j] * scalar;
            }
        }
        return result;
    }
    
    public static Matrix operator *(double scalar, Matrix m) => m * scalar;
    
    #endregion
    
    #region Matrix Operations
    
    /// <summary>
    /// Returns the transpose of this matrix
    /// </summary>
    public Matrix Transpose()
    {
        var result = new Matrix(Columns, Rows);
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                result[j, i] = this[i, j];
            }
        }
        return result;
    }
    
    /// <summary>
    /// Calculates the determinant of the matrix
    /// </summary>
    public double Determinant()
    {
        if (Rows != Columns)
            throw new InvalidOperationException("Determinant can only be calculated for square matrices");
            
        if (Rows == 1)
            return this[0, 0];
            
        if (Rows == 2)
            return this[0, 0] * this[1, 1] - this[0, 1] * this[1, 0];
            
        // LU decomposition for larger matrices
        var lu = LUDecomposition();
        double det = lu.pivot;
        
        for (int i = 0; i < Rows; i++)
        {
            det *= lu.L[i, i] * lu.U[i, i];
        }
        
        return det;
    }
    
    /// <summary>
    /// Calculates the inverse of the matrix
    /// </summary>
    public Matrix Inverse()
    {
        if (Rows != Columns)
            throw new InvalidOperationException("Only square matrices can be inverted");
            
        double det = Determinant();
        if (Math.Abs(det) < 1e-10)
            throw new InvalidOperationException("Matrix is singular (non-invertible)");
            
        // For 2x2 matrix, use direct formula
        if (Rows == 2)
        {
            var result = new Matrix(2, 2);
            result[0, 0] = this[1, 1] / det;
            result[0, 1] = -this[0, 1] / det;
            result[1, 0] = -this[1, 0] / det;
            result[1, 1] = this[0, 0] / det;
            return result;
        }
        
        // Use Gauss-Jordan elimination for larger matrices
        return GaussJordanInverse();
    }
    
    /// <summary>
    /// LU Decomposition
    /// </summary>
    private (Matrix L, Matrix U, double pivot) LUDecomposition()
    {
        int n = Rows;
        var L = Identity(n);
        var U = new Matrix(data);
        double pivot = 1;
        
        for (int k = 0; k < n - 1; k++)
        {
            // Partial pivoting
            int maxRow = k;
            for (int i = k + 1; i < n; i++)
            {
                if (Math.Abs(U[i, k]) > Math.Abs(U[maxRow, k]))
                    maxRow = i;
            }
            
            if (maxRow != k)
            {
                // Swap rows
                for (int j = 0; j < n; j++)
                {
                    (U[k, j], U[maxRow, j]) = (U[maxRow, j], U[k, j]);
                }
                pivot *= -1;
            }
            
            for (int i = k + 1; i < n; i++)
            {
                L[i, k] = U[i, k] / U[k, k];
                for (int j = k; j < n; j++)
                {
                    U[i, j] -= L[i, k] * U[k, j];
                }
            }
        }
        
        return (L, U, pivot);
    }
    
    /// <summary>
    /// Gauss-Jordan elimination for matrix inversion
    /// </summary>
    private Matrix GaussJordanInverse()
    {
        int n = Rows;
        var augmented = new Matrix(n, 2 * n);
        
        // Create augmented matrix [A|I]
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                augmented[i, j] = this[i, j];
                augmented[i, j + n] = (i == j) ? 1 : 0;
            }
        }
        
        // Forward elimination
        for (int i = 0; i < n; i++)
        {
            // Find pivot
            int maxRow = i;
            for (int k = i + 1; k < n; k++)
            {
                if (Math.Abs(augmented[k, i]) > Math.Abs(augmented[maxRow, i]))
                    maxRow = k;
            }
            
            // Swap rows
            if (maxRow != i)
            {
                for (int j = 0; j < 2 * n; j++)
                {
                    (augmented[i, j], augmented[maxRow, j]) = (augmented[maxRow, j], augmented[i, j]);
                }
            }
            
            // Scale pivot row
            double pivot = augmented[i, i];
            for (int j = 0; j < 2 * n; j++)
            {
                augmented[i, j] /= pivot;
            }
            
            // Eliminate column
            for (int k = 0; k < n; k++)
            {
                if (k != i)
                {
                    double factor = augmented[k, i];
                    for (int j = 0; j < 2 * n; j++)
                    {
                        augmented[k, j] -= factor * augmented[i, j];
                    }
                }
            }
        }
        
        // Extract inverse from augmented matrix
        var result = new Matrix(n, n);
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                result[i, j] = augmented[i, j + n];
            }
        }
        
        return result;
    }
    
    #endregion
    
    #region Static Methods
    
    /// <summary>
    /// Creates an identity matrix of given size
    /// </summary>
    public static Matrix Identity(int size)
    {
        var result = new Matrix(size, size);
        for (int i = 0; i < size; i++)
        {
            result[i, i] = 1;
        }
        return result;
    }
    
    /// <summary>
    /// Creates a zero matrix
    /// </summary>
    public static Matrix Zero(int rows, int cols)
    {
        return new Matrix(rows, cols);
    }
    
    /// <summary>
    /// Creates a matrix from a string representation
    /// </summary>
    public static Matrix Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be empty");
            
        // Remove outer brackets if present
        input = input.Trim();
        if (input.StartsWith("[[") && input.EndsWith("]]"))
        {
            input = input.Substring(1, input.Length - 2);
        }
        else if (input.StartsWith("[") && input.EndsWith("]"))
        {
            input = input.Substring(1, input.Length - 1);
        }
        
        // Split into rows
        var rows = input.Split(new[] { "],[", "], [" }, StringSplitOptions.RemoveEmptyEntries);
        
        if (rows.Length == 0)
            throw new ArgumentException("Invalid matrix format");
            
        // Parse first row to get column count
        var firstRow = rows[0].Trim('[', ']').Split(',').Select(x => double.Parse(x.Trim())).ToArray();
        int numCols = firstRow.Length;
        
        var data = new double[rows.Length, numCols];
        
        for (int i = 0; i < rows.Length; i++)
        {
            var row = rows[i].Trim('[', ']').Split(',').Select(x => double.Parse(x.Trim())).ToArray();
            
            if (row.Length != numCols)
                throw new ArgumentException("All rows must have the same number of columns");
                
            for (int j = 0; j < numCols; j++)
            {
                data[i, j] = row[j];
            }
        }
        
        return new Matrix(data);
    }
    
    #endregion
    
    #region Equality and ToString
    
    public bool Equals(Matrix? other)
    {
        if (other == null || Rows != other.Rows || Columns != other.Columns)
            return false;
            
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                if (Math.Abs(this[i, j] - other[i, j]) > 1e-10)
                    return false;
            }
        }
        
        return true;
    }
    
    public override bool Equals(object? obj) => Equals(obj as Matrix);
    
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + Rows.GetHashCode();
            hash = hash * 23 + Columns.GetHashCode();
            
            for (int i = 0; i < Math.Min(Rows, 3); i++)
            {
                for (int j = 0; j < Math.Min(Columns, 3); j++)
                {
                    hash = hash * 23 + this[i, j].GetHashCode();
                }
            }
            
            return hash;
        }
    }
    
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append('[');
        
        for (int i = 0; i < Rows; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append('[');
            
            for (int j = 0; j < Columns; j++)
            {
                if (j > 0) sb.Append(", ");
                sb.Append(this[i, j].ToString("G4"));
            }
            
            sb.Append(']');
        }
        
        sb.Append(']');
        return sb.ToString();
    }
    
    /// <summary>
    /// Pretty print matrix
    /// </summary>
    public string ToPrettyString()
    {
        var sb = new StringBuilder();
        
        // Find max width for each column
        var widths = new int[Columns];
        for (int j = 0; j < Columns; j++)
        {
            for (int i = 0; i < Rows; i++)
            {
                widths[j] = Math.Max(widths[j], this[i, j].ToString("F3").Length);
            }
        }
        
        for (int i = 0; i < Rows; i++)
        {
            sb.Append("| ");
            for (int j = 0; j < Columns; j++)
            {
                sb.Append(this[i, j].ToString("F3").PadLeft(widths[j]));
                sb.Append(" ");
            }
            sb.AppendLine("|");
        }
        
        return sb.ToString();
    }
    
    #endregion
}