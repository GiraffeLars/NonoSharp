using System;
using System.Collections.Generic;
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
        public List<CellPosition> Cells { get; internal init; }

        internal CellStateEventArgs(List<CellPosition> cells)
        {
            this.Cells = cells;
        }

    }
}
