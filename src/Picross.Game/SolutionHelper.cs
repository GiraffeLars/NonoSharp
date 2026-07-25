using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Picross.Game
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
        /// <returns>List of Points containing the coordinates of cells that must be filled</returns>
        public static List<Point> GenerateRandomSolution(int width, int height)
        {

            List<Point> solution = GetRandomSolution(width, height);
            Grid g = new(width, height, solution);

            while (!Solver.IsSolvable(g))
            {
                solution = GetRandomSolution(width, height);
                g.SetSolution(solution);
            }

            return solution;
        }

        internal static List<Point> GetRandomSolution(int width, int height)
        {
            var random = new Random();
            List<Point> points = new List<Point>();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (random.NextInt64() % 2 == 0)
                    {
                        Point p = new Point(x, y);
                        points.Add(p);
                    }
                }
            }

            return points;
        }
    }
}
