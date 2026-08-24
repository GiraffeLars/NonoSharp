using System;
using System.Collections.Generic;
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

            List<CellPosition> sol = [new CellPosition(0, 0)];

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

            List<CellPosition> solution =
            [
                new CellPosition(0, 0),
                new CellPosition(0, 1),
                new CellPosition(0, 2), new CellPosition(1, 2), new CellPosition(2, 2)
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

            List<CellPosition> solution =
            [
                new CellPosition(0, 0), new CellPosition(2, 0),
                new CellPosition(1, 1),
                new CellPosition(0, 2), new CellPosition(2, 2)
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
            List<CellPosition> solution = new List<CellPosition> { new CellPosition(0, 0), new CellPosition(1, 1) };
            grid.SetSolution(solution);

            Assert.False(Solver.IsSolvable(grid));

            // Also test the other possibility for this ambigous grid
            // [ ][O]
            // [O][ ]
            grid = new Grid(2, 2);
            solution = new List<CellPosition> { new CellPosition(1, 0), new CellPosition(0, 1) };
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
                new CellPosition(1, 0), new CellPosition(2, 0), new CellPosition(3, 0)
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
                new CellPosition(0, 0),
                new CellPosition(0, 1), new CellPosition(0, 4), new CellPosition(0, 7), new CellPosition(0, 8), new CellPosition(0, 9),
                new CellPosition(0, 10), new CellPosition(0, 12), new CellPosition(0, 13), new CellPosition(1, 1), new CellPosition(1, 3),
                new CellPosition(1, 7), new CellPosition(1, 9), new CellPosition(1, 11), new CellPosition(1, 12), new CellPosition(1, 13),
                new CellPosition(2, 2), new CellPosition(2, 4), new CellPosition(2, 5), new CellPosition(2, 6), new CellPosition(2, 8),
                new CellPosition(2, 11), new CellPosition(3, 0), new CellPosition(3, 1), new CellPosition(3, 5), new CellPosition(3, 6),
                new CellPosition(3, 7), new CellPosition(3, 12), new CellPosition(3, 14), new CellPosition(4, 3), new CellPosition(4, 6),
                new CellPosition(4, 8), new CellPosition(4, 13), new CellPosition(5, 3), new CellPosition(5, 8), new CellPosition(5, 9),
                new CellPosition(5, 12), new CellPosition(5, 14), new CellPosition(6, 1), new CellPosition(6, 4), new CellPosition(6, 5),
                new CellPosition(6, 7), new CellPosition(6, 10), new CellPosition(6, 11), new CellPosition(6, 12), new CellPosition(7, 0),
                new CellPosition(7, 3), new CellPosition(7, 5), new CellPosition(7, 6), new CellPosition(7, 9), new CellPosition(8, 3),
                new CellPosition(8, 5), new CellPosition(8, 6), new CellPosition(8, 7), new CellPosition(8, 9), new CellPosition(8, 10),
                new CellPosition(8, 11), new CellPosition(8, 14), new CellPosition(9, 0), new CellPosition(9, 2), new CellPosition(9, 4),
                new CellPosition(9, 6), new CellPosition(9, 8), new CellPosition(9, 9), new CellPosition(9, 10), new CellPosition(9, 11),
                new CellPosition(9, 12), new CellPosition(9, 14), new CellPosition(10, 1), new CellPosition(10, 2), new CellPosition(10, 6),
                new CellPosition(10, 7), new CellPosition(11, 1), new CellPosition(11, 2), new CellPosition(11, 4), new CellPosition(11, 7),
                new CellPosition(11, 8), new CellPosition(11, 12), new CellPosition(11, 13), new CellPosition(12, 5), new CellPosition(12, 9),
                new CellPosition(12, 13), new CellPosition(13, 1), new CellPosition(13, 3), new CellPosition(13, 4), new CellPosition(13, 5),
                new CellPosition(13, 6), new CellPosition(13, 7), new CellPosition(13, 10), new CellPosition(14, 0), new CellPosition(14, 3),
                new CellPosition(14, 4), new CellPosition(14, 5), new CellPosition(14, 7), new CellPosition(14, 8), new CellPosition(14, 10),
                new CellPosition(14, 13)]
            );
            Assert.False(Solver.IsSolvable(grid));


            grid = new(15, 15);
            grid.SetSolution(
                [
                new CellPosition(0, 0),
                new CellPosition(1, 0),    new CellPosition(2, 0),    new CellPosition(9, 0),    new CellPosition(10, 0),    new CellPosition(12, 0),
                new CellPosition(14, 0),    new CellPosition(0, 1),    new CellPosition(10, 1),    new CellPosition(11, 1),    new CellPosition(12, 1),
                new CellPosition(13, 1),    new CellPosition(14, 1),    new CellPosition(7, 2),    new CellPosition(8, 2),    new CellPosition(9, 2),
                new CellPosition(10, 2),    new CellPosition(11, 2),    new CellPosition(12, 2),    new CellPosition(13, 2),    new CellPosition(14, 2),
                new CellPosition(6, 3),    new CellPosition(7, 3),    new CellPosition(8, 3),    new CellPosition(10, 3),    new CellPosition(11, 3),
                new CellPosition(12, 3),    new CellPosition(13, 3),    new CellPosition(14, 3),    new CellPosition(5, 4),    new CellPosition(6, 4),
                new CellPosition(7, 4),    new CellPosition(8, 4),    new CellPosition(10, 4),    new CellPosition(11, 4),    new CellPosition(12, 4),
                new CellPosition(13, 4),    new CellPosition(0, 5),    new CellPosition(1, 5),    new CellPosition(4, 5),    new CellPosition(5, 5),
                new CellPosition(6, 5),    new CellPosition(11, 5),    new CellPosition(12, 5),    new CellPosition(13, 5),    new CellPosition(14, 5),
                new CellPosition(0, 6),    new CellPosition(1, 6),    new CellPosition(5, 6),    new CellPosition(6, 6),    new CellPosition(7, 6),
                new CellPosition(8, 6),    new CellPosition(12, 6),    new CellPosition(13, 6),    new CellPosition(14, 6),    new CellPosition(1, 7),
                new CellPosition(2, 7),    new CellPosition(7, 7),    new CellPosition(8, 7),    new CellPosition(9, 7),    new CellPosition(10, 7),
                new CellPosition(11, 7),    new CellPosition(12, 7),    new CellPosition(1, 8),    new CellPosition(2, 8),    new CellPosition(3, 8),
                new CellPosition(6, 8),    new CellPosition(7, 8),    new CellPosition(8, 8),    new CellPosition(9, 8),    new CellPosition(10, 8),
                new CellPosition(11, 8),    new CellPosition(1, 9),    new CellPosition(2, 9),    new CellPosition(3, 9),    new CellPosition(6, 9),
                new CellPosition(8, 9),    new CellPosition(9, 9),    new CellPosition(10, 9),    new CellPosition(11, 9),    new CellPosition(0, 10),
                new CellPosition(1, 10),    new CellPosition(2, 10),    new CellPosition(3, 10),    new CellPosition(8, 10),    new CellPosition(9, 10),
                new CellPosition(0, 11),    new CellPosition(1, 11),    new CellPosition(6, 11),    new CellPosition(7, 11),    new CellPosition(8, 11),
                new CellPosition(9, 11),    new CellPosition(13, 11),    new CellPosition(14, 11),    new CellPosition(0, 12),    new CellPosition(1, 12),
                new CellPosition(7, 12),    new CellPosition(8, 12),    new CellPosition(9, 12),    new CellPosition(13, 12),    new CellPosition(14, 12),
                new CellPosition(6, 13),    new CellPosition(7, 13),    new CellPosition(8, 13),    new CellPosition(13, 13),    new CellPosition(14, 13),
                new CellPosition(1, 14),    new CellPosition(2, 14),    new CellPosition(6, 14),    new CellPosition(7, 14),    new CellPosition(12, 14),
                new CellPosition(13, 14),    new CellPosition(14, 14),
                ]);

            Assert.True(Solver.IsSolvable(grid));

        }
    }
}
