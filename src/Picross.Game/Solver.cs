using System;
using System.Collections.Generic;
using System.Text;

namespace Picross.Game
{
    /// <summary>
    /// Helper class to determine whether a puzzle can be solved,
    /// and whether the puzzle can be improved with 100% certainty.
    /// At its current state, the Solver will not make any "guesses", meaning that puzzles with a valid
    /// solution that might require logical deductions might be rejected.
    /// </summary>
    internal class Solver
    {
        // The playing grid of the user, should generally not be modified
        private Grid grid;

        // Grid to work on to calculate solutions (Copy of grid).
        private Grid workingGrid;

        private static readonly SquareType[] typesToCheck = [SquareType.FILLED, SquareType.CROSS];

        internal Solver(Grid grid)
        {
            this.grid = grid;
            this.workingGrid = (Grid) grid.Clone();
        }

        /// <summary>
        /// Determines whether a puzzle can be solved
        /// </summary>
        /// <returns>True if the puzzle can be solved, false if not</returns>
        public bool IsSolvable()
        {
            bool changedInIteration;

            do
            {
                changedInIteration = false;

                // Loop through columns and improve where possible
                for (int i = 0; i < workingGrid.Width; i++)
                {
                    SquareType[] line = workingGrid.GetColumnArray(i);
                    bool changedCells = ImproveLine(line, workingGrid.VerticalHints[i]);

                    if (changedCells)
                    {
                        workingGrid.SetColumn(i, line);
                        changedInIteration = true;
                    }
                }
                
                // Loop through rows and improve where possible
                for (int i = 0; i < workingGrid.Height; i++)
                {
                    SquareType[] line = workingGrid.GetRowArray(i);
                    bool changedCells = ImproveLine(line, workingGrid.HorizontalHints[i]);

                    if (changedCells)
                    {
                        workingGrid.SetColumn(i, line);
                        changedInIteration = true;
                    }
                }
            } while (changedInIteration);

            // At the end of all iterations, check if the puzzle is solved.
            // The loop stops either if the puzzle is solved and no lines could be improved, or if the puzzle was not solved
            // and no cells could be filled with certainty.
            return workingGrid.IsSolved();
        }

        /// <summary>
        /// Tries to improve <paramref name="line"/> by determining which squares must be crosses or filled.
        /// Will modify <paramref name="line"/> by setting the squaretypes after improving. Skips non-empty cells.
        /// </summary>
        /// <param name="line">The line to improve. Improved cells will be changed in the array.</param>
        /// <param name="hints">The hints to base the improvement on</param>
        /// <returns>True if any cells were changed, false otherwise</returns>
        private bool ImproveLine(SquareType[] line, Hints hints)
        {
            List<SquareType[]> validPermutations = [];
            ComputePermutations(line, hints, 0, validPermutations);

            bool changedCells = false;
            for (int i = 0; i < line.Length; i++)
            {
                // Already placed tile, this one does not count for improvement
                if (line[i] != SquareType.BLANK)
                {
                    continue;
                }

                SquareType firstPermutationType = validPermutations[0][i];
                int j;
                for (j = 1;  j < validPermutations.Count; j++)
                {
                    if (validPermutations[j][i] != firstPermutationType)
                    {
                        break;
                    }
                }

                // Passed all permutations, then this line can improve, since all valid permutations have this cell as a certain type
                if (j == validPermutations.Count)
                {
                    line[i] = firstPermutationType;
                    changedCells = true;
                }
            }

            return changedCells;
        }

        /// <summary>
        /// Gets all possible permutations of <paramref name="line"/> based on <paramref name="hints"/>. Takes a brute force approach.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private void ComputePermutations(SquareType[] line, Hints hints, int index, List<SquareType[]> currentlyFound)
        {
            if (index >= line.Length)
            {
                // Check if all hints are satisfied
                hints.DoCompletion(line);

                for (int h = 0; h < hints.Count; h++)
                {
                    if (!hints.GetHint(h).Completed)
                    {
                        return;
                    }
                }

                SquareType[] clone = (SquareType[]) line.Clone();
                currentlyFound.Add(clone);
                return;
            }

            // Player placed tile, continue immediately
            if (line[index] != SquareType.BLANK)
            {
                ComputePermutations(line, hints, index + 1, currentlyFound);
                return;
            }

            // Check all possible combinations
            foreach (SquareType type in typesToCheck)
            {
                line[index] = type;
                ComputePermutations(line, hints, index + 1, currentlyFound);
            }

            line[index] = SquareType.BLANK;
        }
    }
}
