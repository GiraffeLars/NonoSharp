using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        /// Width of the grid that is being created.
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Height of the puzzle that is being created.
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// Title of the puzzle. A null value means this puzzle has no title.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// The solution that is being constructed with the builder. Cells contained in Solution
        /// are the cells that will have to be filled to complete the final puzzle.
        /// </summary>
        public IReadOnlySet<CellPosition> Solution => _readOnlySolution;
        private readonly HashSet<CellPosition> _solution;
        private readonly ReadOnlySet<CellPosition> _readOnlySolution;

        private bool isSolvable = false;
        private bool isSolvableDirty = true;

        private readonly SemaphoreSlim _solvableSemaphore = new(1, 1);

        /// <summary>
        /// Creates a new PuzzleBuilder instance with a width of <paramref name="width"/> and a height of
        /// <paramref name="height"/>.
        /// </summary>
        /// <param name="width">Width for the puzzle to create with this PuzzleBuilder.</param>
        /// <param name="height">Height for the puzzle to create with this PuzzleBuilder.</param>
        /// <param name="title">Optional title to give the puzzle.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="width"/> or <paramref name="height"/>
        /// is non-positive (&lt;= 0).</exception>
        public NonogramBuilder(int width, int height, string? title = null)
        {
            ValidateDimensions(width, height);
            Width = width;
            Height = height;
            _solution = [];
            _readOnlySolution = new(_solution);
            Title = title;
        }

        /// <summary>
        /// Creates a <c>NonogramBuilder</c> instance constructed from <paramref name="puzzleString"/>.
        /// </summary>
        /// <remarks>
        /// Expected formats for <paramref name="puzzleString"/> are of the following forms:
        /// "OO O", "[O][ ][O]", where 'O' is a filled cell in the solution, and ' ' an empty one. 
        /// 'X' is also accepted for an empty cell in the solution.
        /// </remarks>
        /// <example>
        /// The following code creates a 3x3 <c>NonogramBuilder</c> with all four corners filled in the solution.
        /// <code>
        /// NonogramBuilder builder = NonogramBuilder
        ///     .FromString("O O\n   \nO O").
        /// </code>
        /// Alternatively, the same can be achieved as follows:
        /// <code>
        /// NonogramBuilder builder = NonogramBuilder
        ///     .FromString("[O][ ][O]\n[ ][ ][ ]\n[O][ ][O]").
        /// </code>
        /// </example>
        /// <param name="puzzleString">The string to convert to an instance.</param>
        /// <param name="title">Optional title to give the puzzle.</param>
        /// <returns><c>NonogramBuilder</c> instance with dimensions and solution constructed from 
        /// <paramref name="puzzleString"/>.</returns>
        /// <exception cref="FormatException">Thrown when parsing fails. See the error message for details.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the width or height parsed from 
        /// <paramref name="puzzleString"/> is non-positive.</exception>
        public static NonogramBuilder FromString(string puzzleString, string? title = null)
        {
            // Ensures we can split no matter the line break type 
            puzzleString = puzzleString.ReplaceLineEndings();
            string[] rows = puzzleString.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

            int leftBrackets = rows[0].Count('[');
            int rightBrackets = rows[0].Count(']');
            if (leftBrackets != rightBrackets)
            {
                throw new FormatException($"Mismatched '[' and ']' characters in line 0! " +
                    $"Found {leftBrackets} '[' characters but {rightBrackets} ']'!");
            }

            int width = rows[0].Length - leftBrackets - rightBrackets;
            int height = rows.Length;
            NonogramBuilder builder = new(width, height, title);


            CellRowStringParser parser;
            for (int y = 0; y < rows.Length; y++)
            {
                if (rows[y].Length != rows[0].Length) throw new FormatException($"The width of line {y} " +
                    $"does not match the width of the preceding lines!");

                if (leftBrackets > 0)
                {
                    parser = new CellBlocksParser(rows[y], y);
                } else
                {
                    parser = new StandaloneCellsParser(rows[y], y);
                }

                ParseRowString(y, parser, builder);
            }
            return builder;
        }

        private static void ParseRowString(int rowNum, CellRowStringParser parser, NonogramBuilder builder)
        {
            int x = 0;

            foreach (CellType cellType in parser.ParseCells())
            {
                if (cellType == CellType.FILLED)
                {
                    builder.FillCell(x, rowNum);
                }
                x++;
            }   
        }

        /// <summary>
        /// Converts this instance into a <c>PuzzleDefinition</c>.
        /// </summary>
        /// <returns><c>PuzzleDefinition</c> of this instance.</returns>
        private PuzzleDefinition ToPuzzleDefinition()
        {
            return new(Width, Height, _solution, Title);
        }

        /// <summary>
        /// Converts this puzzle to a <c>Puzzle</c> instance.
        /// </summary>
        /// <returns>Puzzle with solution and dimensions corresponding to this builder.</returns>
        private Puzzle ToPuzzle()
        {
            return new(ToPuzzleDefinition());
        }

        /// <summary>
        /// Determines whether the built puzzle is uniquely solvable.
        /// </summary>
        /// <returns><c>true</c> if it is uniquely solvable, <c>false</c> otherwise.</returns>
        public bool IsSolvable()
        {
            try
            {
                _solvableSemaphore.Wait();
                if (!isSolvableDirty) return isSolvable;

                bool solvable = Solver.IsSolvable(ToPuzzle());

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
        /// <returns>True if it is uniquely solvable, false otherwise.</returns>

        public async Task<bool> IsSolvableAsync()
        {
            try
            {
                await _solvableSemaphore.WaitAsync();
                bool solvable = await Task.Run(() => Solver.IsSolvable(ToPuzzle()));

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
        /// <param name="newSolvableValue">New value for isSolvable.</param>
        private void UpdateSolvable(bool newSolvableValue)
        {
            isSolvable = newSolvableValue;
            isSolvableDirty = false;
        }

        /// <summary>
        /// Converts this puzzle into a playable <see cref="NonogramAPI"/>.
        /// </summary>
        /// <exception cref="PuzzleNotSolvableException">Thrown when the built puzzle is not uniquely solvable.</exception>
        public NonogramAPI GetNonogramAPI()
        {
            if (!IsSolvable())
            {
                throw new PuzzleNotSolvableException("The built puzzle is not uniquely solvable!");
            }

            return new(ToPuzzle());
        }


        /// <summary>
        /// Converts this puzzle into a playable <see cref="NonogramAPI"/> asynchronously.
        /// </summary>
        /// <exception cref="PuzzleNotSolvableException">Thrown when the built puzzle is not uniquely solvable.</exception>
        public async Task<NonogramAPI> GetNonogramAPIAsync()
        {
            if (!await IsSolvableAsync())
            {
                throw new PuzzleNotSolvableException("This puzzle is not uniquely solvable!");
            }

            return new(ToPuzzle());                                                                             
        }

        /// <summary>
        /// Saves the built puzzle at <paramref name="path"/>. Specifically, the expected solution and dimension are stored.
        /// The puzzle must be uniquely solvable.
        /// </summary>
        /// <param name="path">The path to save the puzzle to.</param>
        /// <exception cref="PuzzleSerializationFailedException">Thrown when serialization fails. For example, 
        /// when the given title is too long, or an I/O exception occurs.
        /// Usually, there is an inner exception giving more details.</exception>
        /// <exception cref="PuzzleSavingFailedException">Thrown when saving files fails, e.g. because of an I/O Exception.
        /// See the inner exception for more details.</exception>
        /// <exception cref="PuzzleNotSolvableException">Thrown when the built puzzle is not uniquely solvable.</exception>
        public void SaveAsFile(string path)
        {
            if (!IsSolvable())
            {
                throw new PuzzleNotSolvableException("The built puzzle is not uniquely solvable!");
            }

            ToPuzzleDefinition().SavePuzzle(path);
        }


        /// <summary>
        /// Saves this puzzle asynchronously at <paramref name="path"/>. Specifically, the expected solution and dimension are stored.
        /// </summary>
        /// <param name="path">The path to save the puzzle to.</param>
        /// <exception cref="PuzzleSerializationFailedException">Thrown when serialization fails. For example,
        /// when the given title is too long, or an I/O exception occurs.
        /// Usually, there is an inner exception giving more details.</exception>
        /// <exception cref="PuzzleSavingFailedException">Thrown when saving files fails, e.g. because of an I/O Exception.
        /// See the inner exception for more details.</exception>
        /// <exception cref="PuzzleNotSolvableException">Thrown when the built puzzle is not uniquely solvable.</exception>
        public async Task SaveAsFileAsync(string path)
        {
            if (!await IsSolvableAsync())
            {
                throw new PuzzleNotSolvableException("The built puzzle is not uniquely solvable!");
            }
            
            await ToPuzzleDefinition().SavePuzzleAsync(path);
        }

        /// <summary>
        /// Sets the dimensions of this PuzzleBuilder. This is done by adding new empty cells to the right and bottom.
        /// Cannot shrink the puzzle.
        /// </summary>
        /// <param name="newWidth">New width for the puzzle</param>
        /// <param name="newHeight">New height for the puzzle</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="newWidth"/> or <paramref name="newHeight"/> are
        /// less than their respective old value</exception>
        public void SetDimensions(int newWidth, int newHeight)
        {
            if (newWidth == Width && newHeight == Height)
            {
                return;
            }

            ArgumentOutOfRangeException.ThrowIfLessThan(newWidth, Width, nameof(newWidth));
            ArgumentOutOfRangeException.ThrowIfLessThan(newHeight, Height, nameof(newHeight));

            Width = newWidth;
            Height = newHeight;
        }

        private void SetSolutionAt(int x, int y, bool filled)
        {
            if (Solution == null) throw new InvalidOperationException("There is no solution for this puzzle!");

            try
            {
                _solvableSemaphore.Wait();

                CellPosition position = new(x, y);
                if (filled)
                {
                    _solution.Add(position);
                }
                else
                {
                    _solution.Remove(position);
                }
                isSolvableDirty = true;
            } finally
            {
                _solvableSemaphore.Release();
            }
        }

        /// <summary>
        /// Sets the cell at (<paramref name="x"/>, <paramref name="y"/>) to <paramref name="newValue"/>.
        /// </summary>
        /// <param name="newValue">New value of cell. True if it is supposed to be filled, false if empty</param>
        /// <param name="x">x-coordinate.</param>
        /// <param name="y">y-coordinate</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when x or y are less than 0 or are
        /// greater or equal to Width or Height respectively</exception>
        private void SetCell(int x, int y, bool newValue)
        {
            ValidateCoordinates(x, y);
            SetSolutionAt(x, y, newValue);
        }

        /// <summary>
        /// Sets the cell at (<paramref name="x"/>, <paramref name="y"/>) to <paramref name="newCellType"/>.
        /// </summary>
        /// <param name="x">x-coordinate of cell to set.</param>
        /// <param name="y">y-coordinate of cell to set.</param>
        /// <param name="newCellType">The new <c>CellType</c>. Can not be <c>CellType.CROSS</c>.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="newCellType"/> is <c>CellType.CROSS</c></exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when x or y are less than 0 or are
        /// greater or equal to Width or Height respectively</exception>
        public void SetCell(int x, int y, CellType newCellType)
        {
            if (newCellType == CellType.CROSS)
            {
                throw new ArgumentException("Can not cross cells when using NonogramBuilder!");
            }

            SetCell(x, y, newCellType == CellType.FILLED);
        }

        /// <summary>
        /// Sets the cell at <paramref name="position"/> to <paramref name="newCellType"/>.
        /// </summary>
        /// <inheritdoc cref="SetCell(int,int,CellType)"/>
        /// <param name="position">The position of the cell to set.</param>
        /// <param name="newCellType">The new <c>CellType</c>. Can not be <c>CellType.CROSS</c>.</param>
        public void SetCell(CellPosition position, CellType newCellType)
        {
            SetCell(position.X, position.Y, newCellType);
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
        /// Gets the <c>CellType</c> of the cell at (<paramref name="x"/>, <paramref name="y"/>).
        /// </summary>
        /// <param name="x">x-coordinate of cell to get.</param>
        /// <param name="y">y-coordinate of cell to get.</param>
        /// <returns>The <c>CellType</c> of the cell at (<paramref name="x"/>, <paramref name="y"/>).</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="x"/> or <paramref name="y"/> is not within the
        /// puzzle bounds.</exception>
        public CellType GetCell(int x, int y)
        {
            ValidateCoordinates(x, y);
            return _solution.Contains(new(x, y)) ? CellType.FILLED : CellType.BLANK;
        }

        /// <summary>
        /// Gets the <c>CellType</c> of the cell at <paramref name="position"/>.
        /// </summary>
        /// <inheritdoc cref="GetCell(int,int)"/>
        /// <param name="position">The position of the cell.</param>
        public CellType GetCell(CellPosition position)
        {
            return GetCell(position.X, position.Y);
        }

        /// <summary>
        /// Checks if the cell at (<paramref name="x"/>, <paramref name="y"/>) is expected to be filled in the solution.
        /// </summary>
        /// <param name="x">x-coordinate of the cell to check.</param>
        /// <param name="y">y-coordinate of the cell to check.</param>
        /// <returns>True if the cell is expected to be filled, false otherwise.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="x"/> or <paramref name="y"/> is not within the
        /// puzzle bounds.</exception>
        public bool IsCellFilled(int x, int y)
        {
            return GetCell(x, y) == CellType.FILLED;
        }

        /// <summary>
        /// Checks if a cell at <paramref name="position"/> is expected to be filled in the solution.
        /// </summary>
        /// <param name="position">CellPosition to check.</param>
        /// <returns>True if the cell is expected to be filled, false otherwise.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the coordinates of <paramref name="position"/> are not within the
        /// puzzle bounds.</exception>
        public bool IsCellFilled(CellPosition position)
        {
            return IsCellFilled(position.X, position.Y);
        }

        /// <summary>
        /// Checks if a cell at (<paramref name="x"/>, <paramref name="y"/>) is expected to be empty in the solution.
        /// </summary>
        /// <param name="x">x-coordinate of the cell to check.</param>
        /// <param name="y">y-coordinate of the cell to check.</param>
        /// <returns><c>true</c> if the cell is expected to be empty, <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="x"/> or <paramref name="y"/> is not within the
        /// puzzle bounds.</exception>
        public bool IsCellEmpty(int x, int y)
        {
            return !IsCellFilled(x, y);
        }

        /// <summary>
        /// Checks if a cell at <paramref name="position"/> is expected to be empty in the solution.
        /// </summary>
        /// <param name="position">CellPosition to check.</param>
        /// <returns><c>true</c> if the cell is expected to be empty, <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the coordinates of <paramref name="position"/> are not within the
        /// puzzle bounds.</exception>
        public bool IsCellEmpty(CellPosition position)
        {
            return IsCellEmpty(position.X, position.Y);
        }

        /// <summary>
        /// Validates the dimensions given by checking if this will construct a valid puzzle grid (i.e. dimensions are positive).
        /// </summary>
        /// <param name="width">Width to check.</param>
        /// <param name="height">Height to check.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="width"/> or <paramref name="height"/>
        /// is non-positive (&lt;= 0).</exception>
        private static void ValidateDimensions(int width, int height)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(width, 0, nameof(width));
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(height, 0, nameof(height));
        }

        /// <summary>
        /// Validates whether x and y are in bounds.
        /// </summary>
        /// <param name="x">x-coordinate to check.</param>
        /// <param name="y">y-coordinate to check.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when x or y are less than 0 or are
        /// greater or equal to Width or Height respectively.</exception>
        private void ValidateCoordinates(int x, int y)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(x, 0, nameof(x));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(x, Width, nameof(x));

            ArgumentOutOfRangeException.ThrowIfLessThan(y, 0, nameof(y));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(y, Height, nameof(y));
        }
    }

    internal abstract class CellRowStringParser(string row, int lineNumber)
    {
        /// <summary>
        /// The row containing the cells in string format
        /// </summary>
        protected readonly string row = row;

        /// <summary>
        /// The line number this parser is parsing
        /// </summary>
        private readonly int lineNumber = lineNumber;

        /// <summary>
        /// Parses the cells.
        /// </summary>
        /// <returns>IEnumerator of parsed cells</returns>
        public abstract IEnumerable<CellType> ParseCells();

        /// <summary>
        /// Tries to parse <paramref name="cell"/> into a <c>CellType</c>.
        /// </summary>
        /// <param name="cell">cell character to parse</param>
        /// <param name="result">Will be filled if <paramref name="cell"/> is either ' ', 'X', or 'O' and <c>null</c> otherwise.</param>
        /// <returns></returns>
        public static bool TryParseCell(char cell, out CellType? result)
        {
            switch (cell)
            {
                case ' ':
                    result = CellType.BLANK;
                    return true;
                case 'X':
                    result = CellType.CROSS;
                    return true;
                case 'O':
                    result = CellType.FILLED;
                    return true;
                default:
                    result = null;
                    return false;
            }
        }

        /// <inheritdoc cref="ParseCell(int, char)"/>
        protected CellType ParseCell(char cell)
        {
            if (TryParseCell(cell, out var result) && result.HasValue)
            {
                return result.Value;
            }
            throw InvalidCharacterError(null, cell, ' ', 'X', 'O');
        }

        /// <summary>
        /// Parses a cell such as ' ' or 'O'.
        /// </summary>
        /// <param name="position">The position in the line this character is at.</param>
        /// <param name="cell">Cell character to parse</param>
        /// <returns>CellType of parsed cell</returns>
        /// <exception cref="FormatException">Invalid cell character</exception>
        protected CellType ParseCell(int position, char cell)
        {
            if (TryParseCell(cell, out var result) && result.HasValue)
            {
                return result.Value;
            }
            throw InvalidCharacterError(position, cell, ' ', 'X', 'O');
        }

        protected FormatException Error(int? position, string message)
        {
            if (position.HasValue)
            {
                throw new FormatException($"Failed to parse line {lineNumber} at position {position.Value}: {message}");
            } else
            {
                throw new FormatException($"Failed to parse line {lineNumber}: {message}");
            }
        }

        protected FormatException InvalidCharacterError(int? position, char actual, params char[] expected)
        {
            return Error(position, $"Invalid character ('{actual}')! Expected one of the following: " +
                $"{String.Join(", ", expected.Select(c => $"'{c}'"))}.");
        }

        protected FormatException InvalidCharacterError(int? position, char actual, char expected)
        {
            return Error(position, $"Invalid character ('{actual}')! Expected '{expected}'.");
        }
    }

    /// <summary>
    /// Parses cell rows in the forms such as "OO OX "
    /// </summary>
    internal class StandaloneCellsParser(string row, int lineNumber) : CellRowStringParser(row, lineNumber)
    {
        /// <inheritdoc/>
        /// <exception cref="FormatException">Thrown when encountering an invalid character.</exception>
        public override IEnumerable<CellType> ParseCells()
        {
            for (int i = 0; i < row.Length; i++)
            {
                yield return ParseCell(row[i]);
            }
        }
    }


    /// <summary>
    /// Parses cell rows in forms such as "[O][O][ ][O][X][ ]"
    /// </summary>
    internal class CellBlocksParser(string row, int lineNumber) : CellRowStringParser(row, lineNumber)
    {
        /// <inheritdoc/>
        /// <exception cref="FormatException">Thrown when a cell block is not of forms such as "[O]"</exception>
        public override IEnumerable<CellType> ParseCells()
        {
            for (int i = 0; i < row.Length; i += 3)
            {
                CellType parsedAs;
                if (row[i] != '[')
                {
                    throw InvalidCharacterError(i, row[i], '[');
                }

                if (i + 1 >= row.Length)
                {
                    throw Error(null, $"Incomplete line! Line ended with {row[i]} but more characters were expected.");
                }

                parsedAs = ParseCell(row[i + 1]);

                if (i + 2 >= row.Length)
                {
                    throw Error(null, $"Incomplete line! Line ended with {row[i + 1]} but more characters were expected.");
                }

                if (row[i + 2] != ']')
                {
                    throw InvalidCharacterError(i + 2, row[i + 2], ']');
                }

                yield return parsedAs;
            }
        }
    }
}
