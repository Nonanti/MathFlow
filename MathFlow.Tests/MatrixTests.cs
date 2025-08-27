using Xunit;
using MathFlow.Core;
using MathFlow.Core.LinearAlgebra;
using System;

namespace MathFlow.Tests;

public class MatrixTests
{
    private readonly MathEngine engine;
    
    public MatrixTests()
    {
        engine = new MathEngine();
    }
    
    [Fact]
    public void TestMatrixParsing()
    {
        var matrix = Matrix.Parse("[[1,2],[3,4]]");
        Assert.Equal(2, matrix.Rows);
        Assert.Equal(2, matrix.Columns);
        Assert.Equal(1, matrix[0, 0]);
        Assert.Equal(2, matrix[0, 1]);
        Assert.Equal(3, matrix[1, 0]);
        Assert.Equal(4, matrix[1, 1]);
    }
    
    [Fact]
    public void TestMatrixAddition()
    {
        var A = Matrix.Parse("[[1,2],[3,4]]");
        var B = Matrix.Parse("[[5,6],[7,8]]");
        var C = A + B;
        
        Assert.Equal(6, C[0, 0]);
        Assert.Equal(8, C[0, 1]);
        Assert.Equal(10, C[1, 0]);
        Assert.Equal(12, C[1, 1]);
    }
    
    [Fact]
    public void TestMatrixMultiplication()
    {
        var A = Matrix.Parse("[[1,2],[3,4]]");
        var B = Matrix.Parse("[[2,0],[1,2]]");
        var C = A * B;
        
        Assert.Equal(4, C[0, 0]);  // 1*2 + 2*1 = 4
        Assert.Equal(4, C[0, 1]);  // 1*0 + 2*2 = 4
        Assert.Equal(10, C[1, 0]); // 3*2 + 4*1 = 10
        Assert.Equal(8, C[1, 1]);  // 3*0 + 4*2 = 8
    }
    
    [Fact]
    public void TestMatrixTranspose()
    {
        var A = Matrix.Parse("[[1,2,3],[4,5,6]]");
        var AT = A.Transpose();
        
        Assert.Equal(3, AT.Rows);
        Assert.Equal(2, AT.Columns);
        Assert.Equal(1, AT[0, 0]);
        Assert.Equal(4, AT[0, 1]);
        Assert.Equal(2, AT[1, 0]);
        Assert.Equal(5, AT[1, 1]);
    }
    
    [Fact]
    public void TestMatrixDeterminant()
    {
        // 2x2 matrix
        var det2x2 = engine.Determinant("[[1,2],[3,4]]");
        Assert.Equal(-2, det2x2); // 1*4 - 2*3 = -2
        
        // 3x3 matrix
        var det3x3 = engine.Determinant("[[1,2,3],[4,5,6],[7,8,9]]");
        Assert.True(Math.Abs(det3x3) < 1e-10); // This matrix is singular
        
        // Another 3x3 matrix
        var det3x3b = engine.Determinant("[[2,1,3],[1,0,1],[0,2,1]]");
        Assert.Equal(1, det3x3b); // Corrected expected value
    }
    
    [Fact]
    public void TestMatrixInverse()
    {
        var matrix = Matrix.Parse("[[1,2],[3,4]]");
        var inverse = matrix.Inverse();
        var identity = matrix * inverse;
        
        // Check if result is identity matrix
        Assert.True(Math.Abs(identity[0, 0] - 1) < 1e-10);
        Assert.True(Math.Abs(identity[0, 1] - 0) < 1e-10);
        Assert.True(Math.Abs(identity[1, 0] - 0) < 1e-10);
        Assert.True(Math.Abs(identity[1, 1] - 1) < 1e-10);
        
        // Check specific values
        Assert.True(Math.Abs(inverse[0, 0] - (-2)) < 1e-10);
        Assert.True(Math.Abs(inverse[0, 1] - 1) < 1e-10);
        Assert.True(Math.Abs(inverse[1, 0] - 1.5) < 1e-10);
        Assert.True(Math.Abs(inverse[1, 1] - (-0.5)) < 1e-10);
    }
    
    [Fact]
    public void TestMatrixInverse_SingularMatrix_ThrowsException()
    {
        var matrix = Matrix.Parse("[[1,2],[2,4]]"); // Singular matrix
        Assert.Throws<InvalidOperationException>(() => matrix.Inverse());
    }
    
    [Fact]
    public void TestIdentityMatrix()
    {
        var identity = Matrix.Identity(3);
        
        Assert.Equal(1, identity[0, 0]);
        Assert.Equal(0, identity[0, 1]);
        Assert.Equal(0, identity[1, 0]);
        Assert.Equal(1, identity[1, 1]);
        Assert.Equal(1, identity[2, 2]);
    }
    
    [Fact]
    public void TestMatrixScalarMultiplication()
    {
        var matrix = Matrix.Parse("[[1,2],[3,4]]");
        var scaled = matrix * 2;
        
        Assert.Equal(2, scaled[0, 0]);
        Assert.Equal(4, scaled[0, 1]);
        Assert.Equal(6, scaled[1, 0]);
        Assert.Equal(8, scaled[1, 1]);
    }
    
    [Fact]
    public void TestLinearSystemSolver()
    {
        // Solve: 2x + y = 5
        //        x + 3y = 7
        var solution = engine.SolveLinearSystem("[[2,1],[1,3]]", new[] { 5.0, 7.0 });
        
        Assert.Equal(2, solution.Length);
        Assert.True(Math.Abs(solution[0] - 8.0/5.0) < 1e-10); // x = 1.6
        Assert.True(Math.Abs(solution[1] - 9.0/5.0) < 1e-10); // y = 1.8
    }
    
    [Fact]
    public void TestMatrixRank()
    {
        var rank1 = engine.MatrixRank("[[1,2,3],[2,4,6],[1,2,3]]");
        Assert.Equal(1, rank1); // All rows are linearly dependent
        
        var rank2 = engine.MatrixRank("[[1,2],[3,4]]");
        Assert.Equal(2, rank2); // Full rank
        
        var rank3 = engine.MatrixRank("[[1,0,2],[0,1,1],[1,1,3]]");
        Assert.Equal(2, rank3); // This matrix has rank 2, not 3
    }
    
    [Fact]
    public void TestMatrixTrace()
    {
        var trace = engine.MatrixTrace("[[1,2,3],[4,5,6],[7,8,9]]");
        Assert.Equal(15, trace); // 1 + 5 + 9 = 15
    }
    
    [Fact]
    public void TestEigenvalues()
    {
        // Simple 2x2 matrix with known eigenvalues
        var result = engine.Eigen("[[3,1],[0,2]]");
        
        Assert.NotNull(result.Eigenvalues);
        Assert.Equal(2, result.Eigenvalues.Length);
        
        // Eigenvalues should be 3 and 2
        Assert.Contains(result.Eigenvalues, x => Math.Abs(x - 3) < 1e-10);
        Assert.Contains(result.Eigenvalues, x => Math.Abs(x - 2) < 1e-10);
    }
    
    [Fact]
    public void TestSymmetricMatrix()
    {
        var symmetric = Matrix.Parse("[[1,2,3],[2,4,5],[3,5,6]]");
        Assert.True(symmetric.IsSymmetric());
        
        var nonSymmetric = Matrix.Parse("[[1,2],[3,4]]");
        Assert.False(nonSymmetric.IsSymmetric());
    }
    
    [Fact]
    public void TestDiagonalMatrix()
    {
        var diagonal = Matrix.Parse("[[1,0,0],[0,2,0],[0,0,3]]");
        Assert.True(diagonal.IsDiagonal());
        
        var nonDiagonal = Matrix.Parse("[[1,2],[0,3]]");
        Assert.False(nonDiagonal.IsDiagonal());
    }
    
    [Fact]
    public void TestMatrixToString()
    {
        var matrix = Matrix.Parse("[[1,2],[3,4]]");
        var str = matrix.ToString();
        Assert.Equal("[[1, 2], [3, 4]]", str);
    }
    
    [Fact]
    public void TestMatrixEquality()
    {
        var A = Matrix.Parse("[[1,2],[3,4]]");
        var B = Matrix.Parse("[[1,2],[3,4]]");
        var C = Matrix.Parse("[[1,2],[3,5]]");
        
        Assert.True(A.Equals(B));
        Assert.False(A.Equals(C));
    }
    
    [Fact]
    public void TestComplexMatrixOperation()
    {
        // Test (A * B) + C
        var A = Matrix.Parse("[[1,2],[3,4]]");
        var B = Matrix.Parse("[[2,0],[1,2]]");
        var C = Matrix.Parse("[[1,1],[1,1]]");
        
        var result = (A * B) + C;
        
        Assert.Equal(5, result[0, 0]);  // 4 + 1
        Assert.Equal(5, result[0, 1]);  // 4 + 1
        Assert.Equal(11, result[1, 0]); // 10 + 1
        Assert.Equal(9, result[1, 1]);  // 8 + 1
    }
}