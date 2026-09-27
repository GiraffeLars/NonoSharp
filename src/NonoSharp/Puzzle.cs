using NonoSharp.Events;
using System.Collections.Frozen;
using System.Text;

namespace NonoSharp
{
    internal class Puzzle : PuzzleDefinition, ICloneable
    {
        public PuzzleClues[] ColumnClues { get; }
        public PuzzleClues[] RowClues { get; }

        public Grid Grid { get; }
        private int paddingString = 0;

        /// <summary>
        /// Constructs a Puzzle. Automatically sets the puzzle clues based on <paramref name="solution"/>.
        /// </summary>
        /// <param name="width">Width of the puzzle grid</param>
        /// <param name="height">Height of the puzzle grid</param>
        /// <param name="solution">The solution for the puzzle</param>
        /// <exception cref="ArgumentException">Thrown when width or height are non-positive</exception>
        /// <exception cref="IndexOutOfRangeException">Thrown when any of the CellPositions
        /// found in the solution is out of bounds.</exception>
        internal Puzzle(int width, int height, IEnumerable<CellPosition>? solution) : this(new(width, height), solution)
        { }

        /// <summary>
        /// Constructs a Puzzle with a pre-made <see cref="Grid"/>.
        /// </summary>
        /// <param name="grid">Grid for this puzzle</param>
        /// <param name="solution">Solution of this puzzle.</param>
        internal Puzzle(Grid grid, IEnumerable<CellPosition>? solution) : base(grid.Width, grid.Height, solution)
        {
            Grid = grid;
            ColumnClues = new PuzzleClues[grid.Width];
            RowClues = new PuzzleClues[grid.Height];
            SetSolution(solution);

            Grid.CellStateChanged += GridCellsChanged;
        }

        /// <summary>
        /// Constructs a Puzzle with already determined solution and clues.
        /// </summary>
        /// <remarks>
        /// <paramref name="solution"/> is not validated to be correct according to <paramref name="columnClues"/> and
        /// <paramref name="rowClues"/>.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when width or height are invalid dimensions.</exception>
        internal Puzzle(int width, int height, Clues[] columnClues, Clues[] rowClues,
            IEnumerable<CellPosition>? solution) :
            this(new(width, height), columnClues, rowClues, solution, 0)
        { }

        internal Puzzle(PuzzleDefinition definition) : base(definition.Width, definition.Height, definition.Solution, definition.Title)
        {
            Grid = new(definition.Width, definition.Height);

            ColumnClues = new PuzzleClues[definition.Width];
            RowClues = new PuzzleClues[definition.Height];
            SetSolution(definition.Solution);

            Grid.CellStateChanged += GridCellsChanged;
        }

        private Puzzle(Grid grid, Clues[] columnClues, Clues[] rowClues, IEnumerable<CellPosition>? solution,
            int paddingString) : base(grid.Width, grid.Height, solution)
        {
            Grid = grid;
            ColumnClues = PuzzleClues.ArrayFromClues(columnClues, true);
            RowClues = PuzzleClues.ArrayFromClues(rowClues, false);

            DoCompletionAllClues(ColumnClues);
            DoCompletionAllClues(RowClues);

            Grid.CellStateChanged += GridCellsChanged;
            this.paddingString = paddingString;
        }

        /// <summary>
        /// Sets the solution to the Puzzle and determines and sets the clues.
        /// </summary>
        /// <param name="solution">Solution for the puzzle</param>
        internal void SetSolution(IEnumerable<CellPosition>? solution)
        {
            Solution = solution?.ToFrozenSet();
            InitializeClues();
        }
        /// <summary>
        /// Makes the solution into a 2D array representation, just as <c>Grid</c>
        /// </summary>
        /// <returns>2D array of <c>CellType</c> where each cell position in the solution is <c>CellType.Filled</c></returns>
        /// <exception cref="IndexOutOfRangeException">Thrown when any of the CellPositions
        /// found in the solution is out of bounds.</exception>
        private CellType[,] GridifySolution()
        {
            CellType[,] s = new CellType[Width, Height];

            foreach (CellPosition p in Solution!)
            {
                s[p.X, p.Y] = CellType.Filled;
            }

            return s;
        }

        private void InitializeClues()

        {
            for (int i = 0; i < Width; i++)
            {
                ColumnClues[i] = new(true, i);
            }
            for (int i = 0; i < Height; i++)
            {
                RowClues[i] = new(false, i);
            }

            if (Solution != null)
            {
                SetClues(ColumnClues, true);
                SetClues(RowClues, false);
            }

        }

        /// <summary>
        /// Checks if the puzzle is solved. If <c>Solution</c> is set, this is based on
        /// <c>Solution</c>. Else, this is based on whether all clues are complete.
        /// </summary>
        /// <returns>True if the puzzle is solved, false otherwise.</returns>
        public bool IsSolved()
        {
            if (Solution != null)
            {
                // We know the solution, so check if the solution is fully complete and not
                // if all clues are satisfied as that is more expensive generally

                if (Grid.Filled != Solution.Count)
                {
                    return false;
                }

                foreach (CellPosition p in Solution)
                {
                    if (Grid[p.X, p.Y] != CellType.Filled)
                    {
                        return false;
                    }
                }

                return true;
            } else
            {
                return AreAllCluesSatisfied();
            }
        }

        /// <summary>
        /// Determines if the clues for all columns and rows are fully completed and satisfied,
        /// without too many cells being filled. In other words, this checks if the grid is solved
        /// by checking the clues.
        /// </summary>
        /// <remarks>
        /// If possible, check whether the grid is solved with <see cref="IsSolved"/> as that
        /// is computationally less expensive.
        /// </remarks>
        /// <returns></returns>
        public bool AreAllCluesSatisfied()
        {
            for (int col = 0; col < Width; col++)
            {
                CellType[] line = Grid.GetColumnArray(col);
                Clues clues = ColumnClues[col];

                bool allSatisfied = AreAllCluesSatisfied(line, clues);
                if (!allSatisfied) return false;
            }

            for (int row = 0; row < Height; row++)
            {
                CellType[] line = Grid.GetRowArray(row);
                Clues clues = RowClues[row];

                bool allSatisfied = AreAllCluesSatisfied(line, clues);
                if (!allSatisfied) return false;
            }

            return true;
        }

        private static bool AreAllCluesSatisfied(CellType[] line, Clues clues)
        {
            if (!clues.FullyCompleted) return false;

            // Check if there are not too many cells filled
            int filled = line.Where(c => c == CellType.Filled).Count();
            return filled == clues.TotalCellsInClues;
        }

        /// <summary>
        /// Creates the clues for the solution of this grid.
        /// </summary>
        /// <param name="clues">Which clues to set, either <c>ColumnClues</c> or <c>RowClues</c></param>
        /// <param name="isColumn">Whether we are setting the ColumnClues, corresponding to the <paramref name="clues"/> parameter</param>
        private void SetClues(Clues[] clues, bool isColumn)
        {
            // Sets the clue limits based on whether we process the column clues
            int xLimit = isColumn ? Width : Height;
            int yLimit = isColumn ? Height : Width;

            CellType[,] gridSol = GridifySolution();
            for (int x = 0; x < xLimit; x++)
            {
                int count = 0;
                for (int y = 0; y < yLimit; y++)
                {
                    CellType cell = isColumn ? gridSol[x, y] : gridSol[y, x];

                    // If this is not a filled cell
                    if (cell != CellType.Filled)
                    {
                        // Add the new clue to the list
                        AddClue(clues, x, count); 
                        count = 0;
                        continue;
                    }

                    count++;
                }

                // Do final clue adding in case the last cell is filled
                // Count minus 1 as it is increased by one even if unfilled
                CellType lastCell = isColumn ? gridSol[x, yLimit - 1] : gridSol[yLimit - 1, x];
                if (count > 0 && lastCell == CellType.Filled)
                {
                    AddClue(clues, x, count);
                }
                else if (clues[x].Count == 0)
                {
                    clues[x].Add(new Clue(0));
                }

                DoRowPaddingCount(clues[x].Count, isColumn);
            }
        }

        private static void AddClue(Clues[] clues, int pos, int count)
        {
            if (count > 0)
            {
                clues[pos].Add(new Clue(count));
            }
        }

        /// <summary>
        /// Determines for all Clues in <paramref name="clues"/> if clues and the contained Clue
        /// instances can be completed.
        /// </summary>
        private void DoCompletionAllClues(PuzzleClues[] clues)
        {
            foreach (PuzzleClues unpackedClues in clues)
            {
                unpackedClues.DoCompletion(Grid);
            }
        }

        private void GridCellsChanged(object? sender, CellStateEventArgs args)
        {
            // Slight optimisation to avoid creating sets, as SetCell is also used by Solver
            if (args.Cells.Count == 1)
            {
                CellPosition pos = args.Cells[0];
                RowClues[pos.Y].DoCompletion(Grid);
                ColumnClues[pos.X].DoCompletion(Grid);
                return;
            }

            HashSet<int> handledColumns = [];
            HashSet<int> handledRows = [];
            foreach (CellPosition pos in args.Cells)
            {
                if (handledColumns.Add(pos.X))
                {
                    ColumnClues[pos.X].DoCompletion(Grid);
                }

                if (handledRows.Add(pos.Y))
                {
                    RowClues[pos.Y].DoCompletion(Grid);
                }
            }   
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            String[] rowCluesStr = CreateRowCluesString();

            sb.Append(CreateColumnCluesString());
            //sb.AppendLine();

            for (int y = 0; y < Height; y++)
            {
                sb.Append('\n');
                sb.Append(rowCluesStr[y]);
                for (int x = 0; x < Width; x++)
                {
                    char c = ' ';
                    switch (Grid.GetCell(x, y))
                    {
                        case CellType.Filled:
                            c = 'O';
                            break;
                        case CellType.Empty:
                            c = ' ';
                            break;
                        case CellType.Cross:
                            c = 'X';
                            break;

                    }
                    sb.Append($"[{c}]");
                }

            }
            return sb.ToString();
        }

        private String CreateColumnCluesString()
        {
            StringBuilder sb = new StringBuilder();

            bool newStringRow;
            int lastFilled;
            for (int y = 0; y < Height; y++)
            {
                sb.Append(GetPadding());
                newStringRow = false;
                lastFilled = 0;
                for (int x = 0; x < Width; x++)
                {
                    Clues clues = ColumnClues[x];
                    if (clues.Count > y)
                    {
                        // Add spaces for all columns with no clues until this column
                        sb.Append(new string(' ', (x - lastFilled) * 3));
                        sb.Append($" {clues[y].Number} ");

                        newStringRow = true;
                        lastFilled = x + 1;
                    }
                }

                if (newStringRow)
                {
                    sb.AppendLine();
                    newStringRow = false;
                }
                lastFilled = 0;
            }
            return sb.ToString();
        }

        private String[] CreateRowCluesString()
        {
            String[] cluesStr = new String[Height];

            for (int i = 0; i < Height; i++)
            {
                int x;
                StringBuilder sb = new StringBuilder();
                Clues clues = RowClues[i];

                for (x = 0; x < clues.Count; x++)
                {
                    sb.Append($"{clues[x].Number} ");
                }

                sb.Append(new string(' ', 2 * Math.Max(paddingString - x, 0)));
                cluesStr[i] = sb.ToString();
            }

            return cluesStr;
        }

        private void DoRowPaddingCount(int count, bool isColumn)
        {
            if (!isColumn && count > paddingString)
            {
                paddingString = count;
            }
        }

        private String GetPadding()
        {
            return new string(' ', paddingString * 2);
        }

        /// <summary>
        /// Deep-clones this instance
        /// </summary>
        /// <returns>New Puzzle instance, deep-cloned from this instance</returns>
        public object Clone()
        {
            FrozenSet<CellPosition>? newSol = Solution != null ? [.. Solution] : null;
            return new Puzzle(
                (Grid)Grid.Clone(),
                ColumnClues,
                RowClues,
                newSol,
                paddingString
            );            
        }
    }
}
