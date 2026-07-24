using System.Diagnostics;

namespace Picross.Game
{
    public class GameAPI
    {
        private Grid grid;

        public int Width { get { return grid.Width; } }
        public int Height { get { return grid.Height; } }
        public Hints[] ColumnHints { get { return grid.ColumnHints; } }
        public Hints[] RowHints { get { return grid.RowHints; } }
        public bool CanUndo { get { return undoStack.Count != 0; } }
        public bool CanRedo { get { return redoStack.Count != 0; } }

        private readonly LinkedList<Command> undoStack;
        private readonly LinkedList<Command> redoStack;

        public GameAPI(int width, int height) {
            grid = new Grid(width, height);
            undoStack = new LinkedList<Command>();
            redoStack = new LinkedList<Command>();
        }

        internal GameAPI(Grid grid)
        {
            this.grid = grid;
            undoStack = new LinkedList<Command>();
            redoStack = new LinkedList<Command>();
        }

        /// <summary>
        /// Fills the cell at (<paramref name="x"/>, <paramref name="y"/>)
        /// </summary>
        /// <param name="x">x-coordinate of the cell, zero-indexed from the left.</param>
        /// <param name="y">y-coordinate of the cell, zero-indexed from the top.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="x"/> or <paramref name="y"/> falls outside the bounds of the grid.</exception>
        public void FillCell(int x, int y)
        {
            DoMove(x, y, SquareType.FILLED);
        }

        /// <summary>
        /// Marks the cell at (<paramref name="x"/>, <paramref name="y"/>) as crossed.
        /// </summary>
        /// <param name="x">x-coordinate of the cell, zero-indexed from the left.</param>
        /// <param name="y">y-coordinate of the cell, zero-indexed from the top.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="x"/> or <paramref name="y"/> falls outside the bounds of the grid.</exception>
        public void CrossCell(int x, int y)
        {
            DoMove(x, y, SquareType.CROSS);
        }

        /// <summary>
        /// Clears the cell at (<paramref name="x"/>, <paramref name="y"/>), returning it to blank.
        /// </summary>
        /// <param name="x">x-coordinate of the cell, zero-indexed from the left.</param>
        /// <param name="y">y-coordinate of the cell, zero-indexed from the top.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="x"/> or <paramref name="y"/> falls outside the bounds of the grid.</exception>
        public void EmptyCell(int x, int y)
        {
            DoMove(x, y, SquareType.BLANK);
        }

        /// <summary>
        /// Executes the player's move. Changed (<paramref name="x"/>, <paramref name="y"/>) to <paramref name="newType"/> and handles auto-crosses.
        /// The move is also appended to the undo-stack.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="newType"></param>
        private void DoMove(int x, int y, SquareType newType)
        {
            // Check if auto-crosses are possible (i.e. a square goes to filled or from filled)
            bool mightAutoCross = newType == SquareType.FILLED || grid.GetCell(x, y) == SquareType.FILLED;

            CellCommand initalCommand = CreateCellCommand(x, y, newType);

            // Execute the command before determining auto-crosses
            initalCommand.Execute();

            if (!mightAutoCross)
            {
                // Only push this command if auto-cross is not possible and return
                PushCommand(initalCommand);
                return;
            }

            // Get auto-cross command and execute
            CompositeCommand autoCrossCommand = GetAutoCrossCommand(x, y);

            if (autoCrossCommand.Count > 0)
            {
                autoCrossCommand.Execute();

                // Push the move and the auto-crosses to the stack together for one fluid undo for the player
                PushCommand(CompositeCommand.Combine(initalCommand, autoCrossCommand));
            } else
            {
                // Don't execute auto cross command and only push the initial command since the auto cross command is empty
                PushCommand(initalCommand);
            }
        }

        /// <summary>
        /// Executes <paramref name="command"/> and pushes it onto the undoStack. See also <seealso cref="PushCommand(Command)"/>.
        /// </summary>
        /// <param name="command">Command to execute and push</param>
        private void ExecuteCommand(Command command)
        {
            command.Execute();
            PushCommand(command);
        }

        /// <summary>
        /// Pushes <paramref name="command"/> onto <c>undoStack</c> and clears <c>redoStack</c>.
        /// </summary>
        /// <param name="command">Command to push</param>
        private void PushCommand(Command command)
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
            LinkedList<Command> autoCrossCommands = [];

            foreach (int i in GetColumnAutoCross(x))
            {
                Command cmd = CreateCellCommand(x, i, SquareType.CROSS);
                autoCrossCommands.AddLast(cmd);
            }

            foreach (int i in GetRowAutoCross(y))
            {
                Command cmd = CreateCellCommand(i, y, SquareType.CROSS);
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
                SquareType[] column = grid.GetColumnArray(col);

                for (int i = 0; i < column.Length; i++)
                {
                    if (column[i] == SquareType.BLANK)
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
                SquareType[] rowCells = grid.GetRowArray(row);
                for (int i = 0; i < rowCells.Length; i++)
                {
                    if (rowCells[i] == SquareType.BLANK)
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

        private CellCommand CreateCellCommand(int x, int y, SquareType newType)
        {
            SquareType oldType = grid.GetCell(x, y);
            CellCommand c = new(x, y, grid, newType, oldType);
            return c;
        }

        /// <summary>
        /// Undoes the last move (if any). Silently returns if there is no command to undo.
        /// </summary>
        public void Undo()
        {
            if (!CanUndo) return;

            Command c = undoStack.Last!.Value; // We know for sure that this isn't null as non-empty
            undoStack.RemoveLast();
            c.Undo();
            redoStack.AddLast(c);
        }

        /// <summary>
        /// Redo the last undone move (if any). Silently returns if there is no command to redo.
        /// </summary>
        public void Redo()
        {
            if (!CanRedo) return;

            Command c = redoStack.Last!.Value;
            redoStack.RemoveLast();
            c.Execute();
            undoStack.AddLast(c);
        }

        public bool IsSquareEmpty(int x, int y)
        {
            return grid.GetCell(x, y) == SquareType.BLANK;
        }

        public bool IsSquareFilled(int x, int y)
        {
            return grid.GetCell(x, y) == SquareType.FILLED;
        }

        public bool IsSquareCrossed(int x, int y)
        {
            return grid.GetCell(x, y) == SquareType.CROSS;
        }

        public bool IsPuzzleSolved()
        {
            return grid.IsSolved();
        }

        public override String ToString() { return grid.ToString(); }
    }
}
