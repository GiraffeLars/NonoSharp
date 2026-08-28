using System;
using System.Collections.Generic;
using System.Text;

namespace NonoSharp
{
    /// <summary>
    /// A record containing a cell's position
    /// </summary>
    public readonly record struct CellPosition
    {
        /// <summary>
        /// X-coordinate of a cell on the puzzle grid
        /// </summary>
        public readonly int X;
        /// <summary>
        /// Y-coordinate of a cell on the puzzle grid
        /// </summary>
        public readonly int Y;

        /// <summary>
        /// Creates a CellPosition with the given coordinates
        /// </summary>
        /// <param name="x">X-coordinate of the cell</param>
        /// <param name="y">Y-coordinate of the cell</param>
        public CellPosition(int x, int y)
        {
            X = x; Y = y;
        }

        /// <summary>
        /// Returns a string in the form of "(X, Y)"
        /// </summary>
        /// <returns>String as above</returns>
        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }
}
