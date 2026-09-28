# NonoSharp
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

## Source code
The full repository can be found on [GitHub](https://github.com/GiraffeLars/NonoSharp). 
NonoSharp is paired with an example UI consumer built on top of .NET MAUI, 
found on [the GitHub NonoSharp-Maui repo](https://github.com/GiraffeLars/NonoSharp).

## License
This project is licensed under the **MIT License**. See the `LICENSE` file on the GitHub repo for more information.
