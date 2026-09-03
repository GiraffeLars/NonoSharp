using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NonoSharp
{
    internal class SolverHelperMethods
    {
        

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
        public virtual void ComputePermutations(CellType[] line, Hints hints, int index, List<CellType[]> currentlyFound)
        {
            PlaceHintBlocks(line, hints, 0, 0, currentlyFound);
        }

        private void PlaceHintBlocks(CellType[] permutation, Hints hints, int hintIdx, int cellIdx, List<CellType[]> found)
        {
            if (hintIdx >= hints.Count)
            {
                CellType[] clone = (CellType[])permutation.Clone();
                for (int i = cellIdx; i < permutation.Length; i++)
                {
                    // If there are more filled cells after the hints have been processed, there are too many
                    // filled cells in the line. As such, this pernutation is invalid
                    if (clone[i] == CellType.FILLED)
                    {
                        return;
                    }

                    // Cross out the cell otherwise, as all hints should have been satisfied
                    clone[i] = CellType.CROSS;
                }

                found.Add(clone);
                return;
            }

            if (cellIdx >= permutation.Length)
            {
                // We have reached the last cell but the hints are not completed,
                // making this permutation invalid
                return;
            }

            // Skip crossed (will never contribute to a hint, filled cells can contribute if they are the start of a hint)
            if (permutation[cellIdx] == CellType.CROSS)
            {
                PlaceHintBlocks(permutation, hints, hintIdx, cellIdx + 1, found);
                return; 
            }


            // Simple first check to see if the total of remaining hints can be satisfied
            //int totalCellsNeededForRemainingHints = hints.Skip(hintIdx).Select(hint => hint.Number).Sum();
            //if (totalCellsNeededForRemainingHints > permutation.Length - cellIdx)
            //{
            //    return;
            //}

            if (IsValidPlacement(permutation, hints[hintIdx], cellIdx))
            {
                int hintsNum = hints[hintIdx].Number;
                
                // Cells that need to be emptied again after this iteration
                LinkedList<int> needToBeEmptied = new();
                for (int i = 0; i < hintsNum; i++)
                {
                    if (permutation[cellIdx + i] == CellType.BLANK)
                    {
                        permutation[cellIdx + i] = CellType.FILLED;
                        needToBeEmptied.AddLast(cellIdx + i);
                    }
                }

                // Place cross since space between hints is required to be empty
                if (cellIdx + hintsNum < permutation.Length && permutation[cellIdx + hintsNum] == CellType.BLANK)
                {
                    permutation[cellIdx + hintsNum] = CellType.CROSS;
                    needToBeEmptied.AddLast(cellIdx + hintsNum);
                }

                PlaceHintBlocks(permutation, hints, hintIdx + 1, cellIdx + hintsNum, found);
                
                // Undo filled cells
                foreach (int cellToBeEmptied in needToBeEmptied)
                {
                    permutation[cellToBeEmptied] = CellType.BLANK;
                }
            }

            // A cell can be crossed as well, without any requirements
            if (permutation[cellIdx] == CellType.BLANK)
            {
                permutation[cellIdx] = CellType.CROSS;
                PlaceHintBlocks(permutation, hints, hintIdx, cellIdx + 1, found);
                permutation[cellIdx] = CellType.BLANK;
            }    
        }

        /// <summary>
        /// Determines if a full group of filled cells, with a total length of <paramref name="nextHint"/>.Number
        /// can be placed into the permutation without issue
        /// </summary>
        /// <param name="permutation">Permutation to work on</param>
        /// <param name="nextHint">Hint to consider</param>
        /// <param name="cellIdx">Current cell to check of permutation</param>
        /// <returns>True if possible, false otherwise</returns>
        private static bool IsValidPlacement(CellType[] permutation, Hint nextHint, int cellIdx)
        {
            int cellsToPlace = nextHint.Number;

            // Check if there is enough space left in the permutation
            if (cellIdx + cellsToPlace > permutation.Length)
            {
                return false;
            }

            if (cellIdx + cellsToPlace < permutation.Length)
            {
                // If the end of the hint is not located at the last square of the grid,
                // we need to check if the cell after filling the hint can be/is crossed
                // Else, the placement is invalid since it won't satisfy the hint restrictions
                if (permutation[cellIdx + cellsToPlace] == CellType.FILLED)
                {
                    return false;
                }

            }
            // If cellsIdx + cellsToPlace == permutation.Length, then the hint ends at the last cell,
            // no need for an extra check

            // Finally, check if all cells can be/are already placed
            while (cellsToPlace > 0)
            {
                // Check if this cell is crossed. Filled cells can be "skipped" and empty cells filled,
                // but crossed cells must remain crossed
                if (permutation[cellIdx] == CellType.CROSS)
                {
                    return false;
                }
                cellsToPlace--;
                cellIdx++;
            }
            return true;
        }

        private static bool IsHintAlreadyCompleted(CellType[] permutation, Hint hint, int cellIdx)
        {
            int nextNonFilledCellIdx = Array.FindIndex(permutation, cellIdx, c => c != CellType.FILLED);
            int totalConsecutiveFilled = nextNonFilledCellIdx - cellIdx;

            return totalConsecutiveFilled == hint.Number;
        }

        public bool IsValidPermutation(CellType[] permutation, Hints hints)
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
        /// <summary>
        /// Tries to improve <paramref name="line"/> by determining which cells must be crosses or filled.
        /// Will modify <paramref name="line"/> by setting the celltypes after improving. Skips non-empty cells.
        /// </summary>
        /// <param name="line">The line to improve. Improved cells will be changed in the array.</param>
        /// <param name="hints">The hints to base the improvement on</param>
        /// <returns>True if any cells were changed, false otherwise</returns>
        internal bool ImproveLine(CellType[] line, Hints hints)
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

    internal class UniqueQueueStrategy : SolverHelperMethods, ISolveGridStrategy
    {
        private UniqueQueue<(bool, int)> queue;

        internal UniqueQueueStrategy()
        {
            queue = [];
        }

        public void SolveGrid(Grid grid)
        {
            for (int i = 0; i < grid.Width; i++)
            {
                queue.Enqueue((true, i));
            }

            for (int j = 0; j < grid.Height; j++)
            {
                queue.Enqueue((false, j));
            }

            HandleQueue(grid);
        }

        /// <summary>
        /// While there are elements in the queue, calls <see cref="DoIteration(Grid, int, bool)"/> on the next
        /// element of the queue
        /// </summary>
        /// <param name="grid">Grid to work with</param>
        private void HandleQueue(Grid grid)
        {
            while (queue.Count > 0)
            {
                (bool inColumn, int index) = queue.Dequeue();
                DoIteration(grid, index, inColumn);
            }
        }

        /// <summary>
        /// Does a solve iteration. Changed cells are enqueued to the unique queue. 
        /// </summary>
        /// <param name="grid">Grid to work on</param>
        /// <param name="idx">Index of row/column</param>
        /// <param name="inColumn">Whether this iteration is in a column or not</param>
        private void DoIteration(Grid grid, int idx, bool inColumn)
        {
            var line = inColumn ? grid.GetColumnArray(idx) : grid.GetRowArray(idx);
            var hint = inColumn ? grid.ColumnHints[idx] : grid.RowHints[idx];

            List<CellType[]> perms = [];
            ComputePermutations(line, hint, 0, perms);

            if (perms.Count == 0)
            {
                return; // No valid moves left
            }

            for (int lineIndex = 0; lineIndex < line.Length; lineIndex++)
            {
                if (line[lineIndex] != CellType.BLANK)
                {
                    // This cell was already filled in a previous iteration and should not be updated
                    // to avoid infinite loops
                    continue;
                }
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
