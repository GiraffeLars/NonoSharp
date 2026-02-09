using System;
using System.Collections.Generic;
using System.Text;

namespace Picross
{
    public class Grid
    {
        private SquareType[,] grid;
        private SquareType[,] solution;

        public int width { get; }
        public int height { get; }

        public Grid(int width, int height)
        {
            if (width <= 0 || height <= 0)
            {
                throw new ArgumentException("Width and height must be positive integers.");
            }

            this.width = width;
            this.height = height;

            grid = new SquareType[width, height];
            solution = new SquareType[width, height];
            fillRandomly(solution);
        }

        public void setCell(int x, int y, SquareType value)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                throw new ArgumentOutOfRangeException("Cell coordinates are out of bounds.");
            }

            grid[x, y] = value;
        }

        public SquareType getCell(int x, int y)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                throw new ArgumentOutOfRangeException("Cell coordinates are out of bounds.");
            }

            return grid[x, y];
        }

        private void fillRandomly(SquareType[,] g)
        {
            var random = new Random();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    g[x, y] = random.NextInt64() % 2 == 0 ? SquareType.FILLED : SquareType.BLANK;
                }
            }

        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    sb.Append(grid[x, y] == SquareType.FILLED ? "[O]" : "[ ]");
                }
                sb.Append('\n');
            }
            return sb.ToString();
        }
    }
}
