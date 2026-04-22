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
}
