# NonoSharp
[![NuGet Version](https://img.shields.io/nuget/vpre/NonoSharp?label=NuGet)](https://www.nuget.org/packages/NonoSharp/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/NonoSharp?label=NuGet%20Downloads
)](https://www.nuget.org/packages/NonoSharp/)
[![Build and Test API project (latest commit)](https://github.com/GiraffeLars/NonoSharp/actions/workflows/test-api.yml/badge.svg)](https://github.com/GiraffeLars/NonoSharp/actions/workflows/test-api.yml)

A Nonogram API built with C# featuring support for randomly generated puzzles and saving/loading pre-made puzzle solutions.

> Status: Published on [NuGet](https://www.nuget.org/packages/NonoSharp/) as an initial development release (v0.\*.\*).

<p>
    <img src="docs/PicrossGame.png" alt="Nonogram Puzzle being solved" width="500"/>
    <em><br>Example of a project built on top of NonoSharp, <a href="https://github.com/GiraffeLars/NonoSharp-Maui">NonoSharp-Maui</a>. </em>
</p>

## What is NonoSharp?
NonoSharp is an API for C#, together with an [example UI consumer](https://github.com/GiraffeLars/NonoSharp-Maui), allowing for easy creation and playing of [Nonogram](https://en.wikipedia.org/wiki/Nonogram) (also known as Picross) puzzles.
Nonograms are Japanese puzzles where you fill in a picture based on hints given to you.
The hints, either on the left-side or top-side of the grid, show how many groups there are in a given row/column and show how many cells each group consists of.
By filling the grid one cell at a time, eventually you reach the solution.

## Features
- **A fully functional Nonogram game**, complete with hint checking
- An **API** allowing for game logic to be reused in other projects
- A **custom solver** allowing you to solve any Nonogram puzzle you wish
- **Randomly generated puzzles** guaranteed to be uniquely solvable as verified by the built-in solver
- **Saving and loading solutions** to/from custom file format optimised for file size
- A **Nonogram builder** allowing you to create your own Nonogram puzzles quickly and easily

## Using the API
Since the core logic is separate from the UI, it can be reused in other projects. To add the API
to your project, you can install it from [NuGet](https://www.nuget.org/packages/NonoSharp),
for example by running the following command. This will install the latest version and add it to
your project.
```shell
dotnet add package NonoSharp
```

Currently supported functions include, but are not limited to:
- Abstracted grid, making it easy to implement in your projects
- Built-in undo/redo functionality
- Checking whether the puzzle is solved
- A hint system, together with whether a hint is completed by the user.
- Events for cells changing states and the puzzle being solved correctly
- Generating random uniquely solvable puzzles
- Loading and saving puzzles to a custom file type
- Custom solver for Nonogram puzzles
- Builder to create your own Nonogram puzzles

### Documentation
Documentation for the API is found on this repo's GitHub pages, 
[here](https://giraffelars.github.io/NonoSharp/).

### Example usage
```csharp
using NonoSharp;
 
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
};
```

## Getting Started
### Playing
To play a Nonogram game built on top of NonoSharp, install the beta release for Windows in the [Releases](https://github.com/GiraffeLars/NonoSharp-Maui) tab of the [NonoSharp-Maui GitHub Repo](https://github.com/GiraffeLars/NonoSharp-Maui), or create your own!

### Contributing
To contribute, clone the project and open it in your prefered IDE, such as Visual Studio. The project makes use of [.NET 10.0](https://dotnet.microsoft.com/en-us/download/dotnet/10.0).

### Building the project
After setting everything up, you can follow the regular workflow for building .NET projects:
```
dotnet build
```
This will build the API project and the unit tests.

If you wish to build just the API, run
```
dotnet build src/NonoSharp/NonoSharp.csproj
```

Of course, you are also welcome to use your IDE's debugger to build the project.

### Unit tests
The API project is paired with a test suite found in `tests/NonoSharp.Tests`. To run the tests, either use your IDE's unit testing features or run the following:
```
dotnet test tests/NonoSharp.Tests/NonoSharp.Tests.csproj
```
When contributing, please ensure that the unit tests all pass. These will also be checked when opening a pull request.

## Roadmap
Features that are currently planned to be added *(in no particular order)*:
- [x] Random puzzle generation that have a guaranteed solution
- [x] Automatically cross the remaining blank cells upon line completion
- [x] Support for pre-made puzzles
- [x] Player-created puzzles and puzzle creator
- [x] Settings for consumers, such as toggling auto crosses or enabling automatic correction when a cell was filled incorrectly
- [x] Optimise Solver used for random puzzle generation and make it available publicly
- [ ] Convert pictures to Nonograms
- [ ] Getting hints when stuck solving a puzzle

## Contribution guidelines
This project started as a solo learning project, but contributions are welcome. Please open a PR or an issue if you wish to contribute. 

When submitting a pull request, please make note of the following:
- No AI generated code! 
- Keep PRs focussed
- If you make any changes to the logic, ensure that the tests verify
- Make sure the project builds and functions as intended
- Keep code documented

## License
This project is licensed under the **MIT License**. See the `LICENSE` file.
