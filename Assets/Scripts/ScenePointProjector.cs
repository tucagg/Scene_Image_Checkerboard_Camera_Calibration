using System;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;

[Serializable]
public struct SerializableMatrix
{
    public double m00, m01, m02;
    public double m10, m11, m12;
    public double m20, m21, m22;

    public Matrix<double> ToMatrix()
    {
        return Matrix<double>.Build.DenseOfArray(new double[,]
        {
            { m00, m01, m02 },
            { m10, m11, m12 },
            { m20, m21, m22 }
        });
    }

    public void FromMatrix(Matrix<double> matrix)
    {
        m00 = matrix[0, 0]; m01 = matrix[0, 1]; m02 = matrix[0, 2];
        m10 = matrix[1, 0]; m11 = matrix[1, 1]; m12 = matrix[1, 2];
        m20 = matrix[2, 0]; m21 = matrix[2, 1]; m22 = matrix[2, 2];
    }
}

public class ScenePointProjector : MonoBehaviour
{
    public Vector2 scenePoint = new Vector2(100, 200); // Sahne noktası
    public Vector2 imagePoint = new Vector2(320, 240); // Görüntü noktası
    public SerializableMatrix homographyMatrix; // Inspector'dan ayarlanabilir matris

    [ContextMenu("Project Scene Point")]
    public void ProjectScenePoint()
    {
        try
        {
            var matrix = homographyMatrix.ToMatrix();
            var scenePointVector = Vector<double>.Build.DenseOfArray(new double[] { scenePoint.x, scenePoint.y, 1 });

            var projectedPoint = matrix * scenePointVector;
            var u = projectedPoint[0] / projectedPoint[2];
            var v = projectedPoint[1] / projectedPoint[2];

            Debug.Log($"Projected Point: ({u}, {v})");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error projecting scene point: {ex.Message}");
        }
    }

    [ContextMenu("Back-Project Image Point")]
    public void BackProjectImagePoint()
    {
        try
        {
            var matrix = homographyMatrix.ToMatrix();

            // Homography matrix inverse
            var inverseMatrix = matrix.Inverse();

            var imagePointVector = Vector<double>.Build.DenseOfArray(new double[] { imagePoint.x, imagePoint.y, 1 });

            var backProjectedPoint = inverseMatrix * imagePointVector;
            var x = backProjectedPoint[0] / backProjectedPoint[2];
            var y = backProjectedPoint[1] / backProjectedPoint[2];

            Debug.Log($"Back-Projected Scene Point: ({x}, {y})");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error back-projecting image point: {ex.Message}");
        }
    }
}