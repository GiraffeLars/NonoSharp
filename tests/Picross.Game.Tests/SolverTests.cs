using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Picross.Game.Tests
{
    public class SolverTests
    {
        [Fact]
        public void TestOneCellSolvable()
        {
            // Basic solvable test, where grid is just one cell empty/filled
            Grid grid = new Grid(1, 1);

            // Empty grid
            Assert.True(Solver.IsSolvable(grid));

            List<Point> sol = [new Point(0, 0)];

            grid.SetSolution(sol);

            Assert.True(Solver.IsSolvable(grid));

            // Check if the puzzle is still solvable after the user mistakingly placed a cross
            grid.SetCell(0, 0, CellType.CROSS);
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

        [Fact]
        public void TestEdgeFilledSolvability()
        {
            // A line where, without extra information the solve is impossible, but with an already
            // placed cell at the edge, it is possible.

            // [ ][O][O][O]
            Grid grid = new Grid(4, 1);

            grid.SetSolution([
                new Point(1, 0), new Point(2, 0), new Point(3, 0)
                ]);

            // To make sure that the info from the other hints are not used, we test ImproveLine

            // Copy to avoid changing the grid unintentionally
            CellType[] line = (CellType[]) grid.GetRowArray(0).Clone();
            Solver.ImproveLine(line, grid.RowHints[0]);


            // Check if the only information we get is expected; [ ][O][O][ ] is a must in this case
            Assert.NotEqual(CellType.FILLED, line[0]);
            Assert.Equal(CellType.FILLED, line[1]);
            Assert.Equal(CellType.FILLED, line[2]);
            Assert.NotEqual(CellType.FILLED, line[3]);

            // Now check if the line can correctly be solved with extra information
            grid.SetCell(3, 0, CellType.FILLED);
            line = (CellType[])grid.GetRowArray(0).Clone();
            Solver.ImproveLine(line, grid.RowHints[0]);

            // Check if 0th cell is not filled
            Assert.NotEqual(CellType.FILLED, line[0]);

            // Check if others are filled
            for (int i = 1; i < line.Length; i++)
            {
                Assert.Equal(CellType.FILLED, line[i]);
            }
        }

        [Fact]
        public void Test15x15Grid()
        {
            Grid grid = new(15, 15);
            grid.SetSolution([
                new Point(0, 0),
                new Point(0, 1), new Point(0, 4), new Point(0, 7), new Point(0, 8), new Point(0, 9),
                new Point(0, 10), new Point(0, 12), new Point(0, 13), new Point(1, 1), new Point(1, 3),
                new Point(1, 7), new Point(1, 9), new Point(1, 11), new Point(1, 12), new Point(1, 13),
                new Point(2, 2), new Point(2, 4), new Point(2, 5), new Point(2, 6), new Point(2, 8),
                new Point(2, 11), new Point(3, 0), new Point(3, 1), new Point(3, 5), new Point(3, 6),
                new Point(3, 7), new Point(3, 12), new Point(3, 14), new Point(4, 3), new Point(4, 6),
                new Point(4, 8), new Point(4, 13), new Point(5, 3), new Point(5, 8), new Point(5, 9),
                new Point(5, 12), new Point(5, 14), new Point(6, 1), new Point(6, 4), new Point(6, 5),
                new Point(6, 7), new Point(6, 10), new Point(6, 11), new Point(6, 12), new Point(7, 0),
                new Point(7, 3), new Point(7, 5), new Point(7, 6), new Point(7, 9), new Point(8, 3),
                new Point(8, 5), new Point(8, 6), new Point(8, 7), new Point(8, 9), new Point(8, 10),
                new Point(8, 11), new Point(8, 14), new Point(9, 0), new Point(9, 2), new Point(9, 4),
                new Point(9, 6), new Point(9, 8), new Point(9, 9), new Point(9, 10), new Point(9, 11),
                new Point(9, 12), new Point(9, 14), new Point(10, 1), new Point(10, 2), new Point(10, 6),
                new Point(10, 7), new Point(11, 1), new Point(11, 2), new Point(11, 4), new Point(11, 7),
                new Point(11, 8), new Point(11, 12), new Point(11, 13), new Point(12, 5), new Point(12, 9),
                new Point(12, 13), new Point(13, 1), new Point(13, 3), new Point(13, 4), new Point(13, 5),
                new Point(13, 6), new Point(13, 7), new Point(13, 10), new Point(14, 0), new Point(14, 3),
                new Point(14, 4), new Point(14, 5), new Point(14, 7), new Point(14, 8), new Point(14, 10),
                new Point(14, 13)]
            );
            Assert.False(Solver.IsSolvable(grid));


            grid = new(15, 15);
            grid.SetSolution(
                [
                new Point(0, 0),
                new Point(1, 0),    new Point(2, 0),    new Point(9, 0),    new Point(10, 0),    new Point(12, 0),
                new Point(14, 0),    new Point(0, 1),    new Point(10, 1),    new Point(11, 1),    new Point(12, 1),
                new Point(13, 1),    new Point(14, 1),    new Point(7, 2),    new Point(8, 2),    new Point(9, 2),
                new Point(10, 2),    new Point(11, 2),    new Point(12, 2),    new Point(13, 2),    new Point(14, 2),
                new Point(6, 3),    new Point(7, 3),    new Point(8, 3),    new Point(10, 3),    new Point(11, 3),
                new Point(12, 3),    new Point(13, 3),    new Point(14, 3),    new Point(5, 4),    new Point(6, 4),
                new Point(7, 4),    new Point(8, 4),    new Point(10, 4),    new Point(11, 4),    new Point(12, 4),
                new Point(13, 4),    new Point(0, 5),    new Point(1, 5),    new Point(4, 5),    new Point(5, 5),
                new Point(6, 5),    new Point(11, 5),    new Point(12, 5),    new Point(13, 5),    new Point(14, 5),
                new Point(0, 6),    new Point(1, 6),    new Point(5, 6),    new Point(6, 6),    new Point(7, 6),
                new Point(8, 6),    new Point(12, 6),    new Point(13, 6),    new Point(14, 6),    new Point(1, 7),
                new Point(2, 7),    new Point(7, 7),    new Point(8, 7),    new Point(9, 7),    new Point(10, 7),
                new Point(11, 7),    new Point(12, 7),    new Point(1, 8),    new Point(2, 8),    new Point(3, 8),
                new Point(6, 8),    new Point(7, 8),    new Point(8, 8),    new Point(9, 8),    new Point(10, 8),
                new Point(11, 8),    new Point(1, 9),    new Point(2, 9),    new Point(3, 9),    new Point(6, 9),
                new Point(8, 9),    new Point(9, 9),    new Point(10, 9),    new Point(11, 9),    new Point(0, 10),
                new Point(1, 10),    new Point(2, 10),    new Point(3, 10),    new Point(8, 10),    new Point(9, 10),
                new Point(0, 11),    new Point(1, 11),    new Point(6, 11),    new Point(7, 11),    new Point(8, 11),
                new Point(9, 11),    new Point(13, 11),    new Point(14, 11),    new Point(0, 12),    new Point(1, 12),
                new Point(7, 12),    new Point(8, 12),    new Point(9, 12),    new Point(13, 12),    new Point(14, 12),
                new Point(6, 13),    new Point(7, 13),    new Point(8, 13),    new Point(13, 13),    new Point(14, 13),
                new Point(1, 14),    new Point(2, 14),    new Point(6, 14),    new Point(7, 14),    new Point(12, 14),
                new Point(13, 14),    new Point(14, 14),
                ]);

            Assert.True(Solver.IsSolvable(grid));

        }
    }
}
