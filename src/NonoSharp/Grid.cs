using NonoSharp.Events;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace NonoSharp
{
    internal class Grid : ICloneable
    {
        private readonly CellType[,] grid;
        public int Width { get { return grid.GetLength(0); } }
        public int Height { get { return grid.GetLength(1); } }

        /// <summary>
        /// Amount of cells in the grid that are CellType.FILLED
        /// </summary>
        public int Filled { get; private set; } = 0;

        // Events
        /// <summary>
        /// <c>CellStateChanged</c> is raised when one or more cell change to a new state.
        /// </summary>
        public event EventHandler<CellStateEventArgs>? CellStateChanged;

        /// <summary>
        /// Constructs a Grid.
        /// </summary>
        /// <param name="width">Width of the grid</param>
        /// <param name="height">Height of the grid</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when width or height are non-positive</exception>
        public Grid(int width, int height)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(width, 0);
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(height, 0);

            grid = new CellType[width, height];
        }

        private Grid(CellType[,] grid)
        {
            this.grid = grid;
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
                Filled++; // Keeps track of whether the same amount of cells are Filled as the solution for efficiency
            }
            else if (grid[x, y] == CellType.FILLED && value != CellType.FILLED)
            {
                Filled--;
            }

            grid[x, y] = value;
            OnCellStateChanged(new([new(x, y)]));
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

            List<CellPosition> changed = [];
            for (int i = 0; i < Width; i++)
            {
                if (newRow[i] != GetCell(i, row))
                {
                    grid[i, row] = newRow[i];
                    changed.Add(new(i, row));
                }
            }
            OnCellStateChanged(new(changed));
        }

        internal void SetColumn(int column, CellType[] newColumn)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(column);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(column, Width);

            if (newColumn.Length != Height)
            {
                throw new ArgumentException("newRow must have the match the dimension of the grid!");
            }

            List<CellPosition> changed = [];
            for (int i = 0; i < Height; i++)
            {
                if (newColumn[i] != GetCell(column, i))
                {
                    grid[column, i] = newColumn[i];
                    changed.Add(new(column, i));
                }
            }
            OnCellStateChanged(new(changed));
        }

        /// <summary>
        /// Determines and returns the groups in <paramref name="line"/>. A group is a collection of consecutive Filled in cells.  
        /// </summary>
        /// <param name="line">The line to determine groups from</param>
        /// <returns>A <c>LinkedList of int</c> where each entry is a separate group and each value is the total number of cells Filled in this group</returns>
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

            // Check if we ended on a Filled cell in which case
            // groupSize > 0 and thus still needs to be added
            if (groupSize > 0)
            {
                groups.AddLast(groupSize);
            }

            return groups;
        }

        /// <summary>
        /// Returns a linked list of each group present in row <paramref name="row"/> represented by a <c>LinkedList</c> of <c>int</c>s, where each entry is a separate group
        /// and each value is the total number of cells Filled in this group. A group is a collection of consecutive Filled in cells. See also <seealso cref="GetGroupsInColumn(int)"/>.
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
        /// and each value is the total number of cells Filled in this group. A group is a collection of consecutive Filled in cells. See also <seealso cref="GetGroupsInRow(int)"/>.
        /// </summary>
        /// <param name="col">The column in the grid to get the rows from</param>
        /// <returns>A LinkedList as described above</returns>
        internal LinkedList<int> GetGroupsInColumn(int col)
        {
            CellType[] line = GetColumnArray(col);
            return GetGroups(line);
        }

        /// <summary>
        /// Deep copies <paramref name="toClone"/>
        /// </summary>
        /// <param name="toClone">The clues array to clone</param>
        /// <returns>Deep copy of <c>toClone</c></returns>
        private Clues[] CloneClues(Clues[] toClone)
        {
            Clues[] clone = new Clues[toClone.Length];

            for (int i = 0; i < toClone.Length; i++)
            {
                clone[i] = (Clues) toClone[i].Clone();
            }

            return clone;
        }

        /// <summary>
        /// Gets/sets the value of the cell at (<paramref name="x"/>, <paramref name="y"/>).
        /// The parameters are unchecked.
        /// </summary>
        public CellType this[int x, int y]
        {
            get
            {
                return grid[x, y];
            }
            set
            {
                grid[x, y] = value;
                OnCellStateChanged(new([new(x, y)]));
            }
        }

        public object Clone()
        {
            return new Grid(
                (CellType[,])grid.Clone()
            );
        }

        /// <summary>
        /// Should be called when one or more cells have changed states.
        /// </summary>
        /// <param name="e">The event args corresponding to this event. Should contain the CellPositions of all
        /// changed cells.</param>
        /// <exclude />
        protected internal virtual void OnCellStateChanged(CellStateEventArgs e)
        {
            CellStateChanged?.Invoke(this, e);
        }
    }


}
