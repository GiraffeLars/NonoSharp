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
[!code-csharp[](examples/GettingStarted.cs#L1-L3)]

### Basic cell operations
Of course, to enjoy Nonograms one also needs to manipulate cells. NonoSharp provides a simple API for this, allowing you to fill, cross and empty cells.
Continuing on from the previous example, we can fill, cross and empty the cell at (0, 0) as follows:
[!code-csharp[](examples/GettingStarted.cs#L4-L6)]

The indexer is also available:
[!code-csharp[](examples/GettingStarted.cs#L8)]

or use GetCell and SetCell directly:
[!code-csharp[](examples/GettingStarted.cs#L10-11)]

### More examples
More examples are available on the [examples](examples.md) page, including how to use events, how to use the solver and how to save/load puzzles.
