using System;
using System.Collections.Generic;
using System.Text;

namespace NonoSharp.Events
{
    /// <summary>
    /// The arguments for the Cell Corrected event
    /// </summary>
    public class CorrectionEventArgs : EventArgs
    {
        /// <summary>
        /// Position of cell that was corrected
        /// </summary>
        public CellPosition Cell { get; }

        /// <summary>
        /// The type this cell was before the correction
        /// </summary>
        public CellType Before { get;}

        /// <summary>
        /// The type this cell is now, after the correction
        /// </summary>
        public CellType After { get;}

        /// <summary>
        /// Constructs a CorrectionEventArgs
        /// </summary>
        /// <param name="cell">Cell position that was changed</param>
        /// <param name="before">Cell state before correction, usually this is the state to change to before correction</param>
        /// <param name="after">Corrected state of cell</param>
        internal CorrectionEventArgs(CellPosition cell, CellType before, CellType after)
        {
            Cell = cell;
            Before = before;
            After = after;
        }
    }
}
