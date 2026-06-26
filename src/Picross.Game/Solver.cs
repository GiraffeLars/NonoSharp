using System;
using System.Collections.Generic;
using System.Text;

namespace Picross.Game
{
    /// <summary>
    /// Helper class to determine whether a puzzle can be solved,
    /// and whether the puzzle can be improved with 100% certainty.
    /// At its current state, the Solver will not make any "guesses", meaning that puzzles with a valid
    /// solution that might require logical deductions might be rejected.
    /// </summary>
    internal class Solver
    {
        // The playing grid of the user, should generally not be modified
        private Grid grid;

        // Grid to work on to calculate solutions (Copy of grid).
        private Grid workingGrid; 

        internal Solver(Grid grid)
        {
            this.grid = grid;
            this.workingGrid = (Grid) grid.Clone();
        }

        /// <summary>
        /// Determines whether a puzzle can be solved
        /// </summary>
        /// <returns>True if the puzzle can be solved, false if not</returns>
        public bool IsSolvable()
        {
            throw new NotImplementedException();
        }


        public bool CanColumnImprove(int index)
        {
            LinkedList<SquareType> column = workingGrid.GetColumn(index);
            return CanLineImprove(column, workingGrid.VerticalHints[index]);
        }

        public bool CanRowImprove(int index)
        {
            LinkedList<SquareType> row = workingGrid.GetRow(index);
            return CanLineImprove(row, workingGrid.HorizontalHints[index]);
        }

        private bool CanLineImprove(LinkedList<SquareType> line, Hints hints)
        {
            throw new NotImplementedException();
        }
    }
}
