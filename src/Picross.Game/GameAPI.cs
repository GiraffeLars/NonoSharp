namespace Picross.Game
{
    public class GameAPI
    {
        Grid grid;

        public int Width { get { return grid.Width; } }
        public int Height { get { return grid.Height; } }
        public Hints[] VerticalHints { get { return grid.VerticalHints; } }
        public Hints[] HorizontalHints { get { return grid.HorizontalHints; } }
        public bool CanUndo { get { return undoStack.Count != 0; } }
        public bool CanRedo { get { return redoStack.Count != 0; } }

        private readonly LinkedList<Command> undoStack;
        private readonly LinkedList<Command> redoStack;

        public GameAPI(int width, int height) {
            grid = new Grid(width, height);
            undoStack = new LinkedList<Command>();
            redoStack = new LinkedList<Command>();
        }

        public void FillCell(int x, int y)
        {
            Command command = CreateCellCommand(x, y, SquareType.FILLED);
            ExecuteCommand(command);
        }

        public void CrossCell(int x, int y)
        {
            Command command = CreateCellCommand(x, y, SquareType.CROSS);
            ExecuteCommand(command);
        }

        public void EmptyCell(int x, int y)
        {
            Command command = CreateCellCommand(x, y, SquareType.BLANK);
            ExecuteCommand(command);
        }

        private void ExecuteCommand(Command command)
        {
            command.Execute();
            undoStack.AddLast(command);
            redoStack.Clear();
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
