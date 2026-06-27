using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Picross.Game.Tests
{
    public class SolverTests
    {
        [Fact]
        public void TestOneSquareSolvable()
        {
            // Basic solvable test, where grid is just one square empty/filled
            Grid grid = new Grid(1, 1);

            // Empty grid
            Assert.True(Solver.IsSolvable(grid));

            List<Point> sol = [new Point(0, 0)];

            grid.SetSolution(sol);

            Assert.True(Solver.IsSolvable(grid));

            // Check if the puzzle is still solvable after the user mistakingly placed a cross
            grid.SetCell(0, 0, SquareType.CROSS);
            Assert.False(Solver.IsSolvable(grid));
        }

        [Fact]
        public void TestSolvableLShape()
        {
            // Solution:
            // [O][ ][ ]
            // [O][ ][ ]
            // [O][O][O]
            Grid grid = new Grid(3, 3);

            List<Point> solution =
            [
                new Point(0, 0),
                new Point(0, 1),
                new Point(0, 2), new Point(1, 2), new Point(2, 2)
            ];
            grid.SetSolution(solution);

            Assert.True(Solver.IsSolvable(grid));
        }

        [Fact]
        public void TestSolvableCrossShape()
        {
            // Solution:
            // [O][ ][O]
            // [ ][O][ ]
            // [O][ ][O]
            Grid grid = new Grid(3, 3);

            List<Point> solution =
            [
                new Point(0, 0), new Point(2, 0),
                new Point(1, 1),
                new Point(0, 2), new Point(2, 2)
            ];
            grid.SetSolution(solution);

            Assert.True(Solver.IsSolvable(grid));
        }

        [Fact]
        public void TestSolvableGuessRequired()
        {
            // An ambiguous grid, where there are two possible solutions according to the hints:
            // [O][ ]
            // [ ][O]

            Grid grid = new Grid(2, 2);
            List<Point> solution = new List<Point> { new Point(0, 0), new Point(1, 1) };
            grid.SetSolution(solution);

            Assert.False(Solver.IsSolvable(grid));

            // Also test the other possibility for this ambigous grid
            // [ ][O]
            // [O][ ]
            grid = new Grid(2, 2);
            solution = new List<Point> { new Point(1, 0), new Point(0, 1) };
            grid.SetSolution(solution);

            Assert.False(Solver.IsSolvable(grid));
        }
    }
}
