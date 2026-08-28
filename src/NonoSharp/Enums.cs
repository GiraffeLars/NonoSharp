using System;
using System.Collections.Generic;
using System.Text;

namespace NonoSharp
{
    /// <summary>
    /// Possible states of a cell on the puzzle grid
    /// </summary>
    public enum CellType
    {
        /// <summary>
        /// Represents an empty cell
        /// </summary>
        BLANK,

        /// <summary>
        /// Represents a filled cell
        /// </summary>
        FILLED,

        /// <summary>
        /// Represents a cell that is crossed
        /// </summary>
        CROSS
    }

}
