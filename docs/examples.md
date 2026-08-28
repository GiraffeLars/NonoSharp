# Examples
## Creating a random puzzle and basic cell operations
The following example creates a new puzzle with a width of 5 and a height of 10.
It then fills, crosses and clears the cell located at (0, 0).
```csharp
using NonoSharp;

NonogramAPI nonogram = NonogramAPI.CreateRandomPuzzle(5, 10);
nonogram.FillCell(0, 0);
nonogram.CrossCell(0, 0);
nonogram.EmptyCell(0, 0);
```

## Using Events
The following example uses the @"NonoSharp.NonogramAPI.CellStateChanged"
and @"NonoSharp.NonogramAPI.PuzzleSolved" events to get information
about the changing puzzle state.
```csharp
using NonoSharp;

NonogramAPI api = NonogramAPI.CreateRandomPuzzle(5, 5);
api.CellStateChanged += (s, e) =>
{
    Console.WriteLine("A cell has changed states!");

    // Checks if the first cell in e.Cells is now filled
    CellPosition cell = e.Cells[0];
    Console.WriteLine($"First cell filled: {api.IsCellFilled(cell.X, cell.Y)}");
};

api.PuzzleSolved += (s, e) =>
{
    Console.WriteLine("The puzzle has been solved!");
};

api.FillCell(1, 1);
```

## Saving/Loading solutions
The following code creates a random puzzle, saves it to disk
and then loads the [example puzzle](example_puzzle.ns).
```csharp
using NonoSharp;
NonogramAPI api = NonogramAPI.CreateRandomPuzzle(5, 5);

// Saves the puzzle solution to ./save_puzzle_example.ns
//      (note that this might be in the bin folder when running the code)
// The title of the puzzle is "Example". This field can be left empty to not add a title
Console.WriteLine("Saving puzzle...");
api.SaveAsFile("save_puzzle_example.ns", "Example");
Console.WriteLine("Puzzle successfully saved!");
// Do not forget to catch the exceptions in your code! They have been removed in the example
// to save space

// Loads the example_puzzle.ns file. Note that this might be loaded from the bin folder 
// when running the code.
Console.WriteLine("Loading Example puzzle...");
api = NonogramAPI.LoadPuzzle("example_puzzle.ns");
Console.WriteLine("Successfully loaded puzzle!");
// Once again, do not forget to catch thrown exceptions.
```

## Using NonogramBuilder
With @"NonoSharp.NonogramBuilder", you can create your own puzzles and convert them to a @"NonoSharp.NonogramAPI" instance
or save them to disk! The example below creates a 5×5 puzzle with a smiley face as solution and saves it to a file.
```csharp
using NonoSharp;
NonogramBuilder builder = new(5, 5);

// Construct custom solution
builder.FillCell(1, 0); builder.FillCell(3, 0);

builder.FillCell(1, 1); builder.FillCell(3, 1);

builder.FillCell(0, 3); builder.FillCell(4, 3);

builder.FillCell(0, 4); builder.FillCell(1, 4); builder.FillCell(2, 4); 
builder.FillCell(3, 4); builder.FillCell(4, 4);

// Save the constructed solution. Do not forget to catch the exceptions!
builder.SaveAsFile("builder_example.ns");
```