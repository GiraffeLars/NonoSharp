using System;
using System.Collections.Generic;
using System.Text;
using NonoSharp.Exceptions;

namespace NonoSharp
{
    /// <summary>
    /// Builder class to create a custom Nonogram puzzle, and can at the final stage
    /// be obtained as a <see cref="NonogramAPI"/> instance or be saved to a file.
    /// </summary>
    public class NonogramBuilder
    {
        /// <summary>
        /// Height of the grid that is being created
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Height of the puzzle that is being created
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Title of the puzzle. A null value means this puzzle has no title
        /// </summary>
        public string? Title {
            get { return puzzle.Title; }
            set { puzzle.Title = value; }
        }

        private bool isSolvable = false;
        private bool isSolvableDirty = true;

        private readonly SemaphoreSlim _solvableSemaphore = new(1, 1);

        private readonly PuzzleDefinition puzzle;

        /// <summary>
        /// Creates a new PuzzleBuilder instance with a width of <paramref name="width"/> and a height of
        /// <paramref name="height"/>.
        /// </summary>
        /// <param name="width">Width for the puzzle to create with this PuzzleBuilder</param>
        /// <param name="height">Height for the puzzle to create with this PuzzleBuilder</param>
        /// <param name="title">Optional title to give the puzzle</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="width"/> or <paramref name="height"/>
        /// is non-positive (&lt;= 0)</exception>
        public NonogramBuilder(int width, int height, string? title = null)
        {
            ValidateDimensions(width, height);
            Width = width;
            Height = height;

            bool[] solution = new bool[width * height];
            puzzle = new PuzzleDefinition(width, height, solution, title);
        }

        /// <summary>
        /// Converts this puzzle to a Grid
        /// </summary>
        /// <returns>Grid with solution and dimensions corresponding to this builder</returns>
        private Grid ConvertToGrid()
        {
            Grid g = new(Width, Height, puzzle.ConvertBoolSolutionToPositions());
            return g;
        }

        /// <summary>
        /// Determines whether the built puzzle is uniquely solvable.
        /// </summary>
        /// <returns>True if it is uniquely solvable, false otherwise</returns>
        public bool IsSolvable()
        {
            try
            {
                _solvableSemaphore.Wait();
                if (!isSolvableDirty) return isSolvable;

                bool solvable = Solver.IsSolvable(ConvertToGrid());

                UpdateSolvable(solvable);
                
                return solvable;
            }
            finally 
            {
                _solvableSemaphore.Release(); 
            }
        }

        /// <summary>
        /// Determines whether the built puzzle is uniquely solvable.
        /// </summary>
        /// <returns>True if it is uniquely solvable, false otherwise</returns>

        public async Task<bool> IsSolvableAsync()
        {
            try
            {
                await _solvableSemaphore.WaitAsync();
                bool solvable = await Task.Run(() => Solver.IsSolvable(ConvertToGrid()));

                UpdateSolvable(solvable);
                return solvable;
            }
            finally
            {
                _solvableSemaphore.Release();
            }
        }


        /// <summary>
        /// Updates the value in <c>this.isSolvable</c> and updates cache status.
        /// To avoid race conditions, first lock <c>_solvableSemaphore</c>
        /// </summary>
        /// <param name="newSolvableValue">New value for isSolvable</param>
        private void UpdateSolvable(bool newSolvableValue)
        {
            isSolvable = newSolvableValue;
            isSolvableDirty = false;
        }

        /// <summary>
        /// Converts this puzzle into a playable <see cref="NonogramAPI"/>.
        /// </summary>
        /// <exception cref="PuzzleNotSolvableException">Thrown when the built puzzle is not uniquely solvable</exception>
        public NonogramAPI GetNonogramAPI()
        {
            if (!IsSolvable())
            {
                throw new PuzzleNotSolvableException("The built puzzle is not uniquely solvable!");
            }

            return new(ConvertToGrid());
        }


        /// <summary>
        /// Converts this puzzle into a playable <see cref="NonogramAPI"/> asynchronously.
        /// </summary>
        /// <exception cref="PuzzleNotSolvableException">Thrown when the built puzzle is not uniquely solvable</exception>
        public async Task<NonogramAPI> GetNonogramAPIAsync()
        {
            if (!await IsSolvableAsync())
            {
                throw new PuzzleNotSolvableException("This puzzle is not uniquely solvable!");
            }

            return new(ConvertToGrid());                                                                             
        }

        /// <summary>
        /// Saves the built puzzle at <paramref name="path"/>. Specifically, the expected solution and dimension are stored.
        /// The puzzle must be uniquely solvable.
        /// </summary>
        /// <param name="path">The path to save the puzzle to</param>
        /// <exception cref="PuzzleSerializationFailedException">Thrown when serialization fails. For example, 
        /// when the given title is too long, or an I/O exception occurs.
        /// Usually, there is an inner exception giving more details.</exception>
        /// <exception cref="PuzzleSavingFailedException">Thrown when saving files fails, e.g. because of an I/O Exception.
        /// See the inner exception for more details</exception>
        /// <exception cref="PuzzleNotSolvableException">Thrown when the built puzzle is not uniquely solvable</exception>
        public void SaveAsFile(string path)
        {
            if (!IsSolvable())
            {
                throw new PuzzleNotSolvableException("The built puzzle is not uniquely solvable!");
            }

            puzzle.SavePuzzle(path);
        }


        /// <summary>
        /// Saves this puzzle asynchronously at <paramref name="path"/>. Specifically, the expected solution and dimension are stored.
        /// </summary>
        /// <param name="path">The path to save the puzzle to</param>
        /// <exception cref="PuzzleSerializationFailedException">Thrown when serialization fails. For example,
        /// when the given title is too long, or an I/O exception occurs.
        /// Usually, there is an inner exception giving more details.</exception>
        /// <exception cref="PuzzleSavingFailedException">Thrown when saving files fails, e.g. because of an I/O Exception.
        /// See the inner exception for more details</exception>
        /// <exception cref="PuzzleNotSolvableException">Thrown when the built puzzle is not uniquely solvable</exception>
        public async Task SaveAsFileAsync(string path)
        {
            if (!await IsSolvableAsync())
            {
                throw new PuzzleNotSolvableException("The built puzzle is not uniquely solvable!");
            }
            
            await puzzle.SavePuzzleAsync(path);
        }

        /// <summary>
        /// Sets the dimensions of this PuzzleBuilder. This is done by adding new empty cells to the right and bottom.
        /// Cannot shrink the puzzle.
        /// </summary>
        /// <param name="newWidth">New width for the puzzle</param>
        /// <param name="newHeight">New height for the puzzle</param>
        /// <exception cref="OverflowException">When <paramref name="newWidth"/> * <paramref name="newHeight"/>
        /// causes overflow</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="newWidth"/> or <paramref name="newHeight"/> are
        /// less than their respective old value</exception>
        public void SetDimensions(int newWidth, int newHeight)
        {
            if (newWidth == Width && newHeight == Height)
            {
                return;
            }

            puzzle.SetDimensions(newWidth, newHeight);
        }

        /// <summary>
        /// Sets the cell at (<paramref name="x"/>, <paramref name="y"/>) to <paramref name="newValue"/>.
        /// </summary>
        /// <param name="x">x-coordinate of cell to set</param>
        /// <param name="y">y-coordinate of cell to set</param>
        /// <param name="newValue">New value of cell. True if it is supposed to be filled, false if empty</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when x or y are less than 0 or are
        /// greater or equal to Width or Height respectively</exception>
        private void SetCell(int x, int y, bool newValue)
        {
            ValidateCoordinates(x, y);

            isSolvableDirty = true;
            puzzle.SetSolutionAt(x, y, newValue);
        }

        /// <summary>
        /// Marks the cell at (<paramref name="x"/>, <paramref name="y"/>) as expected to be filled in the solution.
        /// </summary>
        /// <param name="x">x-coordinate of cell to fill</param>
        /// <param name="y">y-coordinate of cell to fill</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when x or y are less than 0 or are
        /// greater or equal to Width or Height respectively</exception>
        public void FillCell(int x, int y)
        {
            SetCell(x, y, true);
        }

        /// <summary>
        /// Marks the cell at <paramref name="position"/> as expected to be filled in the solution.
        /// </summary>
        /// <param name="position">Position of cell to fill</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when x or y are less than 0 or are
        /// greater or equal to Width or Height respectively</exception>

        public void FillCell(CellPosition position)
        {
            FillCell(position.X, position.Y);
        }

        /// <summary>
        /// Marks the cell at (<paramref name="x"/>, <paramref name="y"/>) as expected to be empty in the solution.
        /// </summary>
        /// <param name="x">x-coordinate of cell to mark as empty</param>
        /// <param name="y">y-coordinate of cell to mark as empty</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when x or y are less than 0 or are
        /// greater or equal to Width or Height respectively</exception>
        public void EmptyCell(int x, int y)
        {
            SetCell(x, y, false);
        }

        /// <summary>
        /// Marks the cell at <paramref name="position"/> as expected to be empty in the solution.
        /// </summary>
        /// <param name="position">Position of cell to mark as empty</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when x or y are less than 0 or are
        /// greater or equal to Width or Height respectively</exception>

        public void EmptyCell(CellPosition position)
        {
            EmptyCell(position.X, position.Y);
        }

        /// <summary>
        /// Gets the raw value of the cell at (<paramref name="x"/>, <paramref name="y"/>).
        /// This is true if it is filled, false if it is empty.
        /// </summary>
        /// <param name="x">x-coordinate of cell to get</param>
        /// <param name="y">y-coordinate of cell to get</param>
        /// <returns>True if the cell is marked as filled in the solution, false if it is marked to be empty</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="x"/> or <paramref name="y"/> is not within the
        /// puzzle bounds </exception>
        internal bool GetCell(int x, int y)
        {
            ValidateCoordinates(x, y);
            return puzzle.GetSolutionAt(x, y);
        }

        /// <summary>
        /// Checks if Cell at (<paramref name="x"/>, <paramref name="y"/>) is expected to be filled in the solution.
        /// </summary>
        /// <param name="x">x-coordinate of cell to check</param>
        /// <param name="y">y-coordinate of cell to check</param>
        /// <returns>True if the cell is expected to be filled, false otherwise</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="x"/> or <paramref name="y"/> is not within the
        /// puzzle bounds </exception>
        public bool IsCellFilled(int x, int y)
        {
            return GetCell(x, y);
        }

        /// <summary>
        /// Checks if Cell at <paramref name="position"/> is expected to be filled in the solution.
        /// </summary>
        /// <param name="position">CellPosition to check</param>
        /// <returns>True if the cell is expected to be filled, false otherwise</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the coordinates of <paramref name="position"/> are not within the
        /// puzzle bounds </exception>
        public bool IsCellFilled(CellPosition position)
        {
            return IsCellFilled(position.X, position.Y);
        }

        /// <summary>
        /// Checks if Cell at (<paramref name="x"/>, <paramref name="y"/>) is expected to be empty in the solution.
        /// </summary>
        /// <param name="x">x-coordinate of cell to check</param>
        /// <param name="y">y-coordinate of cell to check</param>
        /// <returns>True if the cell is expected to be empty, false otherwise</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="x"/> or <paramref name="y"/> is not within the
        /// puzzle bounds </exception>
        public bool IsCellEmpty(int x, int y)
        {
            return !GetCell(x, y);
        }

        /// <summary>
        /// Checks if Cell at <paramref name="position"/> is expected to be empty in the solution.
        /// </summary>
        /// <param name="position">CellPosition to check</param>
        /// <returns>True if the cell is expected to be empty, false otherwise</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the coordinates of <paramref name="position"/> are not within the
        /// puzzle bounds </exception>
        public bool IsCellEmpty(CellPosition position)
        {
            return IsCellEmpty(position.X, position.Y);
        }

        /// <summary>
        /// Validates the dimensions given by checking if this will construct a valid puzzle grid (i.e. dimensions are positive).
        /// </summary>
        /// <param name="width">Width to check</param>
        /// <param name="height">Height to check</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="width"/> or <paramref name="height"/>
        /// is non-positive (&lt;= 0)</exception>
        private static void ValidateDimensions(int width, int height)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(width, 0, nameof(width));
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(height, 0, nameof(height));
        }

        /// <summary>
        /// Validates whether x and y are in bounds.
        /// </summary>
        /// <param name="x">x-coordinate to check</param>
        /// <param name="y">y-coordinate to check</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when x or y are less than 0 or are
        /// greater or equal to Width or Height respectively</exception>
        private void ValidateCoordinates(int x, int y)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(x, 0, nameof(x));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(x, Width, nameof(x));

            ArgumentOutOfRangeException.ThrowIfLessThan(y, 0, nameof(y));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(y, Height, nameof(y));
        }
    }
}
