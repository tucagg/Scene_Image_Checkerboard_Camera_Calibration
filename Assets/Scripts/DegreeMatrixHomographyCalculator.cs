using System;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;

public class DegreeMatrixHomographyCalculator : MonoBehaviour
{
    public Vector2[] scenePoints = new Vector2[4];
    public Vector2[] imagePoints = new Vector2[4];
    public double[,] degreeMatrix = new double[4, 4];

    [ContextMenu("Calculate Homography with Degree Matrix")]
    public void CalculateHomographyWithDegreeMatrix()
    {
        if (scenePoints.Length != degreeMatrix.GetLength(0) || imagePoints.Length != degreeMatrix.GetLength(1))
        {
            Debug.LogError("Degree matrix dimensions do not match scene and image points.");
            return;
        }

        try
        {
            var matchedScenePoints = new Vector2[scenePoints.Length];
            var matchedImagePoints = new Vector2[scenePoints.Length];
            FindBestMatches(matchedScenePoints, matchedImagePoints);

            double[,] matchedScenePts = ConvertVector2ArrayToDouble(matchedScenePoints);
            double[,] matchedImagePts = ConvertVector2ArrayToDouble(matchedImagePoints);

            var homography = ComputeHomography(matchedScenePts, matchedImagePts);

            Debug.Log("Homography Matrix:");
            Debug.Log(homography.ToString());

            ValidateHomography(homography, matchedScenePts, matchedImagePts);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error calculating homography: {ex.Message}");
        }
    }

    private void FindBestMatches(Vector2[] matchedScenePoints, Vector2[] matchedImagePoints)
    {
        for (int i = 0; i < degreeMatrix.GetLength(0); i++)
        {
            int bestMatchIndex = -1;
            double maxDegree = double.MinValue;

            for (int j = 0; j < degreeMatrix.GetLength(1); j++)
            {
                if (degreeMatrix[i, j] > maxDegree)
                {
                    maxDegree = degreeMatrix[i, j];
                    bestMatchIndex = j;
                }
            }

            if (bestMatchIndex >= 0)
            {
                matchedScenePoints[i] = scenePoints[i];
                matchedImagePoints[i] = imagePoints[bestMatchIndex];
            }
            else
            {
                throw new Exception("No valid match found for scene point " + i);
            }
        }
    }

    private Matrix<double> ComputeHomography(double[,] scenePoints, double[,] imagePoints)
    {
        var optimizedParameters = NelderMeadOptimizer.Optimize(
            errorFunction: h =>
            {
                double error = 0.0;
                for (int i = 0; i < scenePoints.GetLength(0); i++)
                {
                    double x = scenePoints[i, 0];
                    double y = scenePoints[i, 1];
                    double u = imagePoints[i, 0];
                    double v = imagePoints[i, 1];

                    double w = h[6] * x + h[7] * y + 1;
                    double uProjected = (h[0] * x + h[1] * y + h[2]) / w;
                    double vProjected = (h[3] * x + h[4] * y + h[5]) / w;

                    error += Math.Pow(u - uProjected, 2) + Math.Pow(v - vProjected, 2);
                }
                return error;
            },
            initialGuess: Vector<double>.Build.DenseOfArray(new double[] { 1, 0, 0, 0, 1, 0, 0, 0 })
        );

        return Matrix<double>.Build.DenseOfArray(new double[,]
        {
            { optimizedParameters[0], optimizedParameters[1], optimizedParameters[2] },
            { optimizedParameters[3], optimizedParameters[4], optimizedParameters[5] },
            { optimizedParameters[6], optimizedParameters[7], 1 }
        });
    }

    private void ValidateHomography(Matrix<double> homography, double[,] scenePoints, double[,] imagePoints)
    {
        double totalError = 0.0;

        for (int i = 0; i < scenePoints.GetLength(0); i++)
        {
            var scenePoint = Vector<double>.Build.DenseOfArray(new double[] { scenePoints[i, 0], scenePoints[i, 1], 1 });
            var imagePoint = Vector<double>.Build.DenseOfArray(new double[] { imagePoints[i, 0], imagePoints[i, 1] });

            var projectedPoint = homography * scenePoint;

            double uProjected = projectedPoint[0] / projectedPoint[2];
            double vProjected = projectedPoint[1] / projectedPoint[2];

            double error = Math.Sqrt(Math.Pow(imagePoint[0] - uProjected, 2) + Math.Pow(imagePoint[1] - vProjected, 2));
            totalError += error;

            Debug.Log($"Point {i}: Projected = ({uProjected}, {vProjected}), Actual = ({imagePoint[0]}, {imagePoint[1]}), Error = {error}");
        }

        Debug.Log($"Total Error: {totalError}");
    }

    private double[,] ConvertVector2ArrayToDouble(Vector2[] points)
    {
        double[,] result = new double[points.Length, 2];
        for (int i = 0; i < points.Length; i++)
        {
            result[i, 0] = points[i].x;
            result[i, 1] = points[i].y;
        }
        return result;
    }
}