using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Picross.Game
{
    /// <summary>
    /// Helper class to determine whether a puzzle can be solved,
    /// and whether the puzzle can be improved with 100% certainty.
    /// At its current state, the Solver will not make any "guesses", meaning that puzzles with a valid
    /// solution that might require logical deductions might be rejected.
    /// Puzzles where guessing is required, i.e. without one unique answer, are rejected. 
    /// TODO replace the Solvable algorithm with a backtracking/dynamic programming aproach for better performance
    /// TODO also consider applying logical deductions, allowing for more difficult puzzles
    /// </summary>
    internal class Solver
    {
        // TODO replace the Solvable algorithm with a backtracking/dynamic programming aproach for better performance
        // TODO also consider applying logical deductions, allowing for more difficult puzzles

        private static readonly CellType[] typesToCheck = [CellType.FILLED, CellType.CROSS];

        /// <summary>
        /// Solves <paramref name="grid"/>.
        /// </summary>
        /// <param name="grid">Grid to solve</param>
        private static void SolveGrid(Grid grid)
        {
            bool changedInIteration;

            do
            {
                changedInIteration = false;

                // Loop through columns and improve where possible
                for (int i = 0; i < grid.Width; i++)
                {
                    CellType[] line = grid.GetColumnArray(i);
                    bool changedCells = ImproveLine(line, grid.ColumnHints[i]);

                    if (changedCells)
                    {
                        grid.SetColumn(i, line);
                        changedInIteration = true;
                    }
                }

                // Loop through rows and improve where possible
                for (int i = 0; i < grid.Height; i++)
                {
                    CellType[] line = grid.GetRowArray(i);

                    bool changedCells = ImproveLine(line, grid.RowHints[i]);

                    if (changedCells)
                    {
                        grid.SetRow(i, line);
                        changedInIteration = true;
                    }
                }
            } while (changedInIteration);
        }

        /// <summary>
        /// Solves <paramref name="grid"/>.
        /// </summary>
        /// <param name="grid">Grid to solve</param>
        public static void Solve(Grid grid)
        {
            SolveGrid(grid);
        }

        /// <summary>
        /// Determines whether a puzzle can be solved
        /// </summary>
        /// <returns>True if the puzzle can be solved, false if not</returns>
        public static bool IsSolvable(Grid grid)
        {
            // Grid to work on to calculate solutions (Copy of grid).
            Grid workingGrid = (Grid) grid.Clone();
            SolveGrid(workingGrid);

            // At the end of all iterations, check if the puzzle is solved.
            // The loop stops either if the puzzle is solved and no lines could be improved, or if the puzzle was not solved
            // and no cells could be filled with certainty.
            return workingGrid.IsSolved();
        }

        /// <summary>
        /// Tries to improve <paramref name="line"/> by determining which cells must be crosses or filled.
        /// Will modify <paramref name="line"/> by setting the celltypes after improving. Skips non-empty cells.
        /// </summary>
        /// <param name="line">The line to improve. Improved cells will be changed in the array.</param>
        /// <param name="hints">The hints to base the improvement on</param>
        /// <returns>True if any cells were changed, false otherwise</returns>
        internal static bool ImproveLine(CellType[] line, Hints hints)
        {
            List<CellType[]> validPermutations = [];
            ComputePermutations(line, hints, 0, validPermutations);

            if (validPermutations.Count == 0)
            {
                // This whole line has been filled in and can not be improved without replacing user placed cells
                return false;
            }

            bool changedCells = false;
            for (int i = 0; i < line.Length; i++)
            {
                // Already placed tile, this one does not count for improvement
                if (line[i] != CellType.BLANK)
                {
                    continue;
                }

                CellType firstPermutationType = validPermutations[0][i];
                int j;
                
                if (firstPermutationType == CellType.BLANK)
                {
                    Debug.WriteLine("Found blank, very bad!!!!");
                }
                for (j = 1; j < validPermutations.Count; j++)
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
        /// Gets all possible permutations of <paramref name="line"/> based on <paramref name="hints"/>, 
        /// all non-empty cells remain as they were.
        /// </summary>
        /// <param name="line">Array of CellType to compute all possible permutations of, filling/crossing only blank cells</param>
        /// <param name="hints">Hints instance corresponding to <paramref name="line"/></param>
        /// <param name="index">Current index of iteration, should be initially called as 0</param>
        /// <param name="currentlyFound">The currently found valid permutations according to <paramref name="hints"/>
        /// and already non-empty cells. This List will be modified by adding the found permutations</param>
        /// <returns>Nothing, <paramref name="currentlyFound"/> is updated.</returns>
        private static void ComputePermutations(CellType[] line, Hints hints, int index, List<CellType[]> currentlyFound)
        {
            if (index >= line.Length)
            {
                if (IsValidPermutation(line, hints))
                { 
                    CellType[] clone = (CellType[])line.Clone();
                    currentlyFound.Add(clone);
                }
                return;
            }

            // Player placed tile, continue immediately
            if (line[index] != CellType.BLANK)
            {
                ComputePermutations(line, hints, index + 1, currentlyFound);
                return;
            }

            // Check all possible combinations
            foreach (CellType type in typesToCheck)
            {
                line[index] = type;
                ComputePermutations(line, hints, index + 1, currentlyFound);
            }

            line[index] = CellType.BLANK;
        }

        private static bool IsValidPermutation(CellType[] permutation, Hints hints)
        {
            /* Hints are marked as completed, even if there are still other cells in the row, i.e. more filled in cells than 
               the hint requires.
               This is intentional behaviour, but means we must check if the total of cells filled in match the hints
            */
            int filledIn = permutation.Count(cell => cell == CellType.FILLED);
            if (filledIn != hints.TotalCellsInHints)
            {
                return false;
            }

            // Check if all hints are satisfied
            hints.DoCompletion(permutation);

            for (int h = 0; h < hints.Count; h++)
            {
                if (!hints[h].Completed)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
