using NonoSharp;

Nonogram nonogram = Nonogram.CreateRandomPuzzle(5, 10);
nonogram.FillCell(0, 0);
nonogram.CrossCell(0, 0);
nonogram.EmptyCell(0, 0);
