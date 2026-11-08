using NonoSharp;

Nonogram nonogram = Nonogram.CreateRandomPuzzle(5, 5);
nonogram.CellStateChanged += (s, e) =>
{
    Console.WriteLine("A cell has changed states!");

    // Checks if the first cell in e.Cells is now filled
    CellPosition cell = e.Cells[0];
    Console.WriteLine($"First cell filled: {nonogram.IsCellFilled(cell.X, cell.Y)}");
};

nonogram.PuzzleSolved += (s, e) =>
{
    Console.WriteLine("The puzzle has been solved!");
};

nonogram.FillCell(1, 1);
