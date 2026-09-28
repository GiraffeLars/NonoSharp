# NonoSharp
A feature-rich Nonogram library for .NET10.0 featuring Nonogram puzzle playing, randomly generated puzzles, easy Nonogram creation, saving/loading pre-made puzzle solutions and a custom Nonogram solver.

## What is NonoSharp?
NonoSharp is an API for C#, allowing for easy creation and playing of [Nonogram](https://en.wikipedia.org/wiki/Nonogram) (also known as Picross) puzzles.
Nonograms are Japanese puzzles where you fill in a picture based on clues given to you.
The clues, either on the left-side or top-side of the grid, show how many groups there are in a given row/column and show how many cells each group consists of.
By filling the grid one cell at a time, eventually you reach the solution.

Currently the library is split into the following components:
- **Nonogram puzzle playing**: Generate, load, save and play Nonogram puzzles, just like you would for any other Nonogram game.
- **Nonogram solving**: Solve any Nonogram puzzle you might encounter, either by using the built-in solver.
- **Nonogram creation**: Create your own Nonogram puzzles, either by using the Nonogram builder.


## Features
- **A fully functional Nonogram game**, complete with clue checking and various other features
- An abstracted **Nonogram API** allowing for game logic to be reused in other projects
- **Custom Solver** to solve any Nonogram puzzle you might encounter
- **Randomly generated puzzles** guaranteed to be uniquely solvable as verified by the built-in solver
- **Custom file format** optimised for file size to load and save puzzles
- A **Nonogram builder** allowing you to create your own Nonogram puzzles quickly and easily
- And more!


## Documentation
Documentation for the API is found on GitHub pages for the corresponding repo, 
[here](https://giraffelars.github.io/NonoSharp/).

## Example usage
### Nonogram playing
```csharp
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
```

### Nonogram creation
NonoSharp's `NonogramBuilder` class allows you to create your own Nonogram puzzles quickly and easily, while
ensuring puzzles remain uniquely solvable.
```csharp
using NonoSharp;

NonogramBuilder builder = NonogramBuilder.FromString(
    " O O \n" +
    "     \n" +
    "     \n" +
    "O   O\n" +
    "OOOOO",
    "Smiley");

if (builder.IsSolvable())
{
    Nonogram puzzle = builder.ToNonogram();
    puzzle.SaveAsFile("smiley.ns");
}
```

### Solving Nonograms
As mentioned, NonoSharp comes with a custom Nonogram solver. This can be used to solve any Nonogram puzzle.
```csharp
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

var solvable = Solver.IsSolvable(3, 3, columnClues, rowClues, out var solution);
```


## Source code
The full repository can be found on [GitHub](https://github.com/GiraffeLars/NonoSharp). 
NonoSharp is paired with an example UI consumer built on top of .NET MAUI, 
found on [the GitHub NonoSharp-Maui repo](https://github.com/GiraffeLars/NonoSharp).

## License
This project is licensed under the **MIT License**. See the `LICENSE` file included in the package or on the GitHub repo for more information.
