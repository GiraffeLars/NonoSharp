using NonoSharp;

Nonogram nonogram = Nonogram.CreateRandomPuzzle(5, 5);
nonogram.FillCell(0, 0);
nonogram.CrossCell(0, 0);
nonogram.EmptyCell(0, 0);

nonogram[0, 0] = CellType.Filled;

nonogram.SetCell(0, 0, CellType.Filled);
CellType cell = nonogram.GetCell(0, 0);

// When changing this file, keep in mind that getting-started.md uses code snippets as lines from this file.
// Change the snippets accordingly.
