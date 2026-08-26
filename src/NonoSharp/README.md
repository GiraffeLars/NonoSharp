# NonoSharp
A feature-rich Nonogram API for C#, featuring pre-made and randomly generated puzzles and custom puzzle saving/loading.

## What is NonoSharp?
NonoSharp is an API for C#, allowing for easy creation and playing of [Nonogram](https://en.wikipedia.org/wiki/Nonogram) (also known as Picross) puzzles.
Nonograms are Japanese puzzles where you fill in a picture based on hints given to you.
The hints, either on the left-side or top-side of the grid, show how many groups there are in a given row/column and show how many cells each group consists of.
By filling the grid one cell at a time, eventually you reach the solution.

## Features
- **A fully functional Nonogram game**, complete with hint checking
- An **API** allowing for game logic to be reused in other projects
- **Randomly generated puzzles** guaranteed to be uniquely solvable as verified by the built-in solver
- **Saving and loading solutions** to/from custom file format

## The API
### Supported features
Currently supported functions include:
- Abstracted grid, making it easy to implement in your projects
- Built-in undo/redo functionality
- Checking whether the puzzle is solved
- A hint system, together with whether a hint is completed by the user.
- Events for cells changing states and the puzzle being solved correctly
- Generating random uniquely solvable puzzles
- Loading and saving puzzles to a custom file type

## Documentation
Documentation for the API is found on GitHub pages for the corresponding repo, 
[here](https://giraffelars.github.io/NonoSharp/).

### Example usage
```csharp
using NonoSharp;
using NonoSharp.Events;
 
// Creates a new random 10x10 puzzle. Generation is guaranteed to produce a solvable puzzle.
// This method is also available asynchronously via NonogramAPI.CreateRandomPuzzleAsync
var game = NonogramAPI.CreateRandomPuzzle(10, 10); // (width x height)
 
// Fill in or cross a cell (coordinates are zero-indexed, (0, 0) is top-left)
game.FillCell(2, 3);
game.CrossCell(0, 0);
 
// Moves can be undone/redone
if (game.CanUndo)
{
    game.Undo();
}
 
// Check individual cell state
bool isFilled = game.IsCellFilled(2, 3);
 
// Check overall progress
if (game.IsPuzzleSolved())
{
    Console.WriteLine("Solved!");
} 
else 
{
    Console.WriteLine("Not solved :(");
}

 
// The hints shown alongside the grid (e.g. "3 1" for a row) are available for building your own UI
Hints[] columnHints = game.ColumnHints;
Hints[] rowHints = game.RowHints;

// There are also some events provided
game.CellStateChanged += (s, e) => {
    Console.WriteLine("A cell has changed states");

    // Include using NonoSharp.Events to gain access to the event args
};
```

## Full repo
The full repo can be found on [GitHub](https://github.com/GiraffeLars/NonoSharp). 
NonoSharp is paired with an example UI consumer built on top of .NET MAUI, 
found on [the GitHub NonoSharp-Maui repo](https://github.com/GiraffeLars/NonoSharp).

## License
This project is licensed under the **MIT License**. See the `LICENSE` file on the GitHub repo for more information.
