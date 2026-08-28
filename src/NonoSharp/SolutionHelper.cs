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
        /// <returns>HashSet of CellPositions containing the coordinates of cells that must be filled</returns>
        public static HashSet<CellPosition> GenerateRandomSolution(int width, int height)
        {

            HashSet<CellPosition> solution = GetRandomSolution(width, height);
            Grid g = new(width, height, solution);

            while (!Solver.IsSolvable(g))
            {
                solution = GetRandomSolution(width, height);
                g.SetSolution(solution);
            }

            return solution;
        }

        internal static HashSet<CellPosition> GetRandomSolution(int width, int height)
        {
            var random = new Random();
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
