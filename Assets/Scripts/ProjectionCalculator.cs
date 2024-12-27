using MathNet.Numerics.LinearAlgebra;
using System;

public static class ProjectionCalculator
{
    public static Vector<double> CalculateProjection(Vector<double> scenePoint, Matrix<double> homographyMatrix)
    {
        if (scenePoint.Count != 2)
            throw new ArgumentException("Scene point must have exactly 2 dimensions (x, y).");

        if (homographyMatrix.RowCount != 3 || homographyMatrix.ColumnCount != 3)
            throw new ArgumentException("Homography matrix must be a 3x3 matrix.");

        var homogeneousScenePoint = Vector<double>.Build.DenseOfArray(new double[] { scenePoint[0], scenePoint[1], 1 });

        var projectedPoint = homographyMatrix * homogeneousScenePoint;

        double u = projectedPoint[0] / projectedPoint[2];
        double v = projectedPoint[1] / projectedPoint[2];

        return Vector<double>.Build.DenseOfArray(new double[] { u, v });
    }
}