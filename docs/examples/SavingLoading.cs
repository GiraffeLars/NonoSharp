using NonoSharp;
Nonogram nonogram = Nonogram.CreateRandomPuzzle(5, 5);

// Saves the puzzle solution to ./save_puzzle_example.ns
//      (note that this might be in the bin folder when running the code)
// The title of the puzzle is "Example". This field can be left empty to not add a title
Console.WriteLine("Saving puzzle...");
byte[] serialized = NonogramSerializer.Serialize(nonogram);
File.WriteAllBytes("save_puzzle_example.ns", serialized);
Console.WriteLine("Puzzle successfully saved!");
// Do not forget to catch the exceptions in your code! They have been removed in the example
// to save space

// Loads the example_puzzle.ns file. Note that this might be loaded from the bin folder 
// when running the code.
Console.WriteLine("Loading Example puzzle...");
nonogram = NonogramSerializer.Deserialize(File.OpenRead("example_puzzle.ns"));
Console.WriteLine("Successfully loaded puzzle!");
// Once again, do not forget to catch thrown exceptions.
