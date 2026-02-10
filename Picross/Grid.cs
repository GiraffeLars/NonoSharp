using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Collections;
using System.Runtime.ConstrainedExecution;

namespace Picross
{
    public class Grid
    {
        private SquareType[,] grid;
        private List<Point> solution;
        private List<int>[] verticalHints;
        private List<int>[] horizontalHints;
        private int filled = 0;

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
            solution = new List<Point>();

            fillRandomly(solution);


            verticalHints = new List<int>[width];
            horizontalHints = new List<int>[height];
            initializeHints();
            
        }

        private SquareType[,] gridifySolution()
        {
            SquareType[,] s = new SquareType[width, height];

            foreach (Point p in solution)
            {
                s[p.X, p.Y] = SquareType.FILLED;
            }

            return s;
        }

        private void initializeHints()
            
        {
            for (int i = 0; i < width; i++)
            {
                verticalHints[i] = new List<int>();
            }
            for (int i = 0; i < height; i++)
            {
                horizontalHints[i] = new List<int>();
            }
            setHints(verticalHints, true);
            setHints(horizontalHints, true);
            
        }

        private void setHints(List<int>[] hints, bool vertical)
        {
            // Sets the hint limits based on whether we process the vertical hints
            int xLimit = vertical ? width : height;
            int yLimit = vertical ? height : width;

            SquareType[,] gridSol = gridifySolution();
            for (int x = 0; x < xLimit; x++)
            {
                int count = 0;
                for (int y = 0; y < yLimit; y++)
                {
                    SquareType cell = vertical ? getCell(x, y) : getCell(y, x);

                    // If this is not a filled square
                    if (gridSol[x,y] != SquareType.FILLED)
                    {
                        // Add the new hint to the list
                        addHint(hints, x, count); // TODO if squares are split (i.e. empty between two patches), separate them with a 0
                        count = 0;
                        continue;
                    }

                    count++;
                }

                // Do final hint adding in case the last square is filled
                addHint(hints, x, count);
            }
        }

        private void addHint(List<int>[] hints, int pos, int count)
        {
            if (count > 0)
            {
                verticalHints[pos].Add(count);
                Console.WriteLine(verticalHints[pos][0]);
            }
        }

        public void setCell(int x, int y, SquareType value)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                throw new ArgumentOutOfRangeException("Cell coordinates are out of bounds.");
            }

            if (grid[x, y] != SquareType.FILLED && value == SquareType.FILLED)
            {
                filled++; // Keeps track of whether the same amount of squares are filled as the solution for efficiency
            }
            else if (grid[x, y] == SquareType.FILLED && value != SquareType.FILLED)
            {
                filled--;
            }

            grid[x, y] = value;

            // Check if this is a correct solution
            Console.Write("Is Correct? ");
            Console.WriteLine(isCorrect());
        }

        public SquareType getCell(int x, int y)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                throw new ArgumentOutOfRangeException("Cell coordinates are out of bounds.");
            }

            return grid[x, y];
        }

        private void fillRandomly(List<Point> g)
        {
            var random = new Random();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (random.NextInt64() % 14 == 0) {
                        Point p = new Point(x, y);
                        g.Add(p);
                    }
                }
            }
        }

        private bool isCorrect()
        {
            if (filled != solution.Count())
            {
                return false; 
            }

            for (int i = 0; i < solution.Count(); i++)
            {
                Point p = solution[i];
                if (grid[p.X, p.Y] != SquareType.FILLED)
                {
                    return false;
                }
            }

            return true;
        }

        public override string ToString()
        {
            foreach (Point p in solution)
            {
                Console.Write(p);
                Console.Write(" ");
            }
            Console.WriteLine();

            StringBuilder sb = new StringBuilder();
            sb.Append(createVerticalHintsString());
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    char c = ' ';
                    switch (getCell(x, y))
                    {
                        case SquareType.FILLED:
                            c = 'O';
                            break;
                        case SquareType.BLANK:
                            c = ' ';
                            break;
                        case SquareType.CROSS:
                            c = 'X';
                            break;
                        
                    }
                    sb.Append($"[{c}]");
                }
                sb.Append('\n');
            }
            return sb.ToString();
        }

        private String createVerticalHintsString()
        {
            StringBuilder sb = new StringBuilder();
            bool newRowString = false;

            // TODO revisit
            for (int y = 0; y < height; y++)
            {
                List<int> row = verticalHints[y];
                for (int x = 0; x < width; x++)
                {
                    sb.Append(" ");

                    try
                    {
                        sb.Append(row[x]);
                        newRowString = true;
                    }
                    catch (Exception e)
                    {
                        if (e is NullReferenceException || e is IndexOutOfRangeException || e is ArgumentOutOfRangeException)
                        {
                            sb.Append(" ");
                            continue;
                        }
                        else
                        {
                            throw;
                        }
                    }
                    sb.Append(" ");
                }

                if (newRowString)
                {
                    sb.Append("\n");
                    newRowString = false;
                }
            }
            return sb.ToString();
        }
    }


}
