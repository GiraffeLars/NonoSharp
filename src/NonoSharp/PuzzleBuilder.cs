using System;
using System.Collections.Generic;
using System.Text;

namespace NonoSharp
{
    /// <summary>
    /// Builder class to create a custom Nonogram puzzle, and can at the final stage
    /// be obtained as a <see cref="NonogramAPI"/> instance or be saved to a file.
    /// </summary>
    public class PuzzleBuilder
    {
        /// <summary>
        /// Height of the grid that is being created
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// Height of the puzzle that is being created
        /// </summary>
        public int Height { get; private set; }

        private PuzzleDefinition puzzle;

        /// <summary>
        /// Creates a new PuzzleBuilder
        /// </summary>
        /// <param name="width">Width for the puzzle to create with this PuzzleBuilder</param>
        /// <param name="height">Height for the puzzle to create with this PuzzleBuilder</param>
        /// <param name="title">Optional title to give the puzzle</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="width"/> or <paramref name="height"/>
        /// is non-positive (&lt;= 0)</exception>
        public PuzzleBuilder(int width, int height, string? title = null)
        {
            ValidateDimensions(width, height);
            Width = width;
            Height = height;

            bool[] solution = new bool[width * height];
            puzzle = new PuzzleDefinition(width, height, solution);
        }

        /// <summary>
        /// Checks if Cell at (<paramref name="x"/>, <paramref name="y"/>) is expected to be filled in the solution
        /// </summary>
        /// <param name="x">x-coordinate of cell to check</param>
        /// <param name="y">y-coordinate of cell to check</param>
        /// <returns>True if the cell is expected to be filled, false otherwise</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="x"/> or <paramref name="y"/> is not within the
        /// puzzle bounds </exception>
        public bool IsCellFilled(int x, int y)
        {
            ValidateCoordinates(x, y);
            return puzzle.GetSolutionAt(x, y);
        }

        /// <summary>
        /// Checks if Cell at <paramref name="position"/> is expected to be filled in the solution
        /// </summary>
        /// <param name="position">CellPosition to check</param>
        /// <returns>True if the cell is expected to be filled, false otherwise</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="x"/> or <paramref name="y"/> is not within the
        /// puzzle bounds </exception>
        public bool IsCellFilled(CellPosition position)
        {
            return IsCellFilled(position.X, position.Y);
        }

        /// <summary>
        /// Checks if Cell at (<paramref name="x"/>, <paramref name="y"/>) is expected to be empty in the solution
        /// </summary>
        /// <param name="x">x-coordinate of cell to check</param>
        /// <param name="y">y-coordinate of cell to check</param>
        /// <returns>True if the cell is expected to be empty, false otherwise</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="x"/> or <paramref name="y"/> is not within the
        /// puzzle bounds </exception>
        public bool IsCellEmpty(int x, int y)
        {
            ValidateCoordinates(x, y);
            return !puzzle.GetSolutionAt(x, y);
        }

        /// <summary>
        /// Checks if Cell at <paramref name="position"/> is expected to be filled in the solution
        /// </summary>
        /// <param name="position">CellPosition to check</param>
        /// <returns>True if the cell is expected to be filled, false otherwise</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="x"/> or <paramref name="y"/> is not within the
        /// puzzle bounds </exception>
        public bool IsCellEmpty(CellPosition position)
        {
            return IsCellEmpty(position.X, position.Y);
        }

        /// <summary>
        /// Validates the dimensions given
        /// </summary>
        /// <param name="width">Width to check</param>
        /// <param name="height">Height to check</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="width"/> or <paramref name="height"/>
        /// is non-positive (&lt;= 0)</exception>
        private static void ValidateDimensions(int width, int height)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(width, 0, nameof(width));
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(height, 0, nameof(height));
        }

        private void ValidateCoordinates(int x, int y)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(x, 0, nameof(x));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(x, Width, nameof(x));

            ArgumentOutOfRangeException.ThrowIfLessThan(y, 0, nameof(y));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(y, Height, nameof(y));
        }
    }
}
