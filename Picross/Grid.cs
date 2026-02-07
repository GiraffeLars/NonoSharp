using System;
using System.Collections.Generic;
using System.Text;

namespace Picross
{
    public class Grid
    {
        private bool[,] grid;

        public int width { get; }
        public int height { get; }

        public Grid(int width, int height)
        {
            this.width = width;
            this.height = height;
        }
    }
}
