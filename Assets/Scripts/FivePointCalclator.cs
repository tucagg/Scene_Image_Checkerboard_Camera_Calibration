using System;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra; // MathNet kütüphanesi

public class FivePointCalculator : MonoBehaviour
{
    // Public değişkenler, Unity Editor'dan atanabilir
    public Vector2[] scenePointsForHomography = new Vector2[5]; // Homografi için sahne noktaları
    public Vector2[] imagePointsForHomography = new Vector2[5]; // Homografi için görüntü noktaları

    public Vector2[] scenePointsForError = new Vector2[3]; // Hata hesaplama için sahne noktaları
    public Vector2[] imagePointsForError = new Vector2[3]; // Hata hesaplama için görüntü noktaları

    [ContextMenu("Calculate Homography and Errors")]
    public void CalculateHomographyAndErrors()
    {
        // Check if there are exactly 5 points for homography
        if (scenePointsForHomography.Length != 5 || imagePointsForHomography.Length != 5)
        {
            Debug.LogError("Exactly 5 point correspondences are required for homography calculation.");
            return;
        }

        // Check if there are exactly 3 points for error calculation
        if (scenePointsForError.Length != 3 || imagePointsForError.Length != 3)
        {
            Debug.LogError("Exactly 3 point correspondences are required for error calculation.");
            return;
        }

        try
        {
            // Homografi matrisini hesapla
            var homography = ComputeHomography(
                ConvertVector2ArrayToDouble(scenePointsForHomography),
                ConvertVector2ArrayToDouble(imagePointsForHomography)
            );

            Debug.Log("Homography Matrix:");
            Debug.Log(homography.ToString());

            // Hata hesapla
            CalculateAndDisplayErrors(
                homography,
                ConvertVector2ArrayToDouble(scenePointsForError),
                ConvertVector2ArrayToDouble(imagePointsForError)
            );
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error calculating homography or errors: {ex.Message}");
        }
    }

    // Homografi matrisini hesaplama
    private Matrix<double> ComputeHomography(double[,] scenePoints, double[,] imagePoints)
    {
        int numPoints = scenePoints.GetLength(0);

        if (numPoints != imagePoints.GetLength(0) || numPoints < 4)
        {
            throw new ArgumentException("At least 4 point correspondences are required.");
        }

        // Optimize edilen hata fonksiyonu
        Func<Vector<double>, double> errorFunction = h =>
        {
            double error = 0.0;

            for (int i = 0; i < numPoints; i++)
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
        };

        var initialGuess = Vector<double>.Build.DenseOfArray(new double[] { 1, 0, 0, 0, 1, 0, 0, 0 });
        var result = NelderMeadOptimizer.Optimize(errorFunction, initialGuess, tolerance: 1e-4, maxIterations: 5000);
        var h = result;

        return Matrix<double>.Build.DenseOfArray(new double[,]
        {
            { h[0], h[1], h[2] },
            { h[3], h[4], h[5] },
            { h[6], h[7], 1 }
        });
    }

    // Hata hesaplama ve görüntüleme
    private void CalculateAndDisplayErrors(Matrix<double> homography, double[,] scenePoints, double[,] imagePoints)
    {
        Debug.Log("Error Validation Results:");

        for (int i = 0; i < scenePoints.GetLength(0); i++)
        {
            var scenePoint = Vector<double>.Build.DenseOfArray(new double[] { scenePoints[i, 0], scenePoints[i, 1], 1 });
            var imagePoint = Vector<double>.Build.DenseOfArray(new double[] { imagePoints[i, 0], imagePoints[i, 1] });

            var projectedPoint = homography * scenePoint;

            double uProjected = projectedPoint[0] / projectedPoint[2];
            double vProjected = projectedPoint[1] / projectedPoint[2];

            double error = Math.Sqrt(Math.Pow(imagePoint[0] - uProjected, 2) + Math.Pow(imagePoint[1] - vProjected, 2));
            Debug.Log($"Test Point {i}: Projected = ({uProjected}, {vProjected}), Actual = ({imagePoint[0]}, {imagePoint[1]}), Error = {error}");
        }
    }

    // Yardımcı fonksiyonlar
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