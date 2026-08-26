# Introduction
## What is NonoSharp?
NonoSharp is an API for C# and .NET10.0, allowing for easy creation and playing of [Nonogram](https://en.wikipedia.org/wiki/Nonogram) (also known as Picross) puzzles.
Nonograms are Japanese puzzles where you fill in a picture based on hints given to you.
The hints, either on the left-side or top-side of the grid, show how many groups there are in a given row/column and show how many cells each group consists of.
By filling the grid one cell at a time, eventually you reach the solution.

## Features
- **A fully functional Nonogram game**, complete with hint checking
- An **API** allowing for game logic to be reused in other projects
- **Randomly generated puzzles** guaranteed to be uniquely solvable as verified by the built-in solver

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

## Full repo
The full repo can be found on [GitHub](https://github.com/GiraffeLars/NonoSharp).
NonoSharp is paired with an example UI consumer built on top of .NET MAUI, 
found on [the GitHub NonoSharp-Maui repo](https://github.com/GiraffeLars/NonoSharp).

## License
This project is licensed under the **MIT License**. See the `LICENSE` file on the GitHub repo for more information.
