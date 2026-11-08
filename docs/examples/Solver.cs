using NonoSharp;

Clues[] columnClues =
[
    new Clues(3),
    new Clues(1),
    new Clues(3),
];
Clues[] rowClues =
[
    new Clues(3),
    new Clues(1, 1),
    new Clues(1, 1),
];

var solvable = Solver.IsSolvable(columnClues, rowClues, out var solution);
