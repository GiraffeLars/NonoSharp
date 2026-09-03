using NonoSharp.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NonoSharp
{
    /// <summary>
    /// A static class to solve Nonogram puzzles, or determine if they can be solved.
    /// The Solver will not make any guesses, meaning that puzzles with a valid
    /// solution that require more advanced logical deductions, 
    /// or guesses in any other form, to arrive at one unique solution, might be rejected.
    /// Puzzles without one unique answer, are rejected.
    /// </summary>
    public static class Solver
    {
        /// <summary>
        /// Solves the puzzle of API instance <paramref name="nonogram"/> in-place.
        /// </summary>
        /// <param name="nonogram">Puzzle to solve.</param>
        public static void Solve(NonogramAPI nonogram)
        {
            Solve(nonogram.grid);
        }

        /// <summary>
        /// Solves the grid <paramref name="grid"/> in-place.
        /// </summary>
        /// <param name="grid">Grid to solve.</param>
        internal static void Solve(Grid grid)
        {
            UniqueQueue<(bool, int)> queue = [];

            for (int i = 0; i < grid.Width; i++)
            {
                queue.Enqueue((true, i));
            }

            for (int j = 0; j < grid.Height; j++)
            {
                queue.Enqueue((false, j));
            }

            HandleQueue(queue, grid);
        }

        /// <summary>
        /// Determines whether a puzzle can be solved.
        /// </summary>
        /// <returns><c>true</c> if the puzzle can be solved, <c>false</c> otherwise.</returns>
        internal static bool IsSolvable(Grid grid)
        {
            // Grid to work on to calculate solutions (Copy of grid).
            Grid workingGrid = (Grid) grid.Clone();
            Solve(workingGrid);

            // At the end of all iterations, check if the puzzle is solved.
            // The loop stops either if the puzzle is solved and no lines could be improved, or if the puzzle was not solved
            // and no cells could be filled with certainty.
            return workingGrid.IsSolved();
        }

        /// <summary>
        /// Determines whether the puzzle in <paramref name="nonogram"/> can be solved.
        /// </summary>
        /// <returns><c>true</c> if the puzzle can be solved, <c>false</c> otherwise.</returns>
        public static bool IsSolvable(NonogramAPI nonogram)
        {
            return IsSolvable(nonogram.grid);
        }

        /// <summary>
        /// While there are elements in <paramref name="queue"/>, does a solve iteration.
        /// Cells changed after improving the line of each iteration are enqueued as the different direction
        /// </summary>
        /// <param name="queue">Queue to clear</param>
        /// <param name="grid">Grid to work with</param>
        private static void HandleQueue(UniqueQueue<(bool, int)> queue, Grid grid)
        {
            while (queue.Count > 0)
            {
                (bool inColumn, int index) = queue.Dequeue();

                LinkedList<int> changed = [];
                ImproveLine(grid, index, inColumn, changed);
                foreach (int i in changed)
                {
                    queue.Enqueue((!inColumn, i));
                }
            }
        }

        /// <summary>
        /// Does a solve iteration. Changed cells in the line corresponding to <paramref name="idx"/> and
        /// <paramref name="inColumn"/> are added to <paramref name="changedIndices"/>. 
        /// </summary>
        /// <param name="grid">Grid to work on</param>
        /// <param name="idx">Index of the column/row in the grid to solve</param>
        /// <param name="inColumn">Whether this is solving for a column or not</param>
        /// <param name="changedIndices">The LinkedList to append indices of the cells that are changed to</param>
        internal static void ImproveLine(Grid grid, int idx, bool inColumn, LinkedList<int> changedIndices)
        {
            var line = inColumn ? grid.GetColumnArray(idx) : grid.GetRowArray(idx);
            var hint = inColumn ? grid.ColumnHints[idx] : grid.RowHints[idx];

            List<CellType[]> perms = [];
            ComputePermutations(line, hint, perms);

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

                    // Add the changed index to the list
                    changedIndices.AddLast(lineIndex);
                }
            }
        }
    

        /// <summary>
        /// Gets all possible permutations of <paramref name="line"/> based on <paramref name="hints"/>, 
        /// all non-empty cells remain as they were.
        /// </summary>
        /// <param name="line">Array of CellType to compute all possible permutations of, filling/crossing only blank cells.</param>
        /// <param name="hints">Hints instance corresponding to <paramref name="line"/>.</param>
        /// <param name="currentlyFound">The currently found valid permutations according to <paramref name="hints"/>
        /// and already non-empty cells. This List will be modified by adding the found permutations.</param>
        private static void ComputePermutations(CellType[] line, Hints hints, List<CellType[]> currentlyFound)
        {
            PlaceHintBlocks(line, hints, 0, 0, currentlyFound);
        }

        /// <summary>
        /// Places filled cells to satisfy all hints, if possible.
        /// </summary>
        /// <param name="permutation">The current permutation to work on.</param>
        /// <param name="hints">Hints associated with the permutation.</param>
        /// <param name="hintIdx">Index of the next hint that needs to be satisfied. Should initially be 0.</param>
        /// <param name="cellIdx">Index of the next cell that needs to be determined if it can satisfy a cell.
        /// Should initially be 0.</param>
        /// <param name="found">List of permutations currently found that are able to satisfy all hints.</param>
        private static void PlaceHintBlocks(CellType[] permutation, Hints hints, int hintIdx, int cellIdx, List<CellType[]> found)
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
            int totalCellsNeededForRemainingHints = hints.Skip(hintIdx).Select(hint => hint.Number).Sum();
            if (totalCellsNeededForRemainingHints > permutation.Length - cellIdx)
            {
                return;
            }

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
    }
}
