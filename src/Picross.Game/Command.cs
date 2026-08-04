using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Picross.Game
{
    internal interface ICommand
    {
        void Execute();
        void Undo();

        /// <summary>
        /// The changed cells.
        /// </summary>
        IEnumerable<Point> GetChanges();
    }

    internal class CellCommand : ICommand
    {
        public readonly int x;
        public readonly int y;
        private Grid grid;
        public readonly SquareType newType;
        public readonly SquareType oldType;

        public CellCommand(int x, int y, Grid g, SquareType newType, SquareType oldType)
        {
            this.x = x;
            this.y = y;
            this.grid = g;
            this.newType = newType;
            this.oldType = oldType;
        }

        public void Execute()
        {
            grid.SetCell(x, y, newType);
        }

        public void Undo()
        {
            grid.SetCell(x, y, oldType);
        }

        public IEnumerable<Point> GetChanges()
        {
            return [new Point(x, y)];
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

        public IEnumerable<Point> GetChanges()
        {
            List<Point> changes = [];

            foreach(ICommand command in commands)
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
            LinkedList<ICommand> combined = new LinkedList<ICommand>();
            combined.AddLast(x);
            combined.AddLast(y);

            return new CompositeCommand(combined);
        }
    }
}
