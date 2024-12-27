using System;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra; // MathNet kütüphanesi

public class HomographyCalculatorNonLinear : MonoBehaviour
{
    // Public değişkenler, Unity Editor'dan atanabilir
    public Vector2[] scenePoints = new Vector2[4]; // Sahne (2D düzlem) noktaları
    public Vector2[] imagePoints = new Vector2[4]; // Görüntü (projeced) noktaları

    [ContextMenu("Calculate Homography")]
    public void CalculateHomography()
    {
        // Check if there are exactly 4 points
        if (scenePoints.Length != 4 || imagePoints.Length != 4)
        {
            Debug.LogError("Exactly 4 point correspondences are required.");
            return;
        }

        // Convert Vector2 arrays to double[,] for Math.NET
        double[,] scenePts = ConvertVector2ArrayToDouble(scenePoints);
        double[,] imagePts = ConvertVector2ArrayToDouble(imagePoints);

        try
        {
            // Calculate homography
            var homography = ComputeHomography(scenePts, imagePts);

            // Print the result
            Debug.Log("Homography Matrix:");
            Debug.Log(homography.ToString());

            // Validate homography
            ValidateHomography(homography, scenePts, imagePts);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error calculating homography: {ex.Message}");
        }
    }

    // Compute the homography matrix using Nelder-Mead optimization
    private Matrix<double> ComputeHomography(double[,] scenePoints, double[,] imagePoints)
    {
        int numPoints = scenePoints.GetLength(0);

        if (numPoints != imagePoints.GetLength(0) || numPoints < 4)
        {
            throw new ArgumentException("At least 4 point correspondences are required.");
        }

        // Define the error function for optimization
        Func<Vector<double>, double> errorFunction = h =>
        {
            double error = 0.0;

            for (int i = 0; i < numPoints; i++)
            {
                double x = scenePoints[i, 0];
                double y = scenePoints[i, 1];
                double u = imagePoints[i, 0];
                double v = imagePoints[i, 1];

                // Homography transformation
                double w = h[6] * x + h[7] * y + 1;
                double uProjected = (h[0] * x + h[1] * y + h[2]) / w;
                double vProjected = (h[3] * x + h[4] * y + h[5]) / w;

                // Sum of squared errors
                error += Math.Pow(u - uProjected, 2) + Math.Pow(v - vProjected, 2);
            }

            return error;
        };

        // Initial guess for the homography parameters
        var initialGuess = Vector<double>.Build.DenseOfArray(new double[] { 1, 0, 0, 0, 1, 0, 0, 0 });

        // Perform Nelder-Mead optimization
        var result = NelderMeadOptimizer.Optimize(errorFunction, initialGuess, tolerance: 1e-4, maxIterations: 5000);

        // Get the optimized parameters
        var h = result;

        // Construct the 3x3 homography matrix
        return Matrix<double>.Build.DenseOfArray(new double[,]
        {
            { h[0], h[1], h[2] },
            { h[3], h[4], h[5] },
            { h[6], h[7], 1 }
        });
    }

    // Validate the homography matrix
    private void ValidateHomography(Matrix<double> homography, double[,] scenePoints, double[,] imagePoints)
    {
        int numPoints = scenePoints.GetLength(0);
        double totalError = 0.0;

        Debug.Log("Validation Results:");

        for (int i = 0; i < numPoints; i++)
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

    // Helper function to convert Vector2[] to double[,]
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