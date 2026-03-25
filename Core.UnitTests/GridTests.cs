using System.Diagnostics;
using System.Drawing;

namespace Core.UnitTests
{
    public class GridTests
    {
        private Grid grid;

        public GridTests()
        {
            grid = new Grid(5, 1);

            // Solution where [O][O][X][O][O] is correct
            List<Point> sol = new List<Point>();
            for (int i = 0; i < 5; i++)
            {
                if (i == 2) { continue; }
                sol.Add(new Point(i, 0));
            }
            grid.setSolution(sol);
        }

        private void FillInGridSolution()
        {
            for (int i = 0; i < grid.width; i++)
            {
                if (i == 2) { continue; }
                grid.setCell(i, 0, SquareType.FILLED);
            }
        }

        [Fact]
        public void TestIsSolved()
        {         
            // Test empty solution
            Assert.False(grid.isCorrect());

            // One cell filled
            grid.setCell(0, 0, SquareType.FILLED);
            Assert.False(grid.isCorrect());


            // All filled
            for (int i = 0; i < 5; i++)
            {
                grid.setCell(i, 0, SquareType.FILLED);
            }
            Assert.False(grid.isCorrect());

            // The correct solution
            grid.setCell(2, 0, SquareType.BLANK);
            Assert.True(grid.isCorrect());
        }

        [Fact]
        public void TestHintsNumbersVertical()
        {
            for (int i = 0; i < grid.width; i++)
            {
                if (i == 2)
                {
                    continue;
                }
                Assert.Equal(1, grid.verticalHints[i].Count);
                Assert.Equal(1, grid.verticalHints[i].GetHint(0).number);
            }

            // Check that vertical hints for 2 is 0
            Assert.Equal(1, grid.verticalHints[2].Count);
            Assert.Equal(0, grid.verticalHints[2].GetHint(0).number);
        }

        [Fact]
        public void TestHintsNumbersHorizontal()
        {
            Hints hints = Assert.Single(grid.horizontalHints);
            Assert.Equal(2, hints.Count);
            Assert.Equal(2, hints.GetHint(0).number);
            Assert.Equal(2, hints.GetHint(1).number);
        }

        [Fact]
        public void TestHintsCompletedVertical()
        {
            // Vertical hints using a grid of 5 x 1 with the current solution
            // should only be correct at index 2 (i.e. where no square is expected)
            for (int i = 0; i < grid.width; i++)
            {
                Assert.Equal(i == 2, grid.verticalHints[i].GetHint(0).completed);
            }

            // Fill in the expected squares
            FillInGridSolution();

            // Now all hints should be completed
            for (int i = 0; i < grid.width; i++)
            {
                Assert.True(grid.verticalHints[i].GetHint(0).completed);
            }

            // Finally, if we place a square at the expected empty one, this should no longer be completed
            grid.setCell(2, 0, SquareType.FILLED);
            Assert.False(grid.verticalHints[2].GetHint(0).completed);
        }

        [Fact]
        public void TestHintsCompletedHorizontal()
        {
            // The grid is not filled in, we expected these to not be completed
            for (int i = 0; i < grid.height; i++)
            {
                Assert.False(grid.horizontalHints[i].GetHint(0).completed);
                Assert.False(grid.horizontalHints[i].GetHint(1).completed);
            }

            FillInGridSolution();

            // Now we expected all hints to be completed
            for (int i = 0; i < grid.height; i++)
            {
                Assert.True(grid.horizontalHints[i].GetHint(0).completed);
                Assert.True(grid.horizontalHints[i].GetHint(1).completed);
            }
        }

        [Fact]
        public void TestHintsCompletedWithCrosses()
        {
            FillInGridSolution();
            grid.setCell(2, 0, SquareType.CROSS);

            for (int i = 0; i < grid.height; i++)
            {
                Assert.True(grid.horizontalHints[i].GetHint(0).completed);
                Assert.True(grid.horizontalHints[i].GetHint(1).completed);
            }

            grid.setCell(1, 0, SquareType.CROSS);
            for (int i = 0; i < grid.height; i++)
            {
                Assert.False(grid.horizontalHints[i].GetHint(0).completed);
                Assert.True(grid.horizontalHints[i].GetHint(1).completed);
            }

            grid.setCell(4, 0, SquareType.CROSS);
            for (int i = 0; i < grid.height; i++)
            {
                Assert.False(grid.horizontalHints[i].GetHint(0).completed);
                Assert.False(grid.horizontalHints[i].GetHint(1).completed);
            }
        }
    }
}
