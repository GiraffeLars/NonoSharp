using System;
using System.Collections.Generic;
using System.Text;

namespace NonoSharp
{
    /// <summary>
    /// Clues that are attached to a puzzle grid and are thus responsible for the clues of a singular
    /// line on the puzzle.
    /// </summary>
    internal class PuzzleClues : Clues
    {
        [Obsolete("Remove when Hints get removed")]
        internal bool? colClues { get { return isColumnClues; } }
        [Obsolete("Remove when Hints get removed")]
        internal int? pos { get { return position; } }
        private bool isColumnClues;
        private int position;

        internal PuzzleClues(bool isColumnClues, int position) : base()
        {
            this.isColumnClues = isColumnClues;
            this.position = position;
        }

        internal PuzzleClues(bool isColumnClues, int position, List<Clue> clues) : base(clues)
        {
            this.isColumnClues = isColumnClues;
            this.position = position;
        }

        internal void DoCompletion(Grid grid)
        {
            if (clues.Count == 0) return;
            Reset();
            LinkedList<CellType> line = isColumnClues ? grid.GetColumn(position) : grid.GetRow(position);
            LinkedListNode<CellType>? node = line.First;

            int leftOffAt = DoCompletionForward(line);
            DoCompletionBackward(line, leftOffAt);

            SetFullyCompleted();
        }
    }
}
