using System;
using System.Collections.Generic;
using System.Text;

namespace NonoSharp
{
    /// <summary>
    /// Helper class to create a random solution and to later help with pre-defined, custom solutions
    /// </summary>
    internal class SolutionHelper
    {
        /// <summary>
        /// Generates a random, solvable puzzle
        /// </summary>
        /// <param name="width">Width of the grid on which the puzzle is to be solved</param>
        /// <param name="height">Height of the grid on which the puzzle is to be solved</param>
        /// <param name="seed">Optional seed to use with randomisation</param>
        /// <returns>HashSet of CellPositions containing the coordinates of cells that must be filled</returns>
        /// <exception cref="ArgumentException">Thrown when width or height are non-positive</exception>
        public static HashSet<CellPosition> GenerateRandomSolution(int width, int height, int? seed = null)
        {
            Random random = seed.HasValue ? new Random(seed.Value) : new Random();
            HashSet<CellPosition> solution;
            Grid g = new(width, height);

            do
            {
                solution = GenerateRandomSet(width, height, random);
                g.SetSolution(solution);
            } while (!Solver.IsSolvable(g));

            return solution;
        }

        /// <summary>
        /// Generates a random HashSet of CellPositions using the Random object in <paramref name="random"/>.
        /// </summary>
        /// <param name="width">Width of grid.</param>
        /// <param name="height">Height of grid.</param>
        /// <param name="random">Random instance to generate CellPositions from.</param>
        /// <returns>A Set of random CellPositions</returns>
        internal static HashSet<CellPosition> GenerateRandomSet(int width, int height, Random random)
        {

            HashSet<CellPosition> positions = [];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (random.Next(2) == 0)
                    {
                        CellPosition p = new(x, y);
                        positions.Add(p);
                    }
                }
            }

            return positions;
        }
    }
}
