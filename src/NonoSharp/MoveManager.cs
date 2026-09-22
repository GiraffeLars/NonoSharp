using NonoSharp.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace NonoSharp
{
    internal class MoveManager
    {
        internal NonogramOptions Options { get; set; } = new NonogramOptions();
        private readonly Puzzle puzzle;
        public HistoryManager History { get; }
        internal event EventHandler<CorrectionEventArgs>? CellCorrected;

        internal MoveManager(NonogramOptions? options, Puzzle puzzle)
        {
            this.puzzle = puzzle;
            Options = options ?? new NonogramOptions();
            History = new();
        }

        /// <summary>
        /// Executes the player's move. Changes the cell at (<paramref name="x"/>, <paramref name="y"/>) to 
        /// <paramref name="newType"/> and handles auto-crosses.
        /// The move is also appended to the undo-stack.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="newType"></param>
        /// <returns>List of CellPositions of the cells that were changed.</returns>
        public List<CellPosition> DoMove(int x, int y, CellType newType)
        {
            newType = DoAutoCorrect(x, y, newType);
            // Check if auto-crosses are possible (i.e. a cell goes to filled or from filled)
            bool mightAutoCross = newType == CellType.FILLED || puzzle.Grid.GetCell(x, y) == CellType.FILLED;

            CellCommand initialCommand = CreateCellCommand(x, y, newType);

            // Execute the command before determining auto-crosses
            initialCommand.Execute();

            if (!Options.EnableAutoCross || !mightAutoCross)
            {
                // Only push this command and send event if auto-cross is not possible and return
                History.PushCommand(initialCommand);
                return [.. initialCommand.GetChanges()];
            }

            // Get auto-cross command and execute
            CompositeCommand autoCrossCommand = GetAutoCrossCommand(x, y);

            if (autoCrossCommand.Count > 0)
            {
                autoCrossCommand.Execute();

                // Push the move and the auto-crosses to the stack together for one fluid undo for the player
                CompositeCommand combinedCommand = CompositeCommand.Combine(initialCommand, autoCrossCommand);
                History.PushCommand(combinedCommand);
                return [.. combinedCommand.GetChanges()];
            }
            else
            {
                // Don't execute auto cross command and only push the initial command since the auto cross command is empty
                History.PushCommand(initialCommand);
                return [.. initialCommand.GetChanges()];
            }
        }

        /// <summary>
        /// Handles auto-correct
        /// </summary>
        /// <param name="x">x-coordinate of cell changed</param>
        /// <param name="y">y-coordinate of cell changed</param>
        /// <param name="typeCellChangedTo">New type of cell</param>
        /// <returns>Corrected cell type if auto correct is enabled, <paramref name="typeCellChangedTo"/> otherwise</returns>
        private CellType DoAutoCorrect(int x, int y, CellType typeCellChangedTo)
        {
            if (!Options.EnableAutoCorrect || puzzle.Solution == null)
            {
                return typeCellChangedTo;
            }

            bool solutionHasCell = puzzle.Solution.Contains(new(x, y));
            if ((solutionHasCell && typeCellChangedTo == CellType.CROSS) || (!solutionHasCell && typeCellChangedTo == CellType.FILLED))
            {
                CellType invertedType = typeCellChangedTo == CellType.FILLED ? CellType.CROSS : CellType.FILLED;
                CorrectionEventArgs args = new(new(x, y), typeCellChangedTo, invertedType);
                OnCellCorrected(args);
                return invertedType;
            }
            return typeCellChangedTo;
        }

        /// <summary>
        /// Returns the CompositeCommand consisting of cross cell commands replacing blank cells after a line has been completed
        /// </summary>
        /// <param name="x">x coordinate of cell changed in current move</param>
        /// <param name="y">y coordinate of cell changed in current move</param>
        /// <returns>CompositeCommand as above</returns>
        private CompositeCommand GetAutoCrossCommand(int x, int y)
        {
            LinkedList<ICommand> autoCrossCommands = [];

            foreach (int i in GetColumnAutoCross(x))
            {
                ICommand cmd = CreateCellCommand(x, i, CellType.CROSS);
                autoCrossCommands.AddLast(cmd);
            }

            foreach (int i in GetRowAutoCross(y))
            {
                ICommand cmd = CreateCellCommand(i, y, CellType.CROSS);
                autoCrossCommands.AddLast(cmd);
            }

            return new CompositeCommand(autoCrossCommands);
        }

        private List<int> GetColumnAutoCross(int col)
        {
            LinkedList<int> groups = puzzle.Grid.GetGroupsInColumn(col);
            Clues clues = puzzle.ColumnClues[col];
            List<int> posToCross = [];

            bool groupsMatchClues = DoGroupsMatchClues(groups, clues);

            if (groupsMatchClues)
            {
                CellType[] column = puzzle.Grid.GetColumnArray(col);

                for (int i = 0; i < column.Length; i++)
                {
                    if (column[i] == CellType.BLANK)
                    {
                        posToCross.Add(i);
                    }
                }
            }

            return posToCross;
        }

        private List<int> GetRowAutoCross(int row)
        {
            LinkedList<int> groups = puzzle.Grid.GetGroupsInRow(row);
            Clues clues = puzzle.RowClues[row];
            List<int> posToCross = [];

            bool groupsMatchClues = DoGroupsMatchClues(groups, clues);

            if (groupsMatchClues)
            {
                CellType[] rowCells = puzzle.Grid.GetRowArray(row);
                for (int i = 0; i < rowCells.Length; i++)
                {
                    if (rowCells[i] == CellType.BLANK)
                    {
                        posToCross.Add(i);
                    }
                }
            }

            return posToCross;
        }

        /// <summary>
        /// Determines whether <paramref name="groups"/> exactly matches <paramref name="clues"/>, i.e. consist of the same groups.
        /// </summary>
        /// <param name="groups">
        /// A linked list where each value represents the size of a consecutive group of filled cells.
        /// </param>
        /// <param name="clues">
        /// The expected clues for the line corresponding with <paramref name="groups"/>.
        /// </param>
        /// <returns>
        /// true if the number of groups matches the clues as described, false otherwise.
        /// </returns>
        private static bool DoGroupsMatchClues(LinkedList<int> groups, Clues clues)
        {
            if (clues.Count != groups.Count)
            {
                return false;
            }

            // Check if each group has the same number of cells filled as expected in the clues
            bool groupsMatchClues = true;
            LinkedListNode<int>? node = groups.First;
            for (int i = 0; i < groups.Count; i++)
            {
                if (clues[i].Number != node!.Value)
                {
                    groupsMatchClues = false;
                    break;
                }
                node = node.Next;
            }

            return groupsMatchClues;
        }

        private CellCommand CreateCellCommand(int x, int y, CellType newType)
        {
            CellType oldType = puzzle.Grid.GetCell(x, y);
            CellCommand c = new(x, y, puzzle.Grid, newType, oldType);
            return c;
        }
        protected internal virtual void OnCellCorrected(CorrectionEventArgs e)
        {
            CellCorrected?.Invoke(this, e);
        }
    }
}
