using System;
using System.Collections.Generic;
using System.Text;

namespace Picross.Game
{
    internal interface Command
    {
        void Execute();
        void Undo();
    }

    internal class CellCommand : Command
    {
        int x;
        int y;
        Grid grid;
        SquareType newType;
        SquareType oldType;

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
    }

    /// <summary>
    /// A Command consisting of multiple Commands.
    /// </summary>
    internal class CompositeCommand : Command
    {
        private readonly LinkedList<Command> commands;

        /// <summary>
        /// Total Command types in this CompositeCommand. Is not recursive.
        /// </summary>
        public int Count { get { return commands.Count; } }

        internal CompositeCommand(LinkedList<Command> commands)
        {
            this.commands = commands;
        }

        public void Execute()
        {
            LinkedListNode<Command>? node = commands.First;

            while (node != null)
            {
                node.Value.Execute();
                node = node.Next;
            }
        }

        public void Undo()
        {
            LinkedListNode<Command>? node = commands.Last;

            while (node != null)
            {
                node.Value.Undo();
                node = node.Previous;
            }
        }

        /// <summary>
        /// Combines commands <paramref name="x"/> and <paramref name="y"/> into one <c>CompositeCommand</c>.
        /// <paramref name="x"/> is the first command.
        /// </summary>
        /// <param name="x">The first command to combine</param>
        /// <param name="y">The second command to combine</param>
        /// <returns><c>CompositeCommand</c> consisting of Commands x and y, with x being the first</returns>
        public static CompositeCommand Combine(Command x, Command y)
        {
            LinkedList<Command> combined = new LinkedList<Command>();
            combined.AddLast(x);
            combined.AddLast(y);

            return new CompositeCommand(combined);
        }
    }
}
