# Getting Started
## Installing NonoSharp
To install NonoSharp, all you have to do is install the package from [NuGet](https://www.nuget.org/packages/NonoSharp)!
<br>This can be done by running the following command in your terminal, or using a package manager,
such as Visual Studio's NuGet Package Manager
```shell
dotnet add package NonoSharp
```

## Using NonoSharp
After installing, you are already ready to go! You can browse through the [API documentation](xref:NonoSharp) or take a look at the example code below to get started.

### Creating a first puzzle
NonoSharp makes it easy to create a new puzzle. You can either create a random puzzle, or load an existing one from a file.
Below is an example of creating a random 5x5 puzzle.
```csharp
using NonoSharp;

Nonogram nonogram = Nonogram.CreateRandomPuzzle(5, 5);
```

### Basic cell operations
Of course, to enjoy Nonograms one also needs to manipulate cells. NonoSharp provides a simple API for this, allowing you to fill, cross and empty cells.
Continuing on from the previous example, we can fill, cross and empty the cell at (0, 0) as follows:
```csharp
nonogram.FillCell(0, 0);
nonogram.CrossCell(0, 0);
nonogram.EmptyCell(0, 0);
```

The indexer is also available:
```csharp
nonogram[0, 0] = CellType.Filled;
```
or use GetCell and SetCell directly:
```csharp
nonogram.SetCell(0, 0, CellType.Filled);
CellType cell = nonogram.GetCell(0, 0);
```

### More examples
More examples are available on the [examples](examples.md) page, including how to use events, how to use the solver and how to save/load puzzles.