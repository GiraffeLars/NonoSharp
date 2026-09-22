using NonoSharp.Events;
using NonoSharp.Exceptions;
using System.Collections.Frozen;

namespace NonoSharp
{
    /// <summary>
    /// Class for the Nonogram API. Can be initialised with static methods such as 
    /// <see cref="CreateRandomPuzzle(int, int, NonogramOptions)"/> or <see cref="LoadPuzzle(string, NonogramOptions)"/>, or
    /// by using the constructor with your own solution, 
    /// <see cref="NonogramAPI(int, int, ISet{CellPosition}, NonogramOptions)"/>.
    /// </summary>
    public class NonogramAPI
    {
        internal Puzzle puzzle;
        internal readonly MoveManager moveManager;
        internal HistoryManager History => moveManager.History;

        /// <summary>
        /// The width of the game grid.
        /// </summary>
        public int Width => puzzle.Width;

        /// <summary>
        /// The height of the game grid.
        /// </summary>
        public int Height => puzzle.Height;

        /// <summary>
        /// The clues, i.e. the numbers on the side of a grid, for the columns of the grid.
        /// In Nonogram puzzles these are usually shown at the top of the grid.
        /// </summary>
        public Clues[] ColumnClues => puzzle.ColumnClues;


        /// <summary>
        /// The clues, i.e. the numbers on the side of a grid, for the rows of the grid.
        /// In Nonogram puzzles these are usually shown at the left side of the grid.
        /// </summary>
        public Clues[] RowClues => puzzle.RowClues;

        /// <summary>
        /// A boolean indicating whether an undo via <see cref="Undo"/> is possible.
        /// </summary>
        public bool CanUndo => History.CanUndo;

        /// <summary>
        /// A boolean indicating whether a redo via <see cref="Redo"/> is possible.
        /// </summary>
        public bool CanRedo => History.CanRedo;

        /// <summary>
        /// The solution for the current puzzle, i.e. the cells (and only those cells) that must be filled
        /// for the puzzle to be solved.
        /// </summary>
        public FrozenSet<CellPosition> Solution { get { return puzzle.Solution!; } }

        /// <summary>
        /// The <see cref="NonogramOptions"/> for this instance.
        /// </summary>
        public NonogramOptions Options { get => moveManager.Options; set => moveManager.Options = value; }

        // Events
        /// <summary>
        /// <c>CellStateChanged</c> is raised when one or more cells change to a new state.
        /// </summary>
        public event EventHandler<CellStateEventArgs>? CellStateChanged;

        /// <summary>
        /// Raised when the puzzle has been solved.
        /// </summary>
        public event EventHandler? PuzzleSolved;

        /// <summary>
        /// Raised when a cell is changed incorrectly and is therefore corrected to the expected state of the cell.
        /// </summary>
        public event EventHandler<CorrectionEventArgs>? CellCorrected;

        internal NonogramAPI(Puzzle puzzle, NonogramOptions? options = null)
        {
            this.puzzle = puzzle;
            moveManager = new(options, puzzle);

            moveManager.CellCorrected += (s, e) => OnCellCorrected(e);
            // The puzzle can switch between solved and unsolved when a cell changes state.
            // Thus, we can handle sending the puzzle solved event after a cell changes state.
            CellStateChanged += (s, a) => HandlePuzzleSolvedEvent();
        }

        /// <summary>
        /// Creates a <c>NonogramAPI</c> instance with a width of <paramref name="width"/>, height of
        /// <paramref name="height"/> and sets the solution to <paramref name="solution"/>.
        /// </summary>
        /// <remarks>
        /// It is not checked whether the created puzzle can be solved. As such, using this constructor with an improper 
        /// <paramref name="solution"/> can lead to the puzzle being unsolvable. To create puzzles that are guaranteed to be
        /// uniquely solvable, use <see cref="NonogramBuilder"/> or create a random solvable puzzle with methods such as
        /// <see cref="CreateRandomPuzzle(int, int, NonogramOptions)"/>.
        /// </remarks>
        /// <param name="width">The width of the puzzle.</param>
        /// <param name="height">The height of the puzzle.</param>
        /// <param name="solution">The solution of the puzzle.</param>
        /// <param name="options">The <see cref="NonogramOptions"/> to use. Leave as <c>null</c> to use the default options.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when width or height are non-positive.</exception>
        /// <exception cref="IndexOutOfRangeException">Thrown when at least one of the <see cref="CellPosition"/>s found in
        /// <paramref name="solution"/> is out of the bounds set by <paramref name="width"/> and <paramref name="height"/>.</exception>
        public NonogramAPI(int width, int height, ISet<CellPosition> solution,
            NonogramOptions? options = null) : this(new(width, height, solution), options)
        { }


        /// <summary>
        /// Creates a <c>NonogramAPI</c> with a solution based on <paramref name="columnClues"/> and <paramref name="rowClues"/>.
        /// </summary>
        /// <remarks>
        /// The puzzle must be uniquely solvable.
        /// </remarks>
        /// <param name="width">Width of the puzzle.</param>
        /// <param name="height">Height of the puzzle.</param>
        /// <param name="columnClues">Array of <see cref="Clues"/> instances, one for each column of the puzzle.</param>
        /// <param name="rowClues">Array of <see cref="Clues"/> instances, one for each row of the puzzle.</param>
        /// <param name="options">The <see cref="NonogramOptions"/> to use. Leave as <c>null</c> to use the default options.</param>
        /// <exception cref="ArgumentException">Thrown when the lengths of <paramref name="columnClues"/> 
        /// and <paramref name="rowClues"/> do not match <paramref name="width"/> 
        /// or <paramref name="height"/> respectively.</exception>
        /// <exception cref="ArgumentOutOfRangeException"> Thrown when <paramref name="width"/> 
        /// or <paramref name="height"/> are non-positive.</exception>
        /// <exception cref="PuzzleNotSolvableException">Thrown when the puzzle constructed from the clues and 
        /// dimensions is not uniquely solvable.</exception>
        public NonogramAPI(int width, int height, Clues[] columnClues, Clues[] rowClues, NonogramOptions? options = null)
        {
            bool solvable = Solver.IsSolvable(width, height, columnClues, rowClues, out var solution);
            if (!solvable || solution == null)
            {
                throw new PuzzleNotSolvableException(
                    "The puzzle constructed from given clues and dimensions is not uniquely solvable!");
            }

            Puzzle puzzle = new(width, height, solution);
            this.puzzle = puzzle;

            moveManager = new(options, puzzle);
            moveManager.CellCorrected += (s, e) => OnCellCorrected(e);
            CellStateChanged += (s, a) => HandlePuzzleSolvedEvent();
        }

        /// <summary>
        /// Creates an API instance with a random puzzle. See <see cref="CreateRandomPuzzleAsync(int, int, NonogramOptions)"/> 
        /// for the asynchronous method.
        /// </summary>
        /// <remarks>
        /// Generating solvable puzzles is computationally expensive and the runtime increases drastically with
        /// <paramref name="width"/> and <paramref name="height"/>. To ensure a fast runtime, try to keep puzzle dimensions
        /// small. The library has no (theoretical) restrictions on <paramref name="width"/> or <paramref name="height"/>,
        /// but it is not recommend to generate large puzzles.
        /// </remarks>
        /// <param name="width">Width of the grid for the game.</param>
        /// <param name="height">Height of the grid for the game.</param>
        /// <param name="seed">The seed to use for randomisation when generating a puzzle.</param>
        /// <param name="options">The <see cref="NonogramOptions"/> to use. Leave as <c>null</c> to use the default options.</param>
        /// <returns>NonogramAPI instance as described above.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when width or height are non-positive.</exception>
        public static NonogramAPI CreateRandomPuzzle(int width, int height, int seed, NonogramOptions? options = null)
        {
            HashSet<CellPosition> solution = SolutionHelper.GenerateRandomSolution(width, height, seed);
            return new NonogramAPI(width, height, solution, options);
        }

        ///<inheritdoc cref="CreateRandomPuzzle(int, int, int, NonogramOptions?)"/>
        public static NonogramAPI CreateRandomPuzzle(int width, int height, NonogramOptions? options = null)
        {
            HashSet<CellPosition> sol = SolutionHelper.GenerateRandomSolution(width, height);
            return new NonogramAPI(width, height, sol, options);
        }

        /// <summary>
        /// Creates an API instance with a random puzzle asynchronously by running 
        /// <see cref="NonogramAPI.CreateRandomPuzzle(int, int, NonogramOptions)"/> on the ThreadPool
        /// as it is computionally expensive.
        /// </summary>
        /// <remarks>
        /// Generating solvable puzzles is computationally expensive and the runtime increases drastically with
        /// <paramref name="width"/> and <paramref name="height"/>. To ensure a fast runtime, try to keep puzzle dimensions
        /// small. The library has no (theoretical) restrictions on <paramref name="width"/> or <paramref name="height"/>,
        /// but it is not recommend to generate large puzzles.
        /// </remarks>
        /// <param name="width">Width of the grid for the game.</param>
        /// <param name="height">Height of the grid for the game.</param>
        /// <param name="seed">The seed to use for randomisation when generating a puzzle.</param>
        /// <param name="options">The <see cref="NonogramOptions"/> to use. Leave as <c>null</c> to use the default options.</param>
        /// <returns>NonogramAPI instance as described above.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when width or height are non-positive.</exception>
        public async static Task<NonogramAPI> CreateRandomPuzzleAsync(int width, int height, int seed, NonogramOptions? options = null)
        {
            return await Task.Run(() => CreateRandomPuzzle(width, height, seed, options));
        }

        /// <inheritdoc cref="CreateRandomPuzzleAsync(int, int, int, NonogramOptions?)"/>
        public async static Task<NonogramAPI> CreateRandomPuzzleAsync(int width, int height, NonogramOptions? options = null)
        {
            return await Task.Run(() => CreateRandomPuzzle(width, height, options));
        }

        /// <summary>
        /// Fills the cell at (<paramref name="x"/>, <paramref name="y"/>).
        /// </summary>
        /// <param name="x">x-coordinate of the cell, zero-indexed from the left.</param>
        /// <param name="y">y-coordinate of the cell, zero-indexed from the top.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="x"/> or 
        /// <paramref name="y"/> falls outside the bounds of the grid.</exception>
        public void FillCell(int x, int y)
        {
            SetCell(x, y, CellType.FILLED);
        }

        /// <summary>
        /// Fills the cell at <paramref name="position"/>.
        /// </summary>
        /// <inheritdoc cref="FillCell(int,int)"/>
        /// <param name="position">The position of the cell to fill.</param>
        public void FillCell(CellPosition position)
        {
            FillCell(position.X, position.Y);
        }

        /// <summary>
        /// Marks the cell at (<paramref name="x"/>, <paramref name="y"/>) as crossed.
        /// </summary>
        /// <param name="x">x-coordinate of the cell, zero-indexed from the left.</param>
        /// <param name="y">y-coordinate of the cell, zero-indexed from the top.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="x"/> or
        /// <paramref name="y"/> falls outside the bounds of the grid.</exception>
        public void CrossCell(int x, int y)
        {
            SetCell(x, y, CellType.CROSS);
        }

        /// <summary>
        /// Marks the cell at <paramref name="position"/> as crossed.
        /// </summary>
        /// <inheritdoc cref="CrossCell(int,int)"/>
        /// <param name="position">The position of the cell to cross.</param>
        public void CrossCell(CellPosition position)
        {
            CrossCell(position.X, position.Y);
        }

        /// <summary>
        /// Clears the cell at (<paramref name="x"/>, <paramref name="y"/>), returning it to blank.
        /// </summary>
        /// <param name="x">x-coordinate of the cell, zero-indexed from the left.</param>
        /// <param name="y">y-coordinate of the cell, zero-indexed from the top.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="x"/> or 
        /// <paramref name="y"/> falls outside the bounds of the grid.</exception>
        public void EmptyCell(int x, int y)
        {
            SetCell(x, y, CellType.BLANK);
        }

        /// <summary>
        /// Clears the cell at <paramref name="position"/>, returning it to blank.
        /// </summary>
        /// <inheritdoc cref="EmptyCell(int,int)"/>
        /// <param name="position">The position of the cell to empty.</param>
        public void EmptyCell(CellPosition position)
        {
            EmptyCell(position.X, position.Y);
        }

        /// <summary>
        /// Sets the cell at (<paramref name="x"/>, <paramref name="y"/>) to <paramref name="newCellType"/>.
        /// </summary>
        /// <param name="x">x-coordinate of the cell, zero-indexed from the left.</param>
        /// <param name="y">y-coordinate of the cell, zero-indexed from the top.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="x"/> or 
        /// <paramref name="y"/> falls outside the bounds of the grid.</exception>
        /// <param name="newCellType">The CellType to set the cell at (<paramref name="x"/>, <paramref name="y"/>) to.</param>

        public void SetCell(int x, int y, CellType newCellType)
        {
            var changes = moveManager.DoMove(x, y, newCellType);
            OnCellStateChanged(new(changes));
        }        

        /// <summary>
        /// Undoes the last move (if any). Silently returns if there is no command to undo.
        /// </summary>
        public void Undo() 
        {
            var changedCells = History.Undo();
            if (changedCells != null)
            {
                OnCellStateChanged(new([.. changedCells]));
            }
        }

        /// <summary>
        /// Redo the last undone move (if any). Silently returns if there is no command to redo.
        /// </summary>
        public void Redo()
        {
            var changedCells = History.Redo();
            if (changedCells != null)
            {
                OnCellStateChanged(new(changedCells));
            }
        }

        /// <summary>
        /// Determines whether the cell at (<paramref name="x"/>, <paramref name="y"/>) is empty.
        /// </summary>
        /// <param name="x">x-coordinate of cell to check.</param>
        /// <param name="y">y-coordinate of cell to check.</param>
        /// <returns><c>true</c> if the cell is empty, <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the cell to check is out
        /// of the grid bounds.</exception>
        public bool IsCellEmpty(int x, int y)
        {
            return GetCell(x, y) == CellType.BLANK;
        }

        /// <summary>
        /// Determines whether the cell at <paramref name="position"/> is empty.
        /// </summary>
        /// <param name="position">Position to check.</param>
        /// <inheritdoc cref="IsCellEmpty(int, int)"/>
        public bool IsCellEmpty(CellPosition position)
        {
            return IsCellEmpty(position.X, position.Y);
        }

        /// <summary>
        /// Determines whether the cell at (<paramref name="x"/>, <paramref name="y"/>) is filled.
        /// </summary>
        /// <param name="x">x-coordinate of cell to check.</param>
        /// <param name="y">y-coordinate of cell to check.</param>
        /// <returns><c>true</c> if the cell is filled, <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the cell to check is out
        /// of the grid bounds.</exception>
        public bool IsCellFilled(int x, int y)
        {
            return GetCell(x, y) == CellType.FILLED;
        }

        /// <summary>
        /// Determines whether the cell at <paramref name="position"/> is filled.
        /// </summary>
        /// <inheritdoc cref="IsCellFilled(int,int)"/>
        /// <param name="position">Position to check.</param>
        public bool IsCellFilled(CellPosition position)
        {
            return IsCellFilled(position.X, position.Y);
        }


        /// <summary>
        /// Determines whether the cell at (<paramref name="x"/>, <paramref name="y"/>) is crossed.
        /// </summary>
        /// <param name="x">x-coordinate of cell to check.</param>
        /// <param name="y">y-coordinate of cell to check.</param>
        /// <returns><c>true</c> if the cell is crossed, <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the cell to check is out
        /// of the grid bounds.</exception>
        public bool IsCellCrossed(int x, int y)
        {
            return GetCell(x, y) == CellType.CROSS;
        }

        /// <summary>
        /// Determines whether the cell at <paramref name="position"/> is crossed.
        /// </summary>
        /// <inheritdoc cref="IsCellCrossed(int,int)"/>
        /// <param name="position">Position to check.</param>
        public bool IsCellCrossed(CellPosition position)
        {
            return IsCellCrossed(position.X, position.Y);
        }

        /// <summary>
        /// Gets the <see cref="CellType"/> of the cell located at (<paramref name="x"/>, <paramref name="y"/>).
        /// </summary>
        /// <param name="x">x-coordinate of the cell.</param>
        /// <param name="y">y-coordinate of the cell.</param>
        /// <returns>CellType of the cell at (<paramref name="x"/>, <paramref name="y"/>).</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="x"/> or <paramref name="y"/> is out
        /// of bounds of the grid.</exception>
        public CellType GetCell(int x, int y)
        {
            return puzzle.Grid.GetCell(x, y);
        }

        /// <summary>Gets the <see cref="CellType"/> of the cell at <paramref name="position"/>.</summary>
        /// <returns>CellType of the cell at <paramref name="position"/>.</returns>
        /// <inheritdoc cref="GetCell(int, int)"/>
        /// <param name="position">The position of the cell.</param>
        public CellType GetCell(CellPosition position)
        {
            return GetCell(position.X, position.Y);
        }

        /// <summary>
        /// Determines whether the puzzle is solved, i.e. the filled cells match the solution exactly.
        /// </summary>
        /// <returns><c>true</c> if the puzzle is solved, <c>false</c> otherwise.</returns>
        public bool IsPuzzleSolved()
        {
            return puzzle.IsSolved();
        }

        /// <summary>
        /// Determines whether the cell at (<paramref name="x"/>, <paramref name="y"/>) is correctly placed
        /// according to the solution.
        /// </summary>
        /// <param name="x">x-coordinate of the cell to check.</param>
        /// <param name="y">y-coordinate of the cell to check.</param>
        /// <inheritdoc cref="IsCellCorrect(CellPosition)"/>
        public bool IsCellCorrect(int x, int y)
        {
            return IsCellCorrect(new(x, y));
        }

        /// <summary>
        /// Determines whether the cell at <paramref name="position"/> is correctly placed according to the solution.
        /// </summary>
        /// <param name="position">The position to check.</param>
        /// <returns><c>true</c> if the cell is placed correctly, <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the cell to check is out
        /// of the grid bounds.</exception>
        public bool IsCellCorrect(CellPosition position)
        {
            return IsCellFilled(position) == Solution.Contains(position);
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

        /// <summary>
        /// Should be called when the puzzle has been solved in current move.
        /// </summary>
        /// <exclude />
        protected internal virtual void OnPuzzleSolved()
        {
            PuzzleSolved?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Should be called when a cell was corrected to the expected type found in the solution.
        /// </summary>
        /// <param name="e">Event args containing the changed cell, the requested state to change to and the corrected cell type.</param>
        /// <exclude />
        protected internal virtual void OnCellCorrected(CorrectionEventArgs e)
        {
            CellCorrected?.Invoke(this, e);
        }

        /// <summary>
        /// Checks if the puzzle is solved and calls <see cref="OnPuzzleSolved()"/> if this is the case
        /// </summary>
        private void HandlePuzzleSolvedEvent()
        {
            if (puzzle.IsSolved())
            {
                OnPuzzleSolved();
            }
        }

        /// <summary>
        /// Saves a serialized version of the puzzle (that is, the solution and dimensions) to <paramref name="path"/>.
        /// If <paramref name="path"/> already exists, it is overwritten.
        /// </summary>
        /// <param name="path">Path to save the puzzle at.</param>
        /// <param name="title">Optional title to give the puzzle.</param>
        /// <exception cref="PuzzleSerializationFailedException">Thrown when serialization failed.
        /// For example, when the given title is too long, or an I/O exception occurs.
        /// Usually, there is an inner exception giving more details.</exception>
        /// <exception cref="PuzzleSavingFailedException">Thrown when saving files fails,
        /// e.g. because of an I/O Exception. See the inner exception for more details.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is <c>null</c> or empty.</exception>
        public void SaveAsFile(string path, string? title = null)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(path, nameof(path));
            PuzzleDefinition puzzle = new(Width, Height, this.puzzle.Solution!, title);
            puzzle.SavePuzzle(path);
        }

        /// <summary>
        /// Saves a serialized version of the puzzle (that is, the solution and dimensions) to <paramref name="path"/>.
        /// If <paramref name="path"/> already exists, it is overwritten.
        /// </summary>
        /// <param name="path">Path to save the puzzle at.</param>
        /// <param name="title">Optional title to give the puzzle.</param>
        /// <exception cref="PuzzleSerializationFailedException">Thrown when serialization failed.
        /// For example, when the given title is too long, or an I/O exception occurs.
        /// Usually, there is an inner exception giving more details.</exception>
        /// <exception cref="PuzzleSavingFailedException">Thrown when saving files fails, e.g. because of an I/O Exception.
        /// See the inner exception for more details.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is <c>null</c> or empty.</exception>
        public async Task SaveAsFileAsync(string path, string? title = null)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(path, nameof(path));
            PuzzleDefinition puzzleDef = new(Width, Height, puzzle.Solution!, title);
            await puzzleDef.SavePuzzleAsync(path);
        }

        /// <summary>
        /// Loads the puzzle at <paramref name="path"/> and returns a new NonogramAPI instance.
        /// </summary>
        /// <param name="path">Puzzle to load.</param>
        /// <param name="options">The <see cref="NonogramOptions"/> to use. Leave as <c>null</c> to use the default options.</param>
        /// <returns>NonogramAPI instance of the puzzle located at the given path.</returns>
        /// <exception cref="InvalidFileFormatException">Thrown when the given file format is not supported.</exception>
        /// <exception cref="NotSupportedException">Thrown when the version of the save system is not supported.</exception>
        /// <exception cref="PuzzleLoadingFailedException">Thrown when loading files fails, e.g. because of an I/O Exception.
        /// See the inner exception for more details.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is <c>null</c> or empty.</exception>
        public static NonogramAPI LoadPuzzle(string path, NonogramOptions? options = null) 
        {
            ArgumentNullException.ThrowIfNullOrEmpty(path, nameof(path));
            PuzzleDefinition puzzleDef = PuzzleDefinition.LoadPuzzle(path);

            Puzzle puzzle = new(puzzleDef);
            return new(puzzle) { Options = options ?? new() };
        }

        /// <summary>
        /// Loads the puzzle in <paramref name="stream"/> and returns a new NonogramAPI instance.
        /// </summary>
        /// <remarks>
        /// <paramref name="stream"/> is left open. Do not forget to close it.
        /// </remarks>
        /// <param name="stream">Stream to read the puzzle from.
        /// To avoid false positives on InvalidFileFormatException exceptions, the stream must consist of ONLY one valid puzzle,
        /// such as one provided by <see cref="PuzzleDefinition.SavePuzzle(string)"/>.</param>
        /// <returns>NonogramAPI instance of the puzzle.</returns>
        /// <param name="options">The <see cref="NonogramOptions"/> to use. Leave as <c>null</c> to use the default options.</param>
        /// <exception cref="InvalidFileFormatException">Thrown when the given file format is not supported.</exception>
        /// <exception cref="NotSupportedException">Thrown when the version of the save system is not supported.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="stream"/> is <c>null</c>.</exception>
        public static NonogramAPI LoadPuzzle(Stream stream, NonogramOptions? options = null)
        {
            ArgumentNullException.ThrowIfNull(stream, nameof(stream));
            PuzzleDefinition puzzleDefinition = PuzzleDefinition.LoadPuzzle(stream);

            Puzzle puzzle = new(puzzleDefinition);
            return new(puzzle) { Options = options ?? new()};
        }

        /// <summary>
        /// Loads the puzzle at <paramref name="path"/> asynchronously and returns a new NonogramAPI instance.
        /// </summary>
        /// <param name="path">Puzzle to load</param>
        /// <param name="options">The <see cref="NonogramOptions"/> to use. Leave as <c>null</c> to use the default options</param>
        /// <returns>NonogramAPI instance of the puzzle located at the given path</returns>
        /// <exception cref="InvalidFileFormatException">Thrown when the given file format is not supported</exception>
        /// <exception cref="NotSupportedException">Thrown when the version of the save system is not supported</exception>
        /// <exception cref="PuzzleLoadingFailedException">Thrown when loading files fails, e.g. because of an I/O Exception. 
        /// See the inner exception for more details</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is <c>null</c> or empty</exception>
        public static async Task<NonogramAPI> LoadPuzzleAsync(string path, NonogramOptions? options = null)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(path, nameof(path));
            PuzzleDefinition puzzleDef = await PuzzleDefinition.LoadPuzzleAsync(path);

            Puzzle puzzle = new(puzzleDef);
            return new(puzzle) { Options = options ?? new() };
        }

        /// <summary>
        /// Loads the puzzle in <paramref name="stream"/> and then asynchronously converts the read data into a usable 
        /// <see cref="NonogramAPI"/>.
        /// </summary>
        /// <remarks>
        /// Contents of <paramref name="stream"/> are expected to be relatively small and is read synchronously. 
        /// Larger stream contents might cause noticeable blocking.<br/>
        /// <paramref name="stream"/> is left open. Do not forget to close it.
        /// </remarks>
        /// <param name="stream">Stream to read the puzzle from. 
        /// To avoid false positives on InvalidFileFormatException exceptions,
        /// the stream must consist of ONLY one valid puzzle, such as one provided by
        /// <see cref="PuzzleDefinition.SavePuzzle(string)"/>.</param>
        /// <param name="options">The <see cref="NonogramOptions"/> to use. Leave as <c>null</c> to use the default options</param>
        /// <returns>A <c>NonogramAPI</c> instance of the puzzle loaded from the stream</returns>
        /// <exception cref="InvalidFileFormatException">Thrown when the given file format is not supported</exception>
        /// <exception cref="NotSupportedException">Thrown when the version of the save system is not supported</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="stream"/> is <c>null</c></exception>
        public static async Task<NonogramAPI> LoadPuzzleAsync(Stream stream, NonogramOptions? options = null)
        {
            ArgumentNullException.ThrowIfNull(stream, nameof(stream));
            PuzzleDefinition puzzleDef = PuzzleDefinition.LoadPuzzle(stream);

            Puzzle puzzle = new(puzzleDef);
            return new(puzzle) { Options = options ?? new() };
        }

        /// <summary>
        /// Converts this instance into a nice string representation of the grid. This includes all cells and
        /// their respective states and the clues at the sides.
        /// </summary>
        /// <returns>A string showing the current state of the game</returns>
        public override String ToString() { return puzzle.ToString(); }
    }
}
