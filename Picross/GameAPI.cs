namespace Core
{
    public class GameAPI
    {
        Grid grid;

        public int Width { get { return grid.width; } }
        public int Height { get { return grid.height; } }
        public Hints[] VerticalHints { get { return grid.verticalHints; } }
        public Hints[] HorizontalHints { get { return grid.horizontalHints; } }

        private LinkedList<Command> undoStack;
        private LinkedList<Command> redoStack;

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
            SquareType oldType = grid.getCell(x, y);
            CellCommand c = new CellCommand(x, y, grid, newType, oldType);
            return c;
        }

        public void Undo()
        {
            if (undoStack.Count == 0) return;

            Command c = undoStack.Last!.Value; // We know for sure that this isn't null
            undoStack.RemoveLast();
            c.Undo();
            redoStack.AddLast(c);
        }

        public void Redo()
        {
            if (redoStack.Count == 0) return;

            Command c = redoStack.Last!.Value;
            redoStack.RemoveLast();
            c.Execute();
            undoStack.AddLast(c);
        }

        public bool IsSquareEmpty(int x, int y)
        {
            return grid.getCell(x, y) == SquareType.BLANK;
        }

        public bool IsSquareFilled(int x, int y)
        {
            return grid.getCell(x, y) == SquareType.FILLED;
        }

        public bool IsSquareCrossed(int x, int y)
        {
            return grid.getCell(x, y) == SquareType.CROSS;
        }

        public bool IsPuzzleSolved()
        {
            return grid.isCorrect();
        }

        public override String ToString() { return grid.ToString(); }
    }
}
