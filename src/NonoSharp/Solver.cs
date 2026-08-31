using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NonoSharp
{
    internal class SolverHelperMethods
    {
        private static readonly CellType[] typesToCheck = [CellType.FILLED, CellType.CROSS];
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
        public static void ComputePermutations(CellType[] line, Hints hints, int index, List<CellType[]> currentlyFound)
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

    internal interface ISolveGridStrategy
    {
        void SolveGrid(Grid grid);

    }

    internal class OldSolverStrategy : SolverHelperMethods, ISolveGridStrategy 
    {
        public void SolveGrid(Grid grid) {
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
    }

    internal class ImmediateDFSStrategy : SolverHelperMethods, ISolveGridStrategy
    {
        public void SolveGrid(Grid grid)
        {
            bool changed;

            do
            {
                changed = false;

                for (int i = 0; i < grid.Width; i++)
                {

                    changed |= DoDFSFrom(grid, i, true);
                }

                for (int j = 0; j < grid.Height; j++)
                {
                    changed |= DoDFSFrom(grid, j, false);
                }
            } while (changed);
        }

        /// <summary>
        /// Checks for cells that are allowed to change, changes them and immediately recurses.
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="col"></param>
        /// <param name="row"></param>
        /// <param name="inColumn"></param>
        /// <returns>True if a cell changed, false otherwise</returns>
        private bool DoDFSFrom(Grid grid, int idx, bool inColumn)
        {
            var line = inColumn ? grid.GetColumnArray(idx) : grid.GetRowArray(idx);
            var hints = inColumn ? grid.ColumnHints[idx] : grid.RowHints[idx];


            bool changed = false;
            for (int i = 0; i < line.Length; i++)
            {
                // Get cell value in case it was changed in other 
                CellType updatedCell = inColumn ? grid.GetCell(idx, i) : grid.GetCell(i, idx);
                

                // Check if this cell was not changed in other iterations or is unallowed to change
                if (updatedCell != CellType.BLANK)
                {
                    continue;
                }




                List<CellType[]> permutations = new();
                ComputePermutations(line, hints, 0, permutations);


                // Check if there are no valid permutations, in that case, either we have not enough
                // information yet, or the grid is unsolvable, depending on the current state
                if (permutations.Count == 0)
                {
                    return false;
                }


                CellType baseCellType = permutations[0][i];
                int j;
                for (j = 1; j < permutations.Count; j++)
                {
                    if (permutations[j][i] != baseCellType)
                    {
                        break;
                    }
                }

                // Check if the loop was completed
                if (j == permutations.Count)
                {
                    changed = true;

                    // Immediately recurse as changing this cell can give information to the other line type
                    if (inColumn)
                    {
                        grid.SetCell(idx, i, baseCellType);
                        DoDFSFrom(grid, i, false);
                    }
                    else
                    {
                        grid.SetCell(i, idx, baseCellType);
                        DoDFSFrom(grid, i, true);
                    }
                }
            }
            return changed;
        }
    }

    internal class CompleteLineThenDFSStrategy : SolverHelperMethods, ISolveGridStrategy
    {
        private UniqueQueue<(bool, int)> queue;

        internal CompleteLineThenDFSStrategy()
        {
            queue = [];
        }

        public void SolveGrid(Grid grid)
        {
            bool changed;
            do
            {
                changed = false;
                for (int i = 0; i < grid.Width; i++)
                {
                    queue.Enqueue((true, i));
                    changed |= HandleQueue(grid);
                }

                for (int j = 0; j < grid.Height; j++)
                {
                    queue.Enqueue((false, j));
                    changed |= HandleQueue(grid);
                }
            } while (changed);
        }

        /// <summary>
        /// While there are elements in the queue, calls <see cref="DoIteration(Grid, int, bool)"/> on the next
        /// element of the queue
        /// </summary>
        /// <param name="grid">Grid to work with</param>
        /// <returns>True if elements were changed during this run, false otherwise</returns>
        private bool HandleQueue(Grid grid)
        {
            bool changed = false;
            while (queue.Count > 0)
            {
                (bool inColumn, int index) = queue.Dequeue();
                changed |= DoIteration(grid, index, inColumn);
            }

            return false;
        }

        /// <summary>
        /// Does a solve iteration. Changed cells are enqueued to the unique queue. 
        /// </summary>
        /// <param name="grid">Grid to work on</param>
        /// <param name="idx">Index of row/column</param>
        /// <param name="inColumn">Whether this iteration is in a column or not</param>
        /// <returns>True if any cells where changed, false otherwise</returns>
        private bool DoIteration(Grid grid, int idx, bool inColumn)
        {
            bool changed = false;
            var line = inColumn ? grid.GetColumnArray(idx) : grid.GetRowArray(idx);
            var hint = inColumn ? grid.ColumnHints[idx] : grid.RowHints[idx];

            List<CellType[]> perms = [];
            ComputePermutations(line, hint, 0, perms);

            if (perms.Count == 0)
            {
                return false; // No valid moves left
            }

            for (int lineIndex = 0; lineIndex < line.Length; lineIndex++)
            {
                CellType baseCellType = perms[0][lineIndex];

                int permIndex = 1;
                for (; permIndex < perms.Count; permIndex++)
                {
                    if (perms[permIndex][lineIndex] != baseCellType)
                    {
                        break;
                    }
                }

                // Check if all permutations were handled and the same type
                if (permIndex == perms.Count)
                {
                    changed = true;

                    if (inColumn)
                    {
                        grid.SetCell(idx, lineIndex, baseCellType);
                    }
                    else
                    {
                        grid.SetCell(lineIndex, idx, baseCellType);
                    }

                    // Enqueue the different direction as the cell has changed and can give information to the other cell
                    queue.Enqueue((!inColumn, lineIndex));
                }
            }
            return changed;
        }
    }

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


        private ISolveGridStrategy solveGridStrategy;

        internal Solver(ISolveGridStrategy strategy) 
        {
            this.solveGridStrategy = strategy;
        }

        /// <summary>
        /// Solves <paramref name="grid"/>.
        /// </summary>
        /// <param name="grid">Grid to solve</param>
        private void SolveGrid(Grid grid)
        {
            solveGridStrategy.SolveGrid(grid);
        }

        /// <summary>
        /// Solves <paramref name="grid"/>.
        /// </summary>
        /// <param name="grid">Grid to solve</param>
        public void Solve(Grid grid)
        {
            SolveGrid(grid);
        }

        /// <summary>
        /// Determines whether a puzzle can be solved
        /// </summary>
        /// <returns>True if the puzzle can be solved, false if not</returns>
        public static bool IsSolvable(Grid grid)
        {
            var strat = new OldSolverStrategy();
            // Grid to work on to calculate solutions (Copy of grid).
            Grid workingGrid = (Grid) grid.Clone();
            strat.SolveGrid(workingGrid);

            // At the end of all iterations, check if the puzzle is solved.
            // The loop stops either if the puzzle is solved and no lines could be improved, or if the puzzle was not solved
            // and no cells could be filled with certainty.
            return workingGrid.IsSolved();
        }

        /// <summary>
        /// Determines whether a puzzle can be solved
        /// </summary>
        /// <returns>True if the puzzle can be solved, false if not</returns>
        public static bool IsSolvable(Grid grid, ISolveGridStrategy strat)
        {
            // Grid to work on to calculate solutions (Copy of grid).
            Grid workingGrid = (Grid)grid.Clone();
            strat.SolveGrid(workingGrid);

            // At the end of all iterations, check if the puzzle is solved.
            // The loop stops either if the puzzle is solved and no lines could be improved, or if the puzzle was not solved
            // and no cells could be filled with certainty.
            return workingGrid.IsSolved();
        }
    }
}
