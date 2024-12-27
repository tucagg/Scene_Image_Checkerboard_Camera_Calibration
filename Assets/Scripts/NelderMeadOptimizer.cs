using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;

public static class NelderMeadOptimizer
{
    public static Vector<double> Optimize(
        Func<Vector<double>, double> errorFunction,
        Vector<double> initialGuess,
        double tolerance = 1e-6,
        int maxIterations = 5000)
    {
        int n = initialGuess.Count;

        // Initialize the simplex
        List<Vector<double>> simplex = new List<Vector<double>>(n + 1);
        simplex.Add(initialGuess);

        for (int i = 0; i < n; i++)
        {
            var point = initialGuess.Clone();
            point[i] += 0.05; // Slight perturbation
            simplex.Add(point);
        }

        for (int iter = 0; iter < maxIterations; iter++)
        {
            // Sort simplex by function values
            simplex.Sort((a, b) => errorFunction(a).CompareTo(errorFunction(b)));

            // Termination criteria
            double range = errorFunction(simplex[^1]) - errorFunction(simplex[0]);
            if (range < tolerance)
                return simplex[0];

            // Compute the centroid of all points except the worst
            var centroid = Vector<double>.Build.Dense(n);
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                    centroid[j] += simplex[i][j];
            }
            centroid /= n;

            // Reflection
            var reflected = centroid + (centroid - simplex[^1]);
            double reflectedValue = errorFunction(reflected);
            if (reflectedValue < errorFunction(simplex[0]))
            {
                // Expansion
                var expanded = centroid + 2 * (reflected - centroid);
                if (errorFunction(expanded) < reflectedValue)
                    simplex[^1] = expanded;
                else
                    simplex[^1] = reflected;
            }
            else if (reflectedValue < errorFunction(simplex[^2]))
            {
                simplex[^1] = reflected;
            }
            else
            {
                // Contraction
                var contracted = centroid + 0.5 * (simplex[^1] - centroid);
                if (errorFunction(contracted) < errorFunction(simplex[^1]))
                    simplex[^1] = contracted;
                else
                {
                    // Shrink
                    for (int i = 1; i < simplex.Count; i++)
                    {
                        simplex[i] = simplex[0] + 0.5 * (simplex[i] - simplex[0]);
                    }
                }
            }
        }

        throw new Exception("Nelder-Mead did not converge within the iteration limit.");
    }
}