using System;
using System.Collections.Generic;
using System.Text;

namespace NonoSharp
{
    internal class HistoryManager
    {
        private readonly LinkedList<ICommand> undoStack;
        private readonly LinkedList<ICommand> redoStack;
        private readonly Lock historyLock = new();
        
        public bool CanUndo 
        { 
            get {
                historyLock.Enter();
                bool allowed = undoStack.Count != 0; 
                historyLock.Exit();
                return allowed;
            } 
        }

        public bool CanRedo
        {
            get
            {
                historyLock.Enter();
                bool allowed = redoStack.Count != 0;
                historyLock.Exit();
                return allowed;
            }
        }

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
            historyLock.Enter();
            try
            {
                undoStack.AddLast(command);
                redoStack.Clear();
            } finally
            {
                historyLock.Exit();
            }
        }

        /// <summary>
        /// Undoes the last move (if any).
        /// </summary>
        /// <returns><c>List</c> of changed cell positions if a move was undone, <c>null</c> otherwise.</returns>
        public List<CellPosition>? Undo()
        {
            historyLock.Enter();

            try
            {
                if (!CanUndo) return null;

                ICommand c = undoStack.Last!.Value; // We know for sure that this isn't null as non-empty
                undoStack.RemoveLast();
                c.Undo();
                redoStack.AddLast(c);
                return [.. c.GetChanges()];
            } finally { historyLock.Exit(); }
        }

        /// <summary>
        /// Redoes the last move (if any).
        /// </summary>
        /// <returns><c>IEnumerable</c> of changed cell positions if a move was redone, <c>null</c> otherwise.</returns>
        public List<CellPosition>? Redo()
        {
            historyLock.Enter();
            try
            {
                if (!CanRedo) return null;

                ICommand c = redoStack.Last!.Value;
                redoStack.RemoveLast();
                c.Execute();
                undoStack.AddLast(c);
                return [.. c.GetChanges()];
            } finally { historyLock.Exit(); }
        }
    }
}
