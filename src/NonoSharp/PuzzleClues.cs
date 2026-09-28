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
        internal bool IsColumnClues { get; }
        internal int Position { get; }

        internal PuzzleClues(bool isColumnClues, int position) : base()
        {
            this.IsColumnClues = isColumnClues;
            this.Position = position;
        }

        internal PuzzleClues(bool isColumnClues, int position, List<Clue> clues) : base(clues)
        {
            this.IsColumnClues = isColumnClues;
            this.Position = position;
        }

        internal void DoCompletion(Grid grid)
        {
            if (clues.Count == 0) return;
            Reset();
            LinkedList<CellType> line = IsColumnClues ? grid.GetColumn(Position) : grid.GetRow(Position);
            LinkedListNode<CellType>? node = line.First;

            int leftOffAt = DoCompletionForward(line);
            DoCompletionBackward(line, leftOffAt);

            SetFullyCompleted();
        }

        internal static PuzzleClues[] ArrayFromClues(Clues[] clues, bool isColumnClues)
        {
            return [..Enumerable.Range(0, clues.Length)
                .Select(i => 
                    new PuzzleClues(
                        isColumnClues,
                        i,
                        [.. clues[i].Select(clue => new Clue(clue.Number))]
                    )
                )
            ];
        }
    }
}
