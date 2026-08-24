using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Picross.Game.Events
{
    /// <summary>
    /// The arguments for the cell state event.
    /// </summary>
    public class CellStateEventArgs
    {
        /// <summary>
        /// List of the cells that were changed
        /// </summary>
        public List<Point> Cells { get; internal init; }

        internal CellStateEventArgs(List<Point> cells)
        {
            this.Cells = cells;
        }

    }
}
