using System;
using System.Collections.Generic;
using System.Linq;
namespace MathFlow.Core.LinearAlgebra;
/// <summary>
/// Advanced matrix operations including eigenvalues and eigenvectors
/// </summary>
public static class MatrixOperations
{
    /// <summary>
    /// Result of eigenvalue decomposition
    /// </summary>
    public class EigenResult
    {
        public double[] Eigenvalues { get; set; } = Array.Empty<double>();
        public Matrix? Eigenvectors { get; set; }
    }
    
    /// <summary>
    /// Calculates eigenvalues and eigenvectors using QR algorithm
    /// </summary>
    public static EigenResult Eigen(Matrix matrix, int maxIterations = 1000, double tolerance = 1e-10)
    {
        if (matrix.Rows != matrix.Columns)
            throw new InvalidOperationException("Eigenvalues can only be calculated for square matrices");
            
        int n = matrix.Rows;
        var A = new Matrix(matrix.ToArray());
        var V = Matrix.Identity(n); // Will accumulate eigenvectors
        
        // QR Algorithm
        for (int iter = 0; iter < maxIterations; iter++)
        {
            var qr = QRDecomposition(A);
            A = qr.R * qr.Q;
            V = V * qr.Q;
            
            // Check for convergence (off-diagonal elements approaching zero)
            double offDiagonal = 0;
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i != j)
                        offDiagonal += Math.Abs(A[i, j]);
                }
            }
            
            if (offDiagonal < tolerance)
                break;
        }
        
        // Extract eigenvalues from diagonal
        var eigenvalues = new double[n];
        for (int i = 0; i < n; i++)
        {
            eigenvalues[i] = A[i, i];
        }
        
        // Sort by magnitude
        var indices = Enumerable.Range(0, n)
            .OrderByDescending(i => Math.Abs(eigenvalues[i]))
            .ToArray();
            
        var sortedEigenvalues = indices.Select(i => eigenvalues[i]).ToArray();
        
        // Reorder eigenvectors accordingly
        var sortedEigenvectors = new Matrix(n, n);
        for (int j = 0; j < n; j++)
        {
            for (int i = 0; i < n; i++)
            {
                sortedEigenvectors[i, j] = V[i, indices[j]];
            }
        }
        
        return new EigenResult
        {
            Eigenvalues = sortedEigenvalues,
            Eigenvectors = sortedEigenvectors
        };
    }
    
    /// <summary>
    /// QR Decomposition using Gram-Schmidt process
    /// </summary>
    public static (Matrix Q, Matrix R) QRDecomposition(Matrix matrix)
    {
        int m = matrix.Rows;
        int n = matrix.Columns;
        
        var Q = new Matrix(m, n);
        var R = new Matrix(n, n);
        
        for (int j = 0; j < n; j++)
        {
            // Get column j
            var v = GetColumn(matrix, j);
            
            // Subtract projections onto previous columns
            for (int i = 0; i < j; i++)
            {
                var q = GetColumn(Q, i);
                R[i, j] = DotProduct(q, v);
                v = SubtractVector(v, MultiplyVector(q, R[i, j]));
            }
            
            // Normalize
            R[j, j] = VectorNorm(v);
            if (Math.Abs(R[j, j]) > 1e-10)
            {
                v = MultiplyVector(v, 1.0 / R[j, j]);
            }
            
            // Set column j of Q
            SetColumn(Q, j, v);
        }
        
        return (Q, R);
    }
    
    /// <summary>
    /// Calculates matrix rank
    /// </summary>
    public static int Rank(Matrix matrix, double tolerance = 1e-10)
    {
        // Use SVD or row echelon form
        var rref = RowEchelonForm(matrix);
        int rank = 0;
        
        for (int i = 0; i < Math.Min(rref.Rows, rref.Columns); i++)
        {
            bool nonZeroRow = false;
            for (int j = 0; j < rref.Columns; j++)
            {
                if (Math.Abs(rref[i, j]) > tolerance)
                {
                    nonZeroRow = true;
                    break;
                }
            }
            if (nonZeroRow) rank++;
        }
        
        return rank;
    }
    
    /// <summary>
    /// Calculates matrix trace (sum of diagonal elements)
    /// </summary>
    public static double Trace(Matrix matrix)
    {
        if (matrix.Rows != matrix.Columns)
            throw new InvalidOperationException("Trace can only be calculated for square matrices");
            
        double trace = 0;
        for (int i = 0; i < matrix.Rows; i++)
        {
            trace += matrix[i, i];
        }
        
        return trace;
    }
    
    /// <summary>
    /// Solves linear system Ax = b
    /// </summary>
    public static double[] Solve(Matrix A, double[] b)
    {
        if (A.Rows != A.Columns)
            throw new InvalidOperationException("Matrix must be square");
            
        if (A.Rows != b.Length)
            throw new ArgumentException("Dimension mismatch");
            
        // Use Gaussian elimination with partial pivoting
        int n = A.Rows;
        var augmented = new Matrix(n, n + 1);
        
        // Create augmented matrix [A|b]
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                augmented[i, j] = A[i, j];
            }
            augmented[i, n] = b[i];
        }
        
        // Forward elimination
        for (int k = 0; k < n; k++)
        {
            // Find pivot
            int maxRow = k;
            for (int i = k + 1; i < n; i++)
            {
                if (Math.Abs(augmented[i, k]) > Math.Abs(augmented[maxRow, k]))
                    maxRow = i;
            }
            
            // Swap rows
            if (maxRow != k)
            {
                for (int j = k; j <= n; j++)
                {
                    (augmented[k, j], augmented[maxRow, j]) = (augmented[maxRow, j], augmented[k, j]);
                }
            }
            
            // Eliminate column
            for (int i = k + 1; i < n; i++)
            {
                double factor = augmented[i, k] / augmented[k, k];
                for (int j = k; j <= n; j++)
                {
                    augmented[i, j] -= factor * augmented[k, j];
                }
            }
        }
        
        // Back substitution
        var x = new double[n];
        for (int i = n - 1; i >= 0; i--)
        {
            x[i] = augmented[i, n];
            for (int j = i + 1; j < n; j++)
            {
                x[i] -= augmented[i, j] * x[j];
            }
            x[i] /= augmented[i, i];
        }
        
        return x;
    }
    
    #region Helper Methods
    
    private static Matrix RowEchelonForm(Matrix matrix)
    {
        var result = new Matrix(matrix.ToArray());
        int lead = 0;
        
        for (int r = 0; r < result.Rows; r++)
        {
            if (lead >= result.Columns)
                return result;
                
            int i = r;
            while (Math.Abs(result[i, lead]) < 1e-10)
            {
                i++;
                if (i == result.Rows)
                {
                    i = r;
                    lead++;
                    if (lead == result.Columns)
                        return result;
                }
            }
            
            // Swap rows i and r
            if (i != r)
            {
                for (int j = 0; j < result.Columns; j++)
                {
                    (result[i, j], result[r, j]) = (result[r, j], result[i, j]);
                }
            }
            
            // Divide row r by result[r, lead]
            double div = result[r, lead];
            if (Math.Abs(div) > 1e-10)
            {
                for (int j = 0; j < result.Columns; j++)
                {
                    result[r, j] /= div;
                }
            }
            
            // Subtract multiples of row r from all other rows
            for (int j = 0; j < result.Rows; j++)
            {
                if (j != r)
                {
                    double mult = result[j, lead];
                    for (int k = 0; k < result.Columns; k++)
                    {
                        result[j, k] -= mult * result[r, k];
                    }
                }
            }
            
            lead++;
        }
        
        return result;
    }
    
    private static double[] GetColumn(Matrix matrix, int col)
    {
        var column = new double[matrix.Rows];
        for (int i = 0; i < matrix.Rows; i++)
        {
            column[i] = matrix[i, col];
        }
        return column;
    }
    
    private static void SetColumn(Matrix matrix, int col, double[] values)
    {
        for (int i = 0; i < matrix.Rows && i < values.Length; i++)
        {
            matrix[i, col] = values[i];
        }
    }
    
    private static double DotProduct(double[] a, double[] b)
    {
        double sum = 0;
        for (int i = 0; i < a.Length && i < b.Length; i++)
        {
            sum += a[i] * b[i];
        }
        return sum;
    }
    
    private static double VectorNorm(double[] v)
    {
        return Math.Sqrt(DotProduct(v, v));
    }
    
    private static double[] SubtractVector(double[] a, double[] b)
    {
        var result = new double[a.Length];
        for (int i = 0; i < a.Length; i++)
        {
            result[i] = a[i] - (i < b.Length ? b[i] : 0);
        }
        return result;
    }
    
    private static double[] MultiplyVector(double[] v, double scalar)
    {
        return v.Select(x => x * scalar).ToArray();
    }
    
    #endregion
}
/// <summary>
/// Extension methods for Matrix
/// </summary>
public static class MatrixExtensions
{
    /// <summary>
    /// Converts matrix to 2D array
    /// </summary>
    public static double[,] ToArray(this Matrix matrix)
    {
        var result = new double[matrix.Rows, matrix.Columns];
        for (int i = 0; i < matrix.Rows; i++)
        {
            for (int j = 0; j < matrix.Columns; j++)
            {
                result[i, j] = matrix[i, j];
            }
        }
        return result;
    }
    
    /// <summary>
    /// Check if matrix is symmetric
    /// </summary>
    public static bool IsSymmetric(this Matrix matrix)
    {
        if (matrix.Rows != matrix.Columns)
            return false;
            
        for (int i = 0; i < matrix.Rows; i++)
        {
            for (int j = i + 1; j < matrix.Columns; j++)
            {
                if (Math.Abs(matrix[i, j] - matrix[j, i]) > 1e-10)
                    return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Check if matrix is diagonal
    /// </summary>
    public static bool IsDiagonal(this Matrix matrix)
    {
        for (int i = 0; i < matrix.Rows; i++)
        {
            for (int j = 0; j < matrix.Columns; j++)
            {
                if (i != j && Math.Abs(matrix[i, j]) > 1e-10)
                    return false;
            }
        }
        
        return true;
    }
}