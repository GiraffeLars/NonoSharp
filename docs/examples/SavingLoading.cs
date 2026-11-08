using NonoSharp;
Nonogram nonogram = Nonogram.CreateRandomPuzzle(5, 5);

// Saves the puzzle solution to ./save_puzzle_example.ns
//      (note that this might be in the bin folder when running the code)
// The title of the puzzle is "Example". This field can be left empty to not add a title
Console.WriteLine("Saving puzzle...");
nonogram.SaveAsFile("save_puzzle_example.ns", "Example");
Console.WriteLine("Puzzle successfully saved!");
// Do not forget to catch the exceptions in your code! They have been removed in the example
// to save space

// Loads the example_puzzle.ns file. Note that this might be loaded from the bin folder 
// when running the code.
Console.WriteLine("Loading Example puzzle...");
nonogram = Nonogram.LoadPuzzle("example_puzzle.ns");
Console.WriteLine("Successfully loaded puzzle!");
// Once again, do not forget to catch thrown exceptions.
