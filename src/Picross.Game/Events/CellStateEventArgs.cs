using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Picross.Game.Events
{
    /// <summary>
    /// The arguments for the cell state event.
    /// </summary>
    /// <param name="cells">The cells this event concerns (e.g. changed cells in event <c>CellStateChanged</c>.)</param>
    public class CellStateEventArgs(List<Point> cells)
    {
        public List<Point> Cells { get; } = cells;
    }
}
