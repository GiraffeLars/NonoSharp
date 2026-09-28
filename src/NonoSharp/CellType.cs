using System;
using System.Collections.Generic;
using System.Text;

namespace NonoSharp
{
    /// <summary>
    /// Possible types of a cell on the puzzle grid.
    /// </summary>
    public enum CellType
    {
        /// <summary>
        /// Represents an empty cell.
        /// </summary>
        Empty,

        /// <summary>
        /// Represents a filled cell.
        /// </summary>
        Filled,

        /// <summary>
        /// Represents a cell that is crossed.
        /// </summary>
        Cross
    }

}
