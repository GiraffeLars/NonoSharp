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
            // Basic solvable test, where grid is just one square filled

            Grid grid = new Grid(1, 1);
            List<Point> sol = [new Point(0, 0)];

            grid.SetSolution(sol);

            Solver solver = new Solver(grid);
            Assert.True(solver.IsSolvable());
        }
    }
}
