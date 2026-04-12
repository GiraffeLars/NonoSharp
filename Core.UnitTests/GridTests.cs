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
            grid.SetSolution(sol);
        }

        private Grid GetBigGrid()
        {
            grid = new Grid(15, 15);

            // Solution where [X][X][O][O][X][X][O][O][X]...
            //                  [X]....is correct
            List<Point> sol = new List<Point>();
            for (int i = 0; i < 8; i++)
            {
                if (i <= 1 || i == 4 || i ==5) { continue; }
                sol.Add(new Point(i, 0));
            }
            grid.SetSolution(sol);

            return grid;
        }

        private void FillInGridSolution()
        {
            for (int i = 0; i < grid.Width; i++)
            {
                if (i == 2) { continue; }
                grid.SetCell(i, 0, SquareType.FILLED);
            }
        }

        [Fact]
        public void TestIsSolved()
        {         
            // Test empty solution
            Assert.False(grid.IsSolved());

            // One cell filled
            grid.SetCell(0, 0, SquareType.FILLED);
            Assert.False(grid.IsSolved());


            // All filled
            for (int i = 0; i < 5; i++)
            {
                grid.SetCell(i, 0, SquareType.FILLED);
            }
            Assert.False(grid.IsSolved());

            // The correct solution
            grid.SetCell(2, 0, SquareType.BLANK);
            Assert.True(grid.IsSolved());
        }

        [Fact]
        public void TestHintsNumbersVertical()
        {
            for (int i = 0; i < grid.Width; i++)
            {
                if (i == 2)
                {
                    continue;
                }
                Assert.Equal(1, grid.VerticalHints[i].Count);
                Assert.Equal(1, grid.VerticalHints[i].GetHint(0).Number);
            }

            // Check that vertical hints for 2 is 0
            Assert.Equal(1, grid.VerticalHints[2].Count);
            Assert.Equal(0, grid.VerticalHints[2].GetHint(0).Number);
        }

        [Fact]
        public void TestHintsNumbersHorizontal()
        {
            Hints hints = Assert.Single(grid.HorizontalHints);
            Assert.Equal(2, hints.Count);
            Assert.Equal(2, hints.GetHint(0).Number);
            Assert.Equal(2, hints.GetHint(1).Number);
        }

        [Fact]
        public void TestHintsCompletedVertical()
        {
            // Vertical hints using a grid of 5 x 1 with the current solution
            // should only be correct at index 2 (i.e. where no square is expected)
            for (int i = 0; i < grid.Width; i++)
            {
                Assert.Equal(i == 2, grid.VerticalHints[i].GetHint(0).Completed);
            }

            // Fill in the expected squares
            FillInGridSolution();

            // Now all hints should be completed
            for (int i = 0; i < grid.Width; i++)
            {
                Assert.True(grid.VerticalHints[i].GetHint(0).Completed);
            }

            // Finally, if we place a square at the expected empty one, this should no longer be completed
            grid.SetCell(2, 0, SquareType.FILLED);
            Assert.False(grid.VerticalHints[2].GetHint(0).Completed);
        }

        [Fact]
        public void TestHintsCompletedHorizontal()
        {
            // The grid is not filled in, we expected these to not be completed
            for (int i = 0; i < grid.Height; i++)
            {
                Assert.False(grid.HorizontalHints[i].GetHint(0).Completed);
                Assert.False(grid.HorizontalHints[i].GetHint(1).Completed);
            }

            FillInGridSolution();

            // Now we expected all hints to be completed
            for (int i = 0; i < grid.Height; i++)
            {
                Assert.True(grid.HorizontalHints[i].GetHint(0).Completed);
                Assert.True(grid.HorizontalHints[i].GetHint(1).Completed);
            }

            // Test for completion if only the last hints are filled in
            grid.SetCell(0, 0, SquareType.BLANK);
            grid.SetCell(1, 0, SquareType.BLANK);
            Assert.True(grid.HorizontalHints[0].GetHint(1).Completed);
        }

        [Fact]
        public void TestHintsCompletedWithCrosses()
        {
            FillInGridSolution();
            grid.SetCell(2, 0, SquareType.CROSS);

            for (int i = 0; i < grid.Height; i++)
            {
                Assert.True(grid.HorizontalHints[i].GetHint(0).Completed);
                Assert.True(grid.HorizontalHints[i].GetHint(1).Completed);
            }

            grid.SetCell(1, 0, SquareType.CROSS);
            for (int i = 0; i < grid.Height; i++)
            {
                Assert.False(grid.HorizontalHints[i].GetHint(0).Completed);
                Assert.True(grid.HorizontalHints[i].GetHint(1).Completed);
            }

            grid.SetCell(4, 0, SquareType.CROSS);
            for (int i = 0; i < grid.Height; i++)
            {
                Assert.False(grid.HorizontalHints[i].GetHint(0).Completed);
                Assert.False(grid.HorizontalHints[i].GetHint(1).Completed);
            }
        }

        [Fact]
        public void TestHintsCompletedWithMultipleBetweenCrosses()
        {
            grid = GetBigGrid();

            Assert.False(grid.IsSolved());
            Assert.False(grid.HorizontalHints[0].GetHint(0).Completed);
            Assert.False(grid.HorizontalHints[0].GetHint(1).Completed);


            grid.SetCell(2, 0, SquareType.FILLED);
            grid.SetCell(3, 0, SquareType.FILLED);
            grid.SetCell(6, 0, SquareType.FILLED);
            grid.SetCell(7, 0, SquareType.FILLED);
            Assert.True(grid.IsSolved());

            grid.SetCell(0, 0, SquareType.CROSS);
            grid.SetCell(1, 0, SquareType.CROSS);
            grid.SetCell(4, 0, SquareType.CROSS);
            grid.SetCell(5, 0, SquareType.CROSS);
            Assert.True(grid.IsSolved());

            Assert.True(grid.HorizontalHints[0].GetHint(0).Completed);

            // Our specifications for when a user knows this should be filled requires all hints after the first/last to be between crosses or other squares
            Assert.False(grid.HorizontalHints[0].GetHint(1).Completed);

            grid.SetCell(8, 0, SquareType.CROSS);
            Assert.True(grid.HorizontalHints[0].GetHint(1).Completed);

            for (int i = 9; i < grid.Width; i++)
            {
                grid.SetCell(i, 0, SquareType.CROSS);
            }
            Assert.True(grid.IsSolved());
            Assert.True(grid.HorizontalHints[0].GetHint(0).Completed);
            Assert.True(grid.HorizontalHints[0].GetHint(1).Completed);

            // Check if hint completeness is still true if we know that it was handled from the back
            grid.SetCell(0, 0, SquareType.BLANK);
            Assert.True(grid.IsSolved());
            Assert.True(grid.HorizontalHints[0].GetHint(0).Completed);
            Assert.True(grid.HorizontalHints[0].GetHint(1).Completed);

            grid.SetCell(7, 0, SquareType.BLANK);
            Assert.False(grid.IsSolved());
        }
    }
}
