using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Collections;
using System.Runtime.ConstrainedExecution;

namespace Picross
{
    internal class Grid
    {
        private SquareType[,] grid;
        private List<Point> solution;
        private List<int>[] _verticalHints;
        private List<int>[] _horizontalHints;
        private int filled = 0;
        private int paddingString = 0;

        public int width { get; }
        public int height { get; }

        public List<int>[] verticalHints { get { return _verticalHints; } }
        public List<int>[] horizontalHints { get { return _horizontalHints; } }

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


            _verticalHints = new List<int>[width];
            _horizontalHints = new List<int>[height];
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
                _verticalHints[i] = new List<int>();
            }
            for (int i = 0; i < height; i++)
            {
                _horizontalHints[i] = new List<int>();
            }
            setHints(_verticalHints, true);
            setHints(_horizontalHints, false);
            
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
                    SquareType cell = vertical ? gridSol[x, y] : gridSol[y, x];

                    // If this is not a filled square
                    if (cell != SquareType.FILLED)
                    {
                        // Add the new hint to the list
                        addHint(hints, x, count); // TODO if squares are split (i.e. empty between two patches), separate them with a 0
                        count = 0;
                        continue;
                    }

                    count++;
                }

                // Do final hint adding in case the last square is filled
                // Count minus 1 as it is increased by one even if unfilled
                SquareType lastCell = vertical ? gridSol[x, yLimit - 1] : gridSol[yLimit - 1, x];
                if (count > 0 && lastCell == SquareType.FILLED)
                {
                    addHint(hints, x, count);
                }
                else if (hints[x].Count == 0)
                {
                    hints[x].Add(0);
                }

                doHorizontalPaddingCount(hints[x].Count, vertical);
            }
        }

        private void addHint(List<int>[] hints, int pos, int count)
        {
            if (count > 0)
            {
                hints[pos].Add(count);
            }
        }

        private void doHorizontalPaddingCount(int count, bool vertical)
        {
            if (!vertical && count > paddingString)
            {
                paddingString = count; 
            }
        }

        private String getPadding()
        {
            return new string(' ', paddingString * 2);
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
            //Console.Write("Is Correct? ");
            //Console.WriteLine(isCorrect());
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
                    if (random.NextInt64() % 2 == 0)
                    {
                        Point p = new Point(x, y);
                        g.Add(p);
                    }
                }
            }
        }

        public bool isCorrect()
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
            //foreach (Point p in solution)
            //{
            //    Console.Write(p);
            //    Console.Write(" ");
            //}
            //Console.WriteLine();

            StringBuilder sb = new StringBuilder();
            String[] horizontalHintsStr = createHorizontalHintsString();

            sb.Append(createVerticalHintsString());
            //sb.AppendLine();

            for (int y = 0; y < height; y++)
            {
                sb.Append('\n');
                sb.Append(horizontalHintsStr[y]);
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
                
            }
            return sb.ToString();
        }

        private String createVerticalHintsString()
        {
            StringBuilder sb = new StringBuilder();
            
            //Console.WriteLine(getPadding());
            //Console.WriteLine(paddingString);

            bool newStringRow;
            int lastFilled;
            for (int y = 0; y < height; y++)
            {
                sb.Append(getPadding());
                newStringRow = false;
                lastFilled = 0;
                for (int x = 0; x < width; x++)
                {
                    List<int> hints = _verticalHints[x];
                    if (hints.Count > y)
                    {
                        // Add spaces for all columns with no hints until this column
                        sb.Append(new string(' ', (x - lastFilled) * 3));
                        sb.Append($" {hints[y]} ");
                        
                        newStringRow = true;
                        lastFilled = x + 1;
                    }
                }

                if (newStringRow)
                {
                    sb.AppendLine();
                    newStringRow = false;
                }
                lastFilled = 0;
            }
            return sb.ToString();
        }

        private String[] createHorizontalHintsString()
        {
            String[] hintsStr = new String[height];

            for (int i = 0; i < height; i++)
            {
                int x;
                StringBuilder sb = new StringBuilder();
                List<int> hints = _horizontalHints[i];

                for (x = 0; x < hints.Count; x++)
                {
                    sb.Append($"{hints[x]} ");
                }

                sb.Append(new string(' ', 2 * Math.Max(paddingString - x, 0)));
                hintsStr[i] = sb.ToString();
            }

            return hintsStr;
        }
    }


}
