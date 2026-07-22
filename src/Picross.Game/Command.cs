using System;
using System.Collections.Generic;
using System.Text;

namespace Picross.Game
{
    internal interface Command
    {
        void Execute();
        void Undo();

        public static Command operator +(Command left, Command right)
        {
            LinkedList<Command> combined = new LinkedList<Command>();
            combined.AddLast(left);
            combined.AddLast(right);

            return new CompositeCommand(combined);
        }
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
    }
}
