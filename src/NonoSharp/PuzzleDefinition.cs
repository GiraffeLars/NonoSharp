using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Text;

using NonoSharp.Exceptions;

namespace NonoSharp
{
    internal class PuzzleDefinition
    {
        /// <summary>
        /// Title of the puzzle
        /// </summary>
        public string? Title { get; internal set; }
        public const int MAX_TITLE_LENGTH = 100;
        public int Width { get; private set; }
        public int Height { get; private set; }

        public FrozenSet<CellPosition>? Solution { get; protected set; }

        /// <summary>
        /// Creates a puzzle definition
        /// </summary>
        /// <param name="width">Width of the puzzle</param>
        /// <param name="height">Height of the puzzle</param>
        /// <param name="solution">The solution of the puzzle</param>
        /// <param name="title">Optional title of the puzzle</param>
        internal PuzzleDefinition(int width, int height, IEnumerable<CellPosition>? solution, string? title = null)
        {
            this.Width = width;
            this.Height = height;
            this.Solution = solution?.ToFrozenSet();
            this.Title = title;
        }

        /// <summary>
        /// Determines if (<paramref name="x"/>, <paramref name="y"/>) is expected to be filled in the solution.
        /// </summary>
        /// <param name="x">x-coordinate to check.</param>
        /// <param name="y">y-coordinate to check.</param>
        /// <returns><c>true</c> if cell is expected to be filled in the solution, <c>false</c> otherwise.</returns>
        internal bool GetSolutionAt(int x, int y)
        {
            if (Solution == null) throw new InvalidOperationException("There is no solution for this puzzle!");
            return Solution.Contains(new(x, y));
        }

        /// <summary>
        /// Sets the dimensions of this puzzle definition. This is done by adding new empty cells to the right and bottom.
        /// Cannot shrink the puzzle.
        /// </summary>
        /// <param name="width">New width for the puzzle</param>
        /// <param name="height">New height for the puzzle</param>
        /// <exception cref="OverflowException">When <paramref name="width"/> * <paramref name="height"/> causes overflow</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="width"/> or <paramref name="height"/> are
        /// less than their respective old value</exception>
        /// <exception cref="ArgumentException">When <paramref name="width"/> is less than the old width or <paramref name="height"/>
        /// is less than the old height</exception>
        internal void SetDimensions(int width, int height)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(width, Width, nameof(width));
            ArgumentOutOfRangeException.ThrowIfLessThan(height, Height, nameof(height));
            Width = width;
            Height = height;
        }
    }
}
