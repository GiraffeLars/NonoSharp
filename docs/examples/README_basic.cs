using NonoSharp;

// Creates a new random 10x10 puzzle. Generation is guaranteed to produce a solvable puzzle.
// This method is also available asynchronously via Nonogram.CreateRandomPuzzleAsync
var nonogram = Nonogram.CreateRandomPuzzle(10, 10); // (width x height)

// Fill in or cross a cell (coordinates are zero-indexed, (0, 0) is top-left)
nonogram.FillCell(2, 3);
nonogram.CrossCell(0, 0);

// Moves can be undone/redone
if (nonogram.CanUndo)
{
    nonogram.Undo();
}

// Check individual cell state
bool isFilled = nonogram.IsCellFilled(2, 3);

// Check overall progress
if (nonogram.IsPuzzleSolved())
{
    Console.WriteLine("Solved!");
}
else
{
    Console.WriteLine("Not solved :(");
}


// The clues shown alongside the grid (e.g. "3 1" for a row) are available for building your own UI
Clues[] columnClues = nonogram.ColumnClues;
Clues[] rowClues = nonogram.RowClues;

// There are also some events provided
nonogram.CellStateChanged += (s, e) => {
    Console.WriteLine("A cell has changed states");
};
