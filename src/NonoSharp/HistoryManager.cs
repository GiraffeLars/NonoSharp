using System;
using System.Collections.Generic;
using System.Text;

namespace NonoSharp
{
    internal class HistoryManager
    {
        private readonly LinkedList<ICommand> undoStack;
        private readonly LinkedList<ICommand> redoStack;
        
        public bool CanUndo { get { return undoStack.Count != 0; } }

        public bool CanRedo { get { return redoStack.Count != 0; } }

        internal HistoryManager()
        {
            undoStack = new();
            redoStack = new();
        }

        /// <summary>
        /// Pushes <paramref name="command"/> onto <c>undoStack</c> and clears <c>redoStack</c>.
        /// </summary>
        /// <param name="command">Command to push</param>
        public void PushCommand(ICommand command)
        {
            undoStack.AddLast(command);
            redoStack.Clear();
        }

        /// <summary>
        /// Undoes the last move (if any).
        /// </summary>
        /// <returns><c>IEnumerable</c> of changed cell positions if a move was undone, <c>null</c> otherwise.</returns>
        public IEnumerable<CellPosition>? Undo()
        {
            if (!CanUndo) return null;

            ICommand c = undoStack.Last!.Value; // We know for sure that this isn't null as non-empty
            undoStack.RemoveLast();
            c.Undo();
            redoStack.AddLast(c);
            return c.GetChanges();
        }

        /// <summary>
        /// Redoes the last move (if any).
        /// </summary>
        /// <returns><c>IEnumerable</c> of changed cell positions if a move was redone, <c>null</c> otherwise.</returns>
        public IEnumerable<CellPosition>? Redo()
        {
            if (!CanRedo) return null;

            ICommand c = redoStack.Last!.Value;
            redoStack.RemoveLast();
            c.Execute();
            undoStack.AddLast(c);
            return c.GetChanges();
        }
    }
}
