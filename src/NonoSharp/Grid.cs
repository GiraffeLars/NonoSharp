using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace NonoSharp
{
    internal class Grid : ICloneable
    {
        private readonly CellType[,] grid;
        internal HashSet<CellPosition> Solution {get; private set; }
        private int filled = 0;
        private int paddingString = 0;

        public int Width { get; }
        public int Height { get; }

        public Hints[] ColumnHints { get; }
        public Hints[] RowHints { get; }

        /// <summary>
        /// Constructs a Grid.
        /// </summary>
        /// <param name="width">Width of the grid</param>
        /// <param name="height">Height of the grid</param>
        /// <param name="solution">The solution for the grid</param>
        /// <exception cref="ArgumentException">Thrown when width or height are non-positive</exception>
        public Grid(int width, int height, HashSet<CellPosition> solution)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(width, 0);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(height, 0);
            ArgumentNullException.ThrowIfNull(solution);
            
            this.Width = width;
            this.Height = height;

            grid = new CellType[width, height];

            ColumnHints = new Hints[width];
            RowHints = new Hints[height];

            SetSolution(solution);
        }

        /// <summary>
        /// Creates a <paramref name="width"/>×<paramref name="height"/> grid with an empty solution.
        /// </summary>
        /// <param name="width">Width of the grid</param>
        /// <param name="height">Height of the grid</param>
        public Grid(int width, int height) : this(width, height, []) { }

        /// <summary>
        /// Creates a full custom Grid
        /// </summary>
        /// <param name="grid">List of CellType, with data of filled in cells, etc.</param>
        /// <param name="solution">Solution to this grid</param>
        /// <param name="filled">How many cells are filled in. Should be consistent with <paramref name="grid"/>.</param>
        /// <param name="paddingString">The padding used to pad out the hints when converting to string</param>
        /// <param name="width">Width of the grid. Should be consistent with <paramref name="grid"/></param>
        /// <param name="height">Height of the grid. Should be consistent with <paramref name="grid"/></param>
        internal Grid(CellType[,] grid, HashSet<CellPosition> solution, int filled, int paddingString, int width, int height)
        {
            this.grid = grid;
            this.filled = filled;
            this.paddingString = paddingString;
            Width = width;
            Height = height;
            ColumnHints = new Hints[width];
            RowHints = new Hints[height];
            SetSolution(solution);
        }



        /// <summary>
        /// Makes the solution into a 2D array representation, just as <c>grid</c>
        /// </summary>
        /// <returns>2D array of <c>CellType</c> where each cell position in the solution is <c>CellType.FILLED</c></returns>
        private CellType[,] GridifySolution()
        {
            CellType[,] s = new CellType[Width, Height];

            foreach (CellPosition p in Solution)
            {
                s[p.X, p.Y] = CellType.FILLED;
            }

            return s;
        }

        /// <summary>
        /// Sets the solution to a Grid and sets the hints.
        /// </summary>
        /// <param name="solution"></param>
        [MemberNotNull(nameof(Solution))]
        internal void SetSolution(HashSet<CellPosition> solution)
        { 
            this.Solution = solution;
            InitializeHints();
        }

        private void InitializeHints()
            
        {
            for (int i = 0; i < Width; i++)
            {
                ColumnHints[i] = new Hints(true, i);
            }
            for (int i = 0; i < Height; i++)
            {
                RowHints[i] = new Hints(false, i);
            }
            SetHints(ColumnHints, true);
            SetHints(RowHints, false);
            
        }

        /// <summary>
        /// Creates the hints for the solution of this grid.
        /// </summary>
        /// <param name="hints">Which hints to set, either <c>ColumnHints</c> or <c>RowHints</c></param>
        /// <param name="isColumn">Whether we are setting the ColumnHints, corresponding to the <paramref name="hints"/> parameter</param>
        private void SetHints(Hints[] hints, bool isColumn)
        {
            // Sets the hint limits based on whether we process the column hints
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
                    if (cell != CellType.FILLED)
                    {
                        // Add the new hint to the list
                        AddHint(hints, x, count); // TODO if cells are split (i.e. empty between two patches), separate them with a 0
                        count = 0;
                        continue;
                    }

                    count++;
                }

                // Do final hint adding in case the last cell is filled
                // Count minus 1 as it is increased by one even if unfilled
                CellType lastCell = isColumn ? gridSol[x, yLimit - 1] : gridSol[yLimit - 1, x];
                if (count > 0 && lastCell == CellType.FILLED)
                {
                    AddHint(hints, x, count);
                }
                else if (hints[x].Count == 0)
                {
                    hints[x].Add(new Hint(0));
                }

                DoRowPaddingCount(hints[x].Count, isColumn);
            }
        }

        private void AddHint(Hints[] hints, int pos, int count)
        {
            if (count > 0)
            {
                hints[pos].Add(new Hint(count));
            }
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
        /// Sets the specified cell at (<paramref name="x"/>, <paramref name="y"/>) to <paramref name="value"/>
        /// </summary>
        /// <param name="x">x-coordinate of cell to set</param>
        /// <param name="y">y-coordinate of cell to set</param>
        /// <param name="value">New value of cell</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when either x or y is out of bounds</exception>
        public void SetCell(int x, int y, CellType value)
        {
            ValidateInputCoordinates(x, y);

            if (grid[x, y] != CellType.FILLED && value == CellType.FILLED)
            {
                filled++; // Keeps track of whether the same amount of cells are filled as the solution for efficiency
            }
            else if (grid[x, y] == CellType.FILLED && value != CellType.FILLED)
            {
                filled--;
            }

            grid[x, y] = value;

            // TODO change hints from ints to using Hints and Hint classes then change completion here
            RowHints[y].DoCompletion(this);
            ColumnHints[x].DoCompletion(this);
        }

        /// <summary>
        /// Gets the specified cell of this grid
        /// </summary>
        /// <param name="x">x-coordinate of the cell</param>
        /// <param name="y">y-coordinate of the cell</param>
        /// <returns><c>CellType</c> of the requested cell</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when either x or y is out of bounds</exception>
        public CellType GetCell(int x, int y)
        {
            ValidateInputCoordinates(x, y);

            return grid[x, y];
        }

        /// <summary>
        /// Validates whether x and y are within the grid bounds
        /// </summary>
        /// <param name="x">x input to check</param>
        /// <param name="y">y input to check</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when either x or y is out of bounds</exception>
        private void ValidateInputCoordinates(int x, int y)
        {
            if (x < 0 || x >= Width)
            {
                throw new ArgumentOutOfRangeException(nameof(x), x, $"x must be between 0 and {Width - 1}!");
            }

            if (y < 0 || y >= Height)
            {
                throw new ArgumentOutOfRangeException(nameof(y), y, $"y must be between 0 and {Height - 1}!");
            }
        }

        /// <summary>
        /// Get the current column data as a LinkedList. For an array representation, see <seealso cref="GetColumnArray(int)"/>.
        /// </summary>
        /// <param name="column">The column of the board to get. Must be between 0 and Width</param>
        /// <returns>LinkedList of cells and their associated CellType data</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <c>column</c> is not a valid column, i.e. out of range of the grid width.</exception>
        internal LinkedList<CellType> GetColumn(int column)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(column);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(column, Width);

            LinkedList<CellType> list = new LinkedList<CellType>();

            for (int i = 0; i < Height; i++)
            {
                CellType type = grid[column, i];
                list.AddLast(type);
            }
            return list;
        }

        /// <summary>
        /// Get the current column data as an array. For an LinkedList representation, see <seealso cref="GetColumn(int)"/>.
        /// </summary>
        /// <param name="column">The column of the board to get. Must be between 0 and Width</param>
        /// <returns>Array of CellType corresponding to the cells in the column</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <c>column</c> is not a valid column, i.e. out of range of the grid width.</exception>
        internal CellType[] GetColumnArray(int column)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(column);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(column, Width);

            CellType[] cells = new CellType[Height];

            for (int i = 0; i < Height; i++)
            {
                CellType type = grid[column, i];
                cells[i] = type;
            }
            return cells;
        }

        /// <summary>
        /// Get the current row data as a LinkedList. For an array representation, see <seealso cref="GetRowArray(int)"/>.
        /// </summary>
        /// <param name="row">The row of the board to get. Must be between 0 and Height</param>
        /// <returns>LinkedList of cells and their associated CellType data</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <c>row</c> is not a valid row, i.e. out of range of the grid height.</exception>
        internal LinkedList<CellType> GetRow(int row)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(row);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(row, Height);

            LinkedList<CellType> list = new LinkedList<CellType>();

            for (int i = 0; i < Width; i++)
            {
                CellType type = grid[i, row];
                list.AddLast(type);
            }
            return list;
        }

        /// <summary>
        /// Get the current row data as an array. For an LinkedList representation, see <seealso cref="GetRow(int)"/>.
        /// </summary>
        /// <param name="row">The row of the board to get. Must be between 0 and Height</param>
        /// <returns>Array of CellType corresponding to the cells in the row</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <c>row</c> is not a valid row, i.e. out of range of the grid height.</exception>
        internal CellType[] GetRowArray(int row) 
        {
            ArgumentOutOfRangeException.ThrowIfNegative(row);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(row, Height);

            CellType[] cells = new CellType[Width];

            for (int i = 0; i < Width; i++)
            {
                CellType type = grid[i, row];
                cells[i] = type;
            }
            return cells;
        }

        /// <summary>
        /// Sets the specified <paramref name="row"/> to <paramref name="newRow"/>
        /// </summary>
        /// <param name="row">The row to change</param>
        /// <param name="newRow">New row</param>
        /// <exception cref="ArgumentException">Thrown when dimensions of <c>newRow</c> do not match the grid</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <c>row</c> is not a valid row</exception>
        internal void SetRow(int row, CellType[] newRow)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(row);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(row, Height);
            
            if (newRow.Length != Width)
            {
                throw new ArgumentException("newRow must have the match the dimension of the grid!");
            }

            for (int i = 0; i < Width; i++)
            {
                if (newRow[i] != GetCell(i, row))
                {
                    SetCell(i, row, newRow[i]);
                }
            }
        }

        internal void SetColumn(int column, CellType[] newColumn)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(column);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(column, Width);

            if (newColumn.Length != Height)
            {
                throw new ArgumentException("newRow must have the match the dimension of the grid!");
            }

            for (int i = 0; i < Height; i++)
            {
                if (newColumn[i] != GetCell(column, i))
                {
                    SetCell(column, i, newColumn[i]);
                }
            }
        }

        /// <summary>
        /// Determines and returns the groups in <paramref name="line"/>. A group is a collection of consecutive filled in cells.  
        /// </summary>
        /// <param name="line">The line to determine groups from</param>
        /// <returns>A <c>LinkedList of int</c> where each entry is a separate group and each value is the total number of cells filled in this group</returns>
        private static LinkedList<int> GetGroups(CellType[] line)
        {
            LinkedList<int> groups = new();
            int groupSize = 0;

            foreach (CellType cell in line)
            {
                if (groupSize == 0 && cell != CellType.FILLED)
                {
                    continue;
                }
                else if (cell != CellType.FILLED)
                {
                    // groupSize > 0, group ends here
                    groups.AddLast(groupSize);
                    groupSize = 0;
                }
                else
                {
                    // cell == FILLED
                    groupSize++;
                }
            }

            // Check if we ended on a filled cell in which case
            // groupSize > 0 and thus still needs to be added
            if (groupSize > 0)
            {
                groups.AddLast(groupSize);
            }

            return groups;
        }

        /// <summary>
        /// Returns a linked list of each group present in row <paramref name="row"/> represented by a <c>LinkedList</c> of <c>int</c>s, where each entry is a separate group
        /// and each value is the total number of cells filled in this group. A group is a collection of consecutive filled in cells. See also <seealso cref="GetGroupsInColumn(int)"/>.
        /// </summary>
        /// <param name="row">The row in the grid to get the rows from</param>
        /// <returns>A LinkedList as described above</returns>
        internal LinkedList<int> GetGroupsInRow(int row)
        {
            CellType[] line = GetRowArray(row);
            return GetGroups(line);
        }

        /// <summary>
        /// Returns a linked list of each group present in column <paramref name="col"/> represented by a <c>LinkedList</c> of <c>int</c>s, where each entry is a separate group
        /// and each value is the total number of cells filled in this group. A group is a collection of consecutive filled in cells. See also <seealso cref="GetGroupsInRow(int)"/>.
        /// </summary>
        /// <param name="col">The column in the grid to get the rows from</param>
        /// <returns>A LinkedList as described above</returns>
        internal LinkedList<int> GetGroupsInColumn(int col)
        {
            CellType[] line = GetColumnArray(col);
            return GetGroups(line);
        }

        public bool IsSolved()
        {
            if (filled != Solution.Count())
            {
                return false; 
            }

            foreach (CellPosition p in Solution)
            {
                if (grid[p.X, p.Y] != CellType.FILLED)
                {
                    return false;
                }
            }

            return true;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            String[] rowHintsStr = CreateRowHintsString();

            sb.Append(CreateColumnHintsString());
            //sb.AppendLine();

            for (int y = 0; y < Height; y++)
            {
                sb.Append('\n');
                sb.Append(rowHintsStr[y]);
                for (int x = 0; x < Width; x++)
                {
                    char c = ' ';
                    switch (GetCell(x, y))
                    {
                        case CellType.FILLED:
                            c = 'O';
                            break;
                        case CellType.BLANK:
                            c = ' ';
                            break;
                        case CellType.CROSS:
                            c = 'X';
                            break;
                        
                    }
                    sb.Append($"[{c}]");
                }
                
            }
            return sb.ToString();
        }

        private String CreateColumnHintsString()
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
                    Hints hints = ColumnHints[x];
                    if (hints.Count > y)
                    {
                        // Add spaces for all columns with no hints until this column
                        sb.Append(new string(' ', (x - lastFilled) * 3));
                        sb.Append($" {hints[y].Number} ");
                        
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

        private String[] CreateRowHintsString()
        {
            String[] hintsStr = new String[Height];

            for (int i = 0; i < Height; i++)
            {
                int x;
                StringBuilder sb = new StringBuilder();
                Hints hints = RowHints[i];

                for (x = 0; x < hints.Count; x++)
                {
                    sb.Append($"{hints[x].Number} ");
                }

                sb.Append(new string(' ', 2 * Math.Max(paddingString - x, 0)));
                hintsStr[i] = sb.ToString();
            }

            return hintsStr;
        }

        /// <summary>
        /// Deep copies <paramref name="toClone"/>
        /// </summary>
        /// <param name="toClone">The hints array to clone</param>
        /// <returns>Deep copy of <c>toClone</c></returns>
        private Hints[] CloneHints(Hints[] toClone)
        {
            Hints[] clone = new Hints[toClone.Length];

            for (int i = 0; i < toClone.Length; i++)
            {
                clone[i] = (Hints) toClone[i].Clone();
            }

            return clone;
        }

        public object Clone()
        {
            return new Grid(
                (CellType[,]) grid.Clone(),
                Solution,
                filled,
                paddingString,
                Width,
                Height
            );
        }
    }


}
