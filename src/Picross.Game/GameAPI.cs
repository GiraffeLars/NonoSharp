using System.Diagnostics;
using Picross.Game.Events;

namespace Picross.Game
{
    /// <summary>
    /// Class for the Nonogram API. Can be initialised with static methods such as 
    /// <see cref="CreateRandomPuzzle(int, int)"/> or <see cref="LoadPuzzle(string)"/>.
    /// </summary>
    public class GameAPI
    {
        private Grid grid;

        /// <summary>
        /// The width of the game grid
        /// </summary>
        public int Width { get { return grid.Width; } }

        /// <summary>
        /// The height of the game grid
        /// </summary>
        public int Height { get { return grid.Height; } }

        /// <summary>
        /// The hints, i.e. the numbers on the side of a grid, for the columns of the grid.
        /// In Nonogram puzzles these are usually shown at the top of the grid.
        /// </summary>
        public Hints[] ColumnHints { get { return grid.ColumnHints; } }


        /// <summary>
        /// The hints, i.e. the numbers on the side of a grid, for the rows of the grid.
        /// In Nonogram puzzles these are usually shown at the left side of the grid.
        /// </summary>
        public Hints[] RowHints { get { return grid.RowHints; } }

        /// <summary>
        /// A boolean indicating whether an undo via <see cref="Undo"/> is possible.
        /// </summary>
        public bool CanUndo { get { return undoStack.Count != 0; } }

        /// <summary>
        /// A boolean indicating whether a redo via <see cref="Redo"/> is possible.
        /// </summary>
        public bool CanRedo { get { return redoStack.Count != 0; } }

        private readonly LinkedList<ICommand> undoStack;
        private readonly LinkedList<ICommand> redoStack;

        // Events
        /// <summary>
        /// <c>CellStateChanged</c> is raised when one or more cell change to a new state.
        /// </summary>
        public event EventHandler<CellStateEventArgs>? CellStateChanged;

        /// <summary>
        /// Raised when the puzzle has been solved.
        /// </summary>
        public event EventHandler? PuzzleSolved;

        /// <summary>
        /// Creates an API instance with a random puzzle. See <see cref="GameAPI.CreateRandomPuzzleAsync(int, int)"/> 
        /// for the asynchronous method.
        /// </summary>
        /// <param name="width">Width of the grid for the game</param>
        /// <param name="height">Height of the grid for the game</param>
        /// <returns>GameAPI instance as described above</returns>
        public static GameAPI CreateRandomPuzzle(int width, int height)
        {
            // Generate solution using a task as generating a puzzle is expensive
            List<CellPosition> sol = SolutionHelper.GenerateRandomSolution(width, height);
            Grid g = new(width, height, sol);
            return new GameAPI(g);
        }

        /// <summary>
        /// Creates an API instance with a random puzzle asynchronously by running 
        /// <see cref="GameAPI.CreateRandomPuzzle(int, int)"/> on the ThreadPool
        /// as it is computionally expensive.
        /// </summary>
        /// <param name="width">Width of the grid for the game</param>
        /// <param name="height">Height of the grid for the game</param>
        /// <returns>GameAPI instance as described above</returns>
        public async static Task<GameAPI> CreateRandomPuzzleAsync(int width, int height)
        {
            return await Task.Run(() => CreateRandomPuzzle(width, height));
        }

        internal GameAPI(Grid grid)
        {
            this.grid = grid;
            undoStack = new LinkedList<ICommand>();
            redoStack = new LinkedList<ICommand>();

            // The puzzle can switch between solved and unsolved when a cell changes state.
            // Thus, we can handle sending the puzzle solved event after a cell changes state.
            CellStateChanged += (s, a) => { HandlePuzzleSolvedEvent(); };
        }

        /// <summary>
        /// Fills the cell at (<paramref name="x"/>, <paramref name="y"/>)
        /// </summary>
        /// <param name="x">x-coordinate of the cell, zero-indexed from the left.</param>
        /// <param name="y">y-coordinate of the cell, zero-indexed from the top.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="x"/> or 
        /// <paramref name="y"/> falls outside the bounds of the grid.</exception>
        public void FillCell(int x, int y)
        {
            DoMove(x, y, CellType.FILLED);
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
            DoMove(x, y, CellType.CROSS);
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
            DoMove(x, y, CellType.BLANK);
        }

        /// <summary>
        /// Executes the player's move. Changes the cell at (<paramref name="x"/>, <paramref name="y"/>) to 
        /// <paramref name="newType"/> and handles auto-crosses.
        /// The move is also appended to the undo-stack.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="newType"></param>
        private void DoMove(int x, int y, CellType newType)
        {
            // Check if auto-crosses are possible (i.e. a cell goes to filled or from filled)
            bool mightAutoCross = newType == CellType.FILLED || grid.GetCell(x, y) == CellType.FILLED;

            CellCommand initialCommand = CreateCellCommand(x, y, newType);

            // Execute the command before determining auto-crosses
            initialCommand.Execute();

            if (!mightAutoCross)
            {
                // Only push this command and send event if auto-cross is not possible and return
                PushCommand(initialCommand);
                OnCellStateChanged(new([.. initialCommand.GetChanges()]));
                return;
            }

            // Get auto-cross command and execute
            CompositeCommand autoCrossCommand = GetAutoCrossCommand(x, y);

            if (autoCrossCommand.Count > 0)
            {
                autoCrossCommand.Execute();

                // Push the move and the auto-crosses to the stack together for one fluid undo for the player
                CompositeCommand combinedCommand = CompositeCommand.Combine(initialCommand, autoCrossCommand);
                PushCommand(combinedCommand);
                OnCellStateChanged(new([.. combinedCommand.GetChanges()]));
            } else
            {
                // Don't execute auto cross command and only push the initial command since the auto cross command is empty
                PushCommand(initialCommand);
                OnCellStateChanged(new([.. initialCommand.GetChanges()]));
            }
        }

        /// <summary>
        /// Executes <paramref name="command"/> and pushes it onto the undoStack. See also <seealso cref="PushCommand(ICommand)"/>.
        /// </summary>
        /// <param name="command">Command to execute and push</param>
        private void ExecuteCommand(ICommand command)
        {
            command.Execute();
            PushCommand(command);
        }

        /// <summary>
        /// Pushes <paramref name="command"/> onto <c>undoStack</c> and clears <c>redoStack</c>.
        /// </summary>
        /// <param name="command">Command to push</param>
        private void PushCommand(ICommand command)
        {
            undoStack.AddLast(command);
            redoStack.Clear();
        }

        /// <summary>
        /// Returns the CompositeCommand consisting of cross cell commands replacing blank cells after a line has been completed
        /// </summary>
        /// <param name="x">x coordinate of cell changed in current move</param>
        /// <param name="y">y coordinate of cell changed in current move</param>
        /// <returns>CompositeCommand as above</returns>
        private CompositeCommand GetAutoCrossCommand(int x, int y)
        {
            LinkedList<ICommand> autoCrossCommands = [];

            foreach (int i in GetColumnAutoCross(x))
            {
                ICommand cmd = CreateCellCommand(x, i, CellType.CROSS);
                autoCrossCommands.AddLast(cmd);
            }

            foreach (int i in GetRowAutoCross(y))
            {
                ICommand cmd = CreateCellCommand(i, y, CellType.CROSS);
                autoCrossCommands.AddLast(cmd);
            }

            return new CompositeCommand(autoCrossCommands);
        }

        private List<int> GetColumnAutoCross(int col)
        {
            LinkedList<int> groups = grid.GetGroupsInColumn(col);
            Hints hints = grid.ColumnHints[col];
            List<int> posToCross = [];

            bool groupsMatchHints = DoGroupsMatchHints(groups, hints);

            if (groupsMatchHints)
            {
                CellType[] column = grid.GetColumnArray(col);

                for (int i = 0; i < column.Length; i++)
                {
                    if (column[i] == CellType.BLANK)
                    {
                        posToCross.Add(i);
                    }
                }
            }

            return posToCross;
        }

        private List<int> GetRowAutoCross(int row)
        {
            LinkedList<int> groups = grid.GetGroupsInRow(row);
            Hints hints = grid.RowHints[row];
            List<int> posToCross = [];

            bool groupsMatchHints = DoGroupsMatchHints(groups, hints);

            if (groupsMatchHints)
            {
                CellType[] rowCells = grid.GetRowArray(row);
                for (int i = 0; i < rowCells.Length; i++)
                {
                    if (rowCells[i] == CellType.BLANK)
                    {
                        posToCross.Add(i);
                    }
                }
            }

            return posToCross;
        }

        /// <summary>
        /// Determines whether <paramref name="groups"/> exactly matches <paramref name="hints"/>, i.e. consist of the same groups.
        /// </summary>
        /// <param name="groups">
        /// A linked list where each value represents the size of a consecutive group of filled cells.
        /// </param>
        /// <param name="hints">
        /// The expected hints for the line corresponding with <paramref name="groups"/>.
        /// </param>
        /// <returns>
        /// true if the number of groups matches the hints as described, false otherwise.
        /// </returns>
        private static bool DoGroupsMatchHints(LinkedList<int> groups, Hints hints)
        {
            if (hints.Count != groups.Count)
            {
                return false;
            }

            // Check if each group has the same number of cells filled as expected in the hints
            bool groupsMatchHints = true;
            LinkedListNode<int>? node = groups.First;
            for (int i = 0; i < groups.Count; i++)
            {
                if (hints[i].Number != node!.Value)
                {
                    groupsMatchHints = false;
                    break;
                }
                node = node.Next;
            }

            return groupsMatchHints;
        }

        private CellCommand CreateCellCommand(int x, int y, CellType newType)
        {
            CellType oldType = grid.GetCell(x, y);
            CellCommand c = new(x, y, grid, newType, oldType);
            return c;
        }

        /// <summary>
        /// Undoes the last move (if any). Silently returns if there is no command to undo.
        /// </summary>
        public void Undo()
        {
            if (!CanUndo) return;

            ICommand c = undoStack.Last!.Value; // We know for sure that this isn't null as non-empty
            undoStack.RemoveLast();
            c.Undo();
            redoStack.AddLast(c);

            OnCellStateChanged(new([.. c.GetChanges()]));
        }

        /// <summary>
        /// Redo the last undone move (if any). Silently returns if there is no command to redo.
        /// </summary>
        public void Redo()
        {
            if (!CanRedo) return;

            ICommand c = redoStack.Last!.Value;
            redoStack.RemoveLast();
            c.Execute();
            undoStack.AddLast(c);

            OnCellStateChanged(new([.. c.GetChanges()]));
        }

        /// <summary>
        /// Determines whether the cell at (<paramref name="x"/>, <paramref name="y"/>) is empty
        /// </summary>
        /// <param name="x">x-coordinate of cell to check</param>
        /// <param name="y">y-coordinate of cell to check</param>
        /// <returns>True if the cell is empty, false otherwise</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="x"/> or <paramref name="y"/> is out
        /// of bounds of the grid</exception>
        public bool IsCellEmpty(int x, int y)
        {
            return grid.GetCell(x, y) == CellType.BLANK;
        }

        /// <summary>
        /// Determines whether the cell at (<paramref name="x"/>, <paramref name="y"/>) is filled
        /// </summary>
        /// <param name="x">x-coordinate of cell to check</param>
        /// <param name="y">y-coordinate of cell to check</param>
        /// <returns>True if the cell is filled, false otherwise</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="x"/> or <paramref name="y"/> is out
        /// of bounds of the grid</exception>
        public bool IsCellFilled(int x, int y)
        {
            return grid.GetCell(x, y) == CellType.FILLED;
        }


        /// <summary>
        /// Determines whether the cell at (<paramref name="x"/>, <paramref name="y"/>) is crossed
        /// </summary>
        /// <param name="x">x-coordinate of cell to check</param>
        /// <param name="y">y-coordinate of cell to check</param>
        /// <returns>True if the cell is crossed, false otherwise</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="x"/> or <paramref name="y"/> is out
        /// of bounds of the grid</exception>
        public bool IsCellCrossed(int x, int y)
        {
            return grid.GetCell(x, y) == CellType.CROSS;
        }

        /// <summary>
        /// Determines whether the puzzle is solved, i.e. the filled cells match the solution exactly.
        /// </summary>
        /// <returns>True if the puzzle is solved, false otherwise</returns>
        public bool IsPuzzleSolved()
        {
            return grid.IsSolved();
        }

        /// <summary>
        /// Should be called when one or more cells have changed states
        /// </summary>
        /// <param name="e">The event args corresponding to this event. Should contain the CellPositions of all
        /// changed cells.</param>
        protected virtual void OnCellStateChanged(CellStateEventArgs e)
        {
            CellStateChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Should be called when the puzzle has been solved in current move
        /// </summary>
        protected virtual void OnPuzzleSolved()
        {
            PuzzleSolved?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Checks if the puzzle is solved and calls <see cref="OnPuzzleSolved()"/> if this is the case
        /// </summary>
        private void HandlePuzzleSolvedEvent()
        {
            if (grid.IsSolved())
            {
                OnPuzzleSolved();
            }
        }

        /// <summary>
        /// Saves a serialized version of the puzzle (that is, the solution and dimensions) to <paramref name="path"/>. 
        /// If <paramref name="path"/> already exists, it is overwritten.
        /// </summary>
        /// <param name="path">Path to save the puzzle at</param>
        /// <param name="title">Optional title to give the puzzle</param>
        /// <exception cref="PuzzleSerializationFailedException">Thrown when serialization failed. 
        /// For example, when the given title is too long, or an I/O exception occurs.
        /// Usually, there is an inner exception giving more details.</exception>
        /// <exception cref="PuzzleSavingFailedException">Thrown when saving files fails, 
        /// e.g. because of an I/O Exception. See the inner exception for more details</exception>
        public void SaveAsFile(string path, string? title = null)
        {
            PuzzleDefinition puzzle = new(Width, Height, grid.Solution, title);
            puzzle.SavePuzzle(path);
        }

        /// <summary>
        /// Saves a serialized version of the puzzle (that is, the solution and dimensions) to <paramref name="path"/>. 
        /// If <paramref name="path"/> already exists, it is overwritten.
        /// </summary>
        /// <param name="path">Path to save the puzzle at</param>
        /// <param name="title">Optional title to give the puzzle</param>
        /// <exception cref="PuzzleSerializationFailedException">Thrown when serialization failed. 
        /// For example, when the given title is too long, or an I/O exception occurs.
        /// Usually, there is an inner exception giving more details.</exception>
        /// <exception cref="PuzzleSavingFailedException">Thrown when saving files fails, e.g. because of an I/O Exception. 
        /// See the inner exception for more details</exception>
        public async Task SaveAsFileAsync(string path, string? title = null)
        {
            PuzzleDefinition puzzle = new PuzzleDefinition(Width, Height, grid.Solution, title);
            await puzzle.SavePuzzleAsync(path);
        }

        /// <summary>
        /// Loads the puzzle at <paramref name="path"/> and returns a new GameAPI instance.
        /// </summary>
        /// <param name="path">Puzzle to load</param>
        /// <returns>GameAPI instance of the puzzle located at the given path</returns>
        /// <exception cref="InvalidFileFormatException">Thrown when the given file format is not supported</exception>
        /// <exception cref="NotSupportedException">Thrown when the version of the save system is not supported</exception>
        /// <exception cref="PuzzleLoadingFailedException">Thrown when loading files fails, e.g. because of an I/O Exception.
        /// See the inner exception for more details</exception>
        public static GameAPI LoadPuzzle(string path) 
        {
            PuzzleDefinition puzzle = PuzzleDefinition.LoadPuzzle(path);

            Grid grid = ConvertPuzzleDefinitionToGrid(puzzle);
            return new(grid);
        }

        /// <summary>
        /// Loads the puzzle in <paramref name="stream"/> and returns a new GameAPI instance. The stream is automatically closed.
        /// </summary>
        /// <param name="stream">Stream to read the puzzle from. 
        /// To avoid false positives on InvalidFileFormatException exceptions, the stream must consist of ONLY one valid puzzle, 
        /// such as one provided by <see cref="PuzzleDefinition.SavePuzzle(string)"/>.</param>
        /// <returns>GameAPI instance of the puzzle</returns>
        /// <exception cref="InvalidFileFormatException">Thrown when the given file format is not supported</exception>
        /// <exception cref="NotSupportedException">Thrown when the version of the save system is not supported</exception>
        public static GameAPI LoadPuzzle(Stream stream)
        {
            PuzzleDefinition puzzle = PuzzleDefinition.LoadPuzzle(stream);

            Grid grid = ConvertPuzzleDefinitionToGrid(puzzle);
            return new(grid);
        }

        /// <summary>
        /// Loads the puzzle at <paramref name="path"/> asynchronously and returns a new GameAPI instance.
        /// </summary>
        /// <param name="path">Puzzle to load</param>
        /// <returns>GameAPI instance of the puzzle located at the given path</returns>
        /// <exception cref="InvalidFileFormatException">Thrown when the given file format is not supported</exception>
        /// <exception cref="NotSupportedException">Thrown when the version of the save system is not supported</exception>
        /// <exception cref="PuzzleLoadingFailedException">Thrown when loading files fails, e.g. because of an I/O Exception. 
        /// See the inner exception for more details</exception>
        public static async Task<GameAPI> LoadPuzzleAsync(string path)
        {
            PuzzleDefinition puzzle = await PuzzleDefinition.LoadPuzzleAsync(path);

            Grid grid = await ConvertPuzzleDefinitionToGridAsync(puzzle);
            return new(grid);
        }

        /// <summary>
        /// Loads the puzzle in <paramref name="stream"/> asynchronously. Contents of <paramref name="stream"/> 
        /// are expected to be relatively small. Larger streams might cause noticeable blocking. 
        /// The stream is automatically closed.
        /// </summary>
        /// <param name="stream">Stream to read the puzzle from. 
        /// To avoid false positives on InvalidFileFormatException exceptions,
        /// the stream must consist of ONLY one valid puzzle, such as one provided by
        /// <see cref="PuzzleDefinition.SavePuzzle(string)"/>.</param>
        /// <returns>GameAPI instance of the puzzle located at the given path</returns>
        /// <exception cref="InvalidFileFormatException">Thrown when the given file format is not supported</exception>
        /// <exception cref="NotSupportedException">Thrown when the version of the save system is not supported</exception>
        public static async Task<GameAPI> LoadPuzzleAsync(Stream stream)
        {
            PuzzleDefinition puzzle = PuzzleDefinition.LoadPuzzle(stream);

            Grid grid = await ConvertPuzzleDefinitionToGridAsync(puzzle);
            return new(grid);
        }

        private static Grid ConvertPuzzleDefinitionToGrid(PuzzleDefinition definition)
        {
            Grid grid = new Grid(definition.Width, definition.Height);
            grid.SetSolution(definition.ConvertBoolSolutionToPositions());
            return grid;
        }

        private static async Task<Grid> ConvertPuzzleDefinitionToGridAsync(PuzzleDefinition definition)
        {
            Grid grid = new Grid(definition.Width, definition.Height);

            List<CellPosition> solution = definition.ConvertBoolSolutionToPositions();

            await Task.Run(() => grid.SetSolution(solution));
            return grid;
        }

        /// <summary>
        /// Converts this instance into a nice string representation of the grid. This includes all cells and
        /// their respective states and the hints at the sides.
        /// </summary>
        /// <returns>A string showing the current state of the game</returns>
        public override String ToString() { return grid.ToString(); }
    }
}
