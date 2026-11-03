using System;
using System.Collections.Generic;
using System.Text;

namespace NonoSharp
{
    internal interface ICommand
    {
        void Execute();
        void Undo();

        /// <summary>
        /// The changed cells.
        /// </summary>
        IEnumerable<CellPosition> GetChanges();
    }

    internal class CellCommand(int x, int y, Grid g, CellType newType, CellType oldType) : ICommand
    {
        public int X { get; } = x;
        public int Y { get; } = y;
        private readonly Grid grid = g;
        public CellType NewType { get; } = newType;
        public CellType OldType { get; } = oldType;

        public void Execute()
        {
            grid.SetCell(X, Y, NewType);
        }

        public void Undo()
        {
            grid.SetCell(X, Y, OldType);
        }

        public IEnumerable<CellPosition> GetChanges()
        {
            return [new CellPosition(X, Y)];
        }
    }

    /// <summary>
    /// A Command consisting of multiple Commands.
    /// </summary>
    internal class CompositeCommand : ICommand
    {
        protected readonly LinkedList<ICommand> commands;

        /// <summary>
        /// Total Command types in this CompositeCommand. Is not recursive.
        /// </summary>
        public int Count { get { return commands.Count; } }

        internal CompositeCommand(LinkedList<ICommand> commands)
        {
            this.commands = commands;
        }

        public void Execute()
        {
            LinkedListNode<ICommand>? node = commands.First;

            while (node != null)
            {
                node.Value.Execute();
                node = node.Next;
            }
        }

        public void Undo()
        {
            LinkedListNode<ICommand>? node = commands.Last;

            while (node != null)
            {
                node.Value.Undo();
                node = node.Previous;
            }
        }

        public IEnumerable<CellPosition> GetChanges()
        {
            List<CellPosition> changes = [];

            foreach (ICommand command in commands)
            {
                changes.AddRange(command.GetChanges());
            }

            return changes;
        }

        /// <summary>
        /// Combines commands <paramref name="x"/> and <paramref name="y"/> into one <c>CompositeCommand</c>.
        /// <paramref name="x"/> is the first command.
        /// </summary>
        /// <param name="x">The first command to combine</param>
        /// <param name="y">The second command to combine</param>
        /// <returns><c>CompositeCommand</c> consisting of Commands x and y, with x being the first</returns>
        public static CompositeCommand Combine(ICommand x, ICommand y)
        {
            LinkedList<ICommand> combined = new();
            combined.AddLast(x);
            combined.AddLast(y);

            return new CompositeCommand(combined);
        }
    }
}
