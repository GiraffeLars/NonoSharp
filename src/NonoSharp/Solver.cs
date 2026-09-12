using NonoSharp.Collections;

namespace NonoSharp
{
    /// <summary>
    /// A static class to solve Nonogram puzzles, or determine if they can be solved.
    /// </summary>
    /// <remarks>
    /// The Solver will not make any guesses, meaning that puzzles with a valid and unique
    /// solution that might require more advanced logical deduction, 
    /// or guesses in any other form, might be rejected.
    /// Puzzles without one unique answer, are rejected.<br/>
    /// Note that the runtime of solving a puzzle grows exponentially with its size.
    /// For a quick runtime, keep puzzles small (~25x25). There are, however, no restrictions on the methods 
    /// regarding puzzle size.
    /// </remarks>
    public static class Solver
    {
        /// <summary>
        /// Solves the puzzle of API instance <paramref name="nonogram"/> in-place.
        /// </summary>
        /// <remarks>
        /// If the given puzzle is not solvable, the returned solution will be incomplete.
        /// To ensure a complete solution, use <see cref="IsSolvable(NonogramAPI, out HashSet{CellPosition}?)"/>.
        /// </remarks>
        /// <param name="nonogram">Puzzle to solve.</param>
        /// <returns>The solution in a HashSet of <see cref="CellPosition"/>s.</returns>
        public static HashSet<CellPosition> Solve(NonogramAPI nonogram)
        {
            return Solve(nonogram.grid);
        }

        /// <summary>
        /// Solves the grid <paramref name="grid"/> in-place.
        /// </summary>
        /// <param name="grid">Grid to solve.</param>
        /// <returns>The solution in a HashSet of <see cref="CellPosition"/>s.</returns>
        internal static HashSet<CellPosition> Solve(Grid grid)
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

            var solution = HandleQueue(queue, grid);
            return solution;
        }

        /// <summary>
        /// Determines whether a puzzle can be solved.
        /// </summary>
        /// <returns><c>true</c> if the puzzle can be solved, <c>false</c> otherwise.</returns>t
        internal static bool IsSolvable(Grid grid, out HashSet<CellPosition>? solution)
        {
            // Grid to work on to calculate solutions (Copy of grid).
            Grid workingGrid = (Grid) grid.Clone();
            solution = Solve(workingGrid);

            // At the end of all iterations, check if the puzzle is solved.
            // The loop stops either if the puzzle is solved and no lines could be improved, or if the puzzle was not solved
            // and no cells could be filled with certainty.
            bool solvable = workingGrid.IsSolved();

            if (!solvable)
            {
                solution = null;
            }
            return solvable;
        }

        /// <inheritdoc cref="IsSolvable(Grid, out HashSet{CellPosition}?)"/>
        internal static bool IsSolvable(Grid grid)
        {
            return IsSolvable(grid, out _);
        }

        /// <inheritdoc cref="IsSolvable(NonogramAPI, out HashSet{CellPosition}?)"/>
        public static bool IsSolvable(NonogramAPI nonogram)
        {
            return IsSolvable(nonogram.grid);
        }

        /// <summary>
        /// Determines whether the puzzle in <paramref name="nonogram"/> can be solved.
        /// </summary>
        /// <param name="nonogram">The <c>NonogramAPI</c> instance to check solvability for.</param>
        /// <param name="solution">The solution HashSet if <paramref name="nonogram"/> is solvable.
        /// <c>null</c> if the puzzle is not solvable.</param>
        /// <returns><c>true</c> if the puzzle can be solved, <c>false</c> otherwise.</returns>
        public static bool IsSolvable(NonogramAPI nonogram, out HashSet<CellPosition>? solution)
        {
            return IsSolvable(nonogram.grid, out solution);
        }

        /// <summary>
        /// While there are elements in <paramref name="queue"/>, does a solve iteration.
        /// Cells changed after the line improvement iteration are enqueued as the different direction
        /// </summary>
        /// <param name="queue">Queue to clear.</param>
        /// <param name="grid">Grid to work with.</param>
        /// <returns>The HashSet containing the solution after the queue has been cleared.</returns>
        private static HashSet<CellPosition> HandleQueue(UniqueQueue<(bool, int)> queue, Grid grid)
        {
            // Allocate changed list beforehand and keep reusing it, instead of building a new one each time
            // as enlarging the list is expensive.
            List<int> changed = [];
            HashSet<CellPosition> solution = [];
            while (queue.Count > 0)
            {
                changed.Clear();
                (bool inColumn, int index) = queue.Dequeue();

                CellType[] line = inColumn ? grid.GetColumnArray(index) : grid.GetRowArray(index);
                Clues clues = inColumn ? grid.ColumnClues[index] : grid.RowClues[index];

                
                ImproveLine(line, clues, changed);
                foreach (int i in changed)
                {
                    CellPosition changedPos = inColumn ? new(index, i) : new(i, index);
                    
                    grid.SetCell(changedPos.X, changedPos.Y, line[i]);

                    if (line[i] == CellType.FILLED)
                    {
                        // Only append to the solution if the changed cell has been changed to FILLED
                        solution.Add(changedPos);
                    }
                    
                    queue.Enqueue((!inColumn, i));
                }
            }

            return solution;
        }

        /// <summary>
        /// Does a solve iteration. Changed cells in the line are added to the <paramref name="changedIndices"/> list. 
        /// </summary>
        /// <param name="line">The line to improve in-place.</param>
        /// <param name="clues">Clues associated with <paramref name="line"/>.</param>
        /// <param name="changedIndices">The List to append indices of the cells that are changed to.</param>
        internal static void ImproveLine(CellType[] line, Clues clues, List<int> changedIndices)
        { 
            List<CellType[]> perms = [];
            ComputePermutations(line, clues, perms);

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
                    // Since in all possible permutations the cell was set as baseCellType, this can be
                    // safely filled in
                    line[lineIndex] = baseCellType;
                    // Add the changed index to the list
                    changedIndices.Add(lineIndex);
                }
            }
        }
    

        /// <summary>
        /// Gets all possible permutations of <paramref name="line"/> based on <paramref name="clues"/>, 
        /// all non-empty cells remain as they were.
        /// </summary>
        /// <param name="line">Array of CellType to compute all possible permutations of, filling/crossing only blank cells.</param>
        /// <param name="clues">Clues instance corresponding to <paramref name="line"/>.</param>
        /// <param name="currentlyFound">The currently found valid permutations according to <paramref name="clues"/>
        /// and already non-empty cells. This List will be modified by adding the found permutations.</param>
        private static void ComputePermutations(CellType[] line, Clues clues, List<CellType[]> currentlyFound)
        {
            PlaceClueBlocks(line, clues, 0, 0, currentlyFound);
        }

        /// <summary>
        /// Places filled cells to satisfy all clues, if possible.
        /// </summary>
        /// <param name="permutation">The current permutation to work on.</param>
        /// <param name="clues">Clues associated with the permutation.</param>
        /// <param name="clueIdx">Index of the next clue that needs to be satisfied. Should initially be 0.</param>
        /// <param name="cellIdx">Index of the next cell that needs to be determined if it can satisfy a cell.
        /// Should initially be 0.</param>
        /// <param name="found">List of permutations currently found that are able to satisfy all clues.</param>
        private static void PlaceClueBlocks(CellType[] permutation, Clues clues, int clueIdx, int cellIdx, List<CellType[]> found)
        {
            if (clueIdx >= clues.Count)
            {
                CellType[] clone = (CellType[])permutation.Clone();
                for (int i = cellIdx; i < permutation.Length; i++)
                {
                    // If there are more filled cells after the clues have been processed, there are too many
                    // filled cells in the line. As such, this pernutation is invalid
                    if (clone[i] == CellType.FILLED)
                    {
                        return;
                    }

                    // Cross out the cell otherwise, as all clues should have been satisfied
                    clone[i] = CellType.CROSS;
                }

                found.Add(clone);
                return;
            }

            if (cellIdx >= permutation.Length)
            {
                // We have reached the last cell but the clues are not completed,
                // making this permutation invalid
                return;
            }

            // Skip crossed (will never contribute to a clue, filled cells can contribute if they are the start of a clue)
            if (permutation[cellIdx] == CellType.CROSS)
            {
                PlaceClueBlocks(permutation, clues, clueIdx, cellIdx + 1, found);
                return;
            }


            // Simple first check to see if the total of remaining clues can be satisfied
            int totalCellsNeededForRemainingClues = clues.Skip(clueIdx).Select(clue => clue.Number).Sum();
            if (totalCellsNeededForRemainingClues > permutation.Length - cellIdx)
            {
                return;
            }

            if (IsValidPlacement(permutation, clues[clueIdx], cellIdx))
            {
                int cluesNum = clues[clueIdx].Number;

                // Cells that need to be emptied again after this iteration
                LinkedList<int> needToBeEmptied = new();
                for (int i = 0; i < cluesNum; i++)
                {
                    if (permutation[cellIdx + i] == CellType.BLANK)
                    {
                        permutation[cellIdx + i] = CellType.FILLED;
                        needToBeEmptied.AddLast(cellIdx + i);
                    }
                }

                // Place cross since space between clues is required to be empty
                if (cellIdx + cluesNum < permutation.Length && permutation[cellIdx + cluesNum] == CellType.BLANK)
                {
                    permutation[cellIdx + cluesNum] = CellType.CROSS;
                    needToBeEmptied.AddLast(cellIdx + cluesNum);
                }

                PlaceClueBlocks(permutation, clues, clueIdx + 1, cellIdx + cluesNum, found);

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
                PlaceClueBlocks(permutation, clues, clueIdx, cellIdx + 1, found);
                permutation[cellIdx] = CellType.BLANK;
            }
        }

        /// <summary>
        /// Determines if a full group of filled cells, with a total length of <c><paramref name="nextClue"/>.Number</c>
        /// can be placed into the permutation without issue
        /// </summary>
        /// <param name="permutation">Permutation to work on.</param>
        /// <param name="nextClue">Clue to consider.</param>
        /// <param name="cellIdx">Current cell to check of permutation.</param>
        /// <returns><c>true</c> if possible, <c>false</c> otherwise.</returns>
        private static bool IsValidPlacement(CellType[] permutation, Clue nextClue, int cellIdx)
        {
            int cellsToPlace = nextClue.Number;

            // Check if there is enough space left in the permutation
            if (cellIdx + cellsToPlace > permutation.Length)
            {
                return false;
            }

            if (cellIdx + cellsToPlace < permutation.Length)
            {
                // If the end of the clue is not located at the last square of the grid,
                // we need to check if the cell after filling the clue can be/is crossed
                // Else, the placement is invalid since it won't satisfy the clue restrictions
                if (permutation[cellIdx + cellsToPlace] == CellType.FILLED)
                {
                    return false;
                }

            }
            // If cellsIdx + cellsToPlace == permutation.Length, then the clue ends at the last cell,
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
