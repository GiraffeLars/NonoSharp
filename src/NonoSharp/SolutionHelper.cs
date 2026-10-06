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
        /// Generates a random, solvable puzzle.
        /// </summary>
        /// <param name="width">Width of the grid on which the puzzle is to be solved.</param>
        /// <param name="height">Height of the grid on which the puzzle is to be solved.</param>
        /// <param name="seed">Optional seed to use with randomisation.</param>
        /// <returns>HashSet of CellPositions containing the coordinates of cells that must be filled.</returns>
        /// <exception cref="ArgumentException">Thrown when width or height are non-positive.</exception>
        internal static HashSet<CellPosition> GenerateRandomSolution(int width, int height, int? seed = null)
        {
            Random random = seed.HasValue ? new Random(seed.Value) : new Random();
            HashSet<CellPosition> solution;
            Puzzle p = new(width, height, null);

            do
            {
                solution = GenerateRandomSet(width, height, random);
                p.SetSolution(solution);
            } while (!Solver.IsSolvable(p));

            return solution;
        }

        /// <inheritdoc cref="GenerateRandomSolution(int, int, int?)"/>
        /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
        internal static async Task<HashSet<CellPosition>> GenerateRandomSolutionAsync(int width, int height, 
            CancellationToken cancellationToken, int? seed = null)
        {
            Random random = seed.HasValue ? new Random(seed.Value) : new Random();

            Puzzle p = new(width, height, null);
            bool isSolvable = false;

            var solution = await Task.Run(() =>
            {
                HashSet<CellPosition> solution;
                do
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    solution = GenerateRandomSet(width, height, random);
                    p.SetSolution(solution);

                    cancellationToken.ThrowIfCancellationRequested();
                    isSolvable = Solver.IsSolvable(p);
                } while (!isSolvable);
                return solution;
            });

            return solution;
        }

        /// <summary>
        /// Generates a random HashSet of CellPositions using the Random object in <paramref name="random"/>.
        /// </summary>
        /// <param name="width">Width of grid.</param>
        /// <param name="height">Height of grid.</param>
        /// <param name="random">Random instance to generate CellPositions from.</param>
        /// <returns>A HashSet of random CellPositions.</returns>
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
