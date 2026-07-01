using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text;

namespace Picross.Game
{
    internal class Grid : ICloneable
    {
        private readonly SquareType[,] grid;
        private List<Point> solution;
        private int filled = 0;
        private int paddingString = 0;

        public int Width { get; }
        public int Height { get; }

        public Hints[] VerticalHints { get; }
        public Hints[] HorizontalHints { get; }

        /// <summary>
        /// Constructs a Grid.
        /// </summary>
        /// <param name="width">Width of the grid</param>
        /// <param name="height">Height of the grid</param>
        /// <exception cref="ArgumentException">Thrown when width or height are non-positive</exception>
        public Grid(int width, int height)
        {
            if (width <= 0 || height <= 0)
            {
                throw new ArgumentException("Width and height must be positive integers.");
            }

            this.Width = width;
            this.Height = height;

            grid = new SquareType[width, height];


            VerticalHints = new Hints[width];
            HorizontalHints = new Hints[height];

            // Generate a random grid and guarantee it is solvable
            do
            {
                solution = new List<Point>();
                FillRandomly(solution);
                InitializeHints();
            } while (!Solver.IsSolvable(this));
        }

        /// <summary>
        /// Creates a full custom Grid
        /// </summary>
        /// <param name="grid">List of SquareType, with data of filled in squares, etc..</param>
        /// <param name="solution">Solution to this grid</param>
        /// <param name="filled">How many squares are filled in. Should be consistent with <paramref name="grid"/>.</param>
        /// <param name="paddingString">The padding used to pad out the hints when converting to string</param>
        /// <param name="width">Width of the grid. Should be consistent with <paramref name="grid"/></param>
        /// <param name="height">Height of the grid. Should be consistent with <paramref name="grid"/></param>
        /// <param name="verticalHints">Vertical hint data (i.e. those on top of the grid)</param>
        /// <param name="horizontalHints">Horizontal hint data (i.e. those on the left of the grid)</param>
        internal Grid(SquareType[,] grid, List<Point> solution, int filled, int paddingString, int width, int height)
        {
            this.grid = grid;
            this.filled = filled;
            this.paddingString = paddingString;
            Width = width;
            Height = height;
            VerticalHints = new Hints[width];
            HorizontalHints = new Hints[height];
            SetSolution(solution);
        }



        /// <summary>
        /// Makes the solution into a 2D array representation, just as <paramref name="this.grid"/>
        /// </summary>
        /// <returns>2D array of <c>SquareType</c> where each point in the solution is <c>SquareType.FILLED</c></returns>
        private SquareType[,] GridifySolution()
        {
            SquareType[,] s = new SquareType[Width, Height];

            foreach (Point p in solution)
            {
                s[p.X, p.Y] = SquareType.FILLED;
            }

            return s;
        }

        /// <summary>
        /// Sets the solution to a Grid and sets the hints.
        /// </summary>
        /// <param name="solution"></param>
        [MemberNotNull(nameof(solution))]
        internal void SetSolution(List<Point> solution)
        { 
            this.solution = solution;
            InitializeHints();
        }

        private void InitializeHints()
            
        {
            for (int i = 0; i < Width; i++)
            {
                VerticalHints[i] = new Hints(true, i);
            }
            for (int i = 0; i < Height; i++)
            {
                HorizontalHints[i] = new Hints(false, i);
            }
            SetHints(VerticalHints, true);
            SetHints(HorizontalHints, false);
            
        }

        /// <summary>
        /// Creates the hints for the solution of this grid.
        /// </summary>
        /// <param name="hints">Which hints to set, either <c>verticalHints</c> or <c>horizontalHints</c></param>
        /// <param name="vertical">Whether we are setting the verticalHints, corresponding to the <paramref name="hints"/> parameter</param>
        private void SetHints(Hints[] hints, bool vertical)
        {
            // Sets the hint limits based on whether we process the vertical hints
            int xLimit = vertical ? Width : Height;
            int yLimit = vertical ? Height : Width;

            SquareType[,] gridSol = GridifySolution();
            for (int x = 0; x < xLimit; x++)
            {
                int count = 0;
                for (int y = 0; y < yLimit; y++)
                {
                    SquareType cell = vertical ? gridSol[x, y] : gridSol[y, x];

                    // If this is not a filled square
                    if (cell != SquareType.FILLED)
                    {
                        // Add the new hint to the list
                        AddHint(hints, x, count); // TODO if squares are split (i.e. empty between two patches), separate them with a 0
                        count = 0;
                        continue;
                    }

                    count++;
                }

                // Do final hint adding in case the last square is filled
                // Count minus 1 as it is increased by one even if unfilled
                SquareType lastCell = vertical ? gridSol[x, yLimit - 1] : gridSol[yLimit - 1, x];
                if (count > 0 && lastCell == SquareType.FILLED)
                {
                    AddHint(hints, x, count);
                }
                else if (hints[x].Count == 0)
                {
                    hints[x].Add(new Hint(0));
                }

                DoHorizontalPaddingCount(hints[x].Count, vertical);
            }
        }

        private void AddHint(Hints[] hints, int pos, int count)
        {
            if (count > 0)
            {
                hints[pos].Add(new Hint(count));
            }
        }

        private void DoHorizontalPaddingCount(int count, bool vertical)
        {
            if (!vertical && count > paddingString)
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
        public void SetCell(int x, int y, SquareType value)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                throw new ArgumentOutOfRangeException("Cell coordinates are out of bounds.");
            }

            if (grid[x, y] != SquareType.FILLED && value == SquareType.FILLED)
            {
                filled++; // Keeps track of whether the same amount of squares are filled as the solution for efficiency
            }
            else if (grid[x, y] == SquareType.FILLED && value != SquareType.FILLED)
            {
                filled--;
            }

            grid[x, y] = value;

            // TODO change hints from ints to using Hints and Hint classes then change completion here
            HorizontalHints[y].DoCompletion(this);
            VerticalHints[x].DoCompletion(this);
        }

        /// <summary>
        /// Gets the specified cell of this grid
        /// </summary>
        /// <param name="x">x-coordinate of the cell</param>
        /// <param name="y">y-coordinate of the cell</param>
        /// <returns><c>SquareType</c> of the requested cell</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when either x or y is out of bounds</exception>
        public SquareType GetCell(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                throw new ArgumentOutOfRangeException("Cell coordinates are out of bounds.");
            }

            return grid[x, y];
        }

        /// <summary>
        /// Get the current column data as a LinkedList. For an array representation, see <seealso cref="GetColumnArray(int)"/>.
        /// </summary>
        /// <param name="column">The column of the board to get. Must be between 0 and Width</param>
        /// <returns>LinkedList of cells and their associated SquareType data</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <c>column</c> is not a valid column, i.e. out of range of the grid width.</exception>
        internal LinkedList<SquareType> GetColumn(int column)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(column);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(column, Width);

            LinkedList<SquareType> list = new LinkedList<SquareType>();

            for (int i = 0; i < Height; i++)
            {
                SquareType type = grid[column, i];
                list.AddLast(type);
            }
            return list;
        }

        /// <summary>
        /// Get the current column data as an array. For an LinkedList representation, see <seealso cref="GetColumn(int)(int)"/>.
        /// </summary>
        /// <param name="column">The column of the board to get. Must be between 0 and Width</param>
        /// <returns>Array of SquareType corresponding to the cells in the column</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <c>column</c> is not a valid column, i.e. out of range of the grid width.</exception>
        internal SquareType[] GetColumnArray(int column)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(column);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(column, Width);

            SquareType[] cells = new SquareType[Height];

            for (int i = 0; i < Height; i++)
            {
                SquareType type = grid[column, i];
                cells[i] = type;
            }
            return cells;
        }

        /// <summary>
        /// Get the current row data as a LinkedList. For an array representation, see <seealso cref="GetRowArray(int)"/>.
        /// </summary>
        /// <param name="row">The row of the board to get. Must be between 0 and Height</param>
        /// <returns>LinkedList of cells and their associated SquareType data</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <c>row</c> is not a valid row, i.e. out of range of the grid height.</exception>
        internal LinkedList<SquareType> GetRow(int row)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(row);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(row, Height);

            LinkedList<SquareType> list = new LinkedList<SquareType>();

            for (int i = 0; i < Width; i++)
            {
                SquareType type = grid[i, row];
                list.AddLast(type);
            }
            return list;
        }

        /// <summary>
        /// Get the current row data as an array. For an LinkedList representation, see <seealso cref="GetRow(int)"/>.
        /// </summary>
        /// <param name="row">The row of the board to get. Must be between 0 and Height</param>
        /// <returns>Array of SquareType corresponding to the cells in the row</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <c>row</c> is not a valid row, i.e. out of range of the grid height.</exception>
        internal SquareType[] GetRowArray(int row) 
        {
            ArgumentOutOfRangeException.ThrowIfNegative(row);
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(row, Height);

            SquareType[] cells = new SquareType[Width];

            for (int i = 0; i < Width; i++)
            {
                SquareType type = grid[i, row];
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
        internal void SetRow(int row, SquareType[] newRow)
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

        internal void SetColumn(int column, SquareType[] newColumn)
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

        private void FillRandomly(List<Point> g)
        {
            var random = new Random();
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (random.NextInt64() % 2 == 0)
                    {
                        Point p = new Point(x, y);
                        g.Add(p);
                    }
                }
            }
        }

        public bool IsSolved()
        {
            if (filled != solution.Count())
            {
                return false; 
            }

            for (int i = 0; i < solution.Count(); i++)
            {
                Point p = solution[i];
                if (grid[p.X, p.Y] != SquareType.FILLED)
                {
                    return false;
                }
            }

            return true;
        }

        public override string ToString()
        {
            //foreach (Point p in solution)
            //{
            //    Console.Write(p);
            //    Console.Write(" ");
            //}
            //Console.WriteLine();

            StringBuilder sb = new StringBuilder();
            String[] horizontalHintsStr = CreateHorizontalHintsString();

            sb.Append(CreateVerticalHintsString());
            //sb.AppendLine();

            for (int y = 0; y < Height; y++)
            {
                sb.Append('\n');
                sb.Append(horizontalHintsStr[y]);
                for (int x = 0; x < Width; x++)
                {
                    char c = ' ';
                    switch (GetCell(x, y))
                    {
                        case SquareType.FILLED:
                            c = 'O';
                            break;
                        case SquareType.BLANK:
                            c = ' ';
                            break;
                        case SquareType.CROSS:
                            c = 'X';
                            break;
                        
                    }
                    sb.Append($"[{c}]");
                }
                
            }
            return sb.ToString();
        }

        private String CreateVerticalHintsString()
        {
            StringBuilder sb = new StringBuilder();
            
            //Console.WriteLine(getPadding());
            //Console.WriteLine(paddingString);

            bool newStringRow;
            int lastFilled;
            for (int y = 0; y < Height; y++)
            {
                sb.Append(GetPadding());
                newStringRow = false;
                lastFilled = 0;
                for (int x = 0; x < Width; x++)
                {
                    Hints hints = VerticalHints[x];
                    if (hints.Count > y)
                    {
                        // Add spaces for all columns with no hints until this column
                        sb.Append(new string(' ', (x - lastFilled) * 3));
                        sb.Append($" {hints.GetHint(y)} ");
                        
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

        private String[] CreateHorizontalHintsString()
        {
            String[] hintsStr = new String[Height];

            for (int i = 0; i < Height; i++)
            {
                int x;
                StringBuilder sb = new StringBuilder();
                Hints hints = HorizontalHints[i];

                for (x = 0; x < hints.Count; x++)
                {
                    sb.Append($"{hints.GetHint(x)} ");
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
                (SquareType[,]) grid.Clone(),
                solution,
                filled,
                paddingString,
                Width,
                Height
            );
        }
    }


}
