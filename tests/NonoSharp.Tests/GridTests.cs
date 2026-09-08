using NonoSharp;
using System.Diagnostics;

namespace NonoSharp.Tests
{
    public class GridTests
    {
        private Grid grid;

        public GridTests()
        {
            grid = new Grid(5, 1);

            // Solution where [O][O][X][O][O] is correct
            HashSet<CellPosition> sol = [];
            for (int i = 0; i < 5; i++)
            {
                if (i == 2) { continue; }
                sol.Add(new CellPosition(i, 0));
            }
            grid.SetSolution(sol);
        }

        private Grid GetBigGrid()
        {
            grid = new Grid(15, 15);

            // Solution where [X][X][O][O][X][X][O][O][X]...
            //                  [X]....is correct
            HashSet<CellPosition> sol = [];
            for (int i = 0; i < 8; i++)
            {
                if (i <= 1 || i == 4 || i ==5) { continue; }
                sol.Add(new CellPosition(i, 0));
            }
            grid.SetSolution(sol);

            return grid;
        }

        private void FillInGridSolution()
        {
            for (int i = 0; i < grid.Width; i++)
            {
                if (i == 2) { continue; }
                grid.SetCell(i, 0, CellType.FILLED);
            }
        }

        [Fact]
        public void TestIsSolved()
        {         
            // Test empty solution
            Assert.False(grid.IsSolved());

            // One cell filled
            grid.SetCell(0, 0, CellType.FILLED);
            Assert.False(grid.IsSolved());


            // All filled
            for (int i = 0; i < 5; i++)
            {
                grid.SetCell(i, 0, CellType.FILLED);
            }
            Assert.False(grid.IsSolved());

            // The correct solution
            grid.SetCell(2, 0, CellType.BLANK);
            Assert.True(grid.IsSolved());
        }

        [Fact]
        public void TestCluesNumbersColumn()
        {
            for (int i = 0; i < grid.Width; i++)
            {
                if (i == 2)
                {
                    continue;
                }
                Assert.Equal(1, grid.ColumnClues[i].Count);
                Assert.Equal(1, grid.ColumnClues[i][0].Number);
            }

            // Check that column clues for 2 is 0
            Assert.Equal(1, grid.ColumnClues[2].Count);
            Assert.Equal(0, grid.ColumnClues[2][0].Number);
        }

        [Fact]
        public void TestCluesNumbersRow()
        {
            Clues clues = Assert.Single(grid.RowClues);
            Assert.Equal(2, clues.Count);
            Assert.Equal(2, clues[0].Number);
            Assert.Equal(2, clues[1].Number);
        }

        [Fact]
        public void TestCluesCompletedColumn()
        {
            // Column clues using a grid of 5 x 1 with the current solution
            // should only be correct at index 2 (i.e. where no cell is expected)
            for (int i = 0; i < grid.Width; i++)
            {
                Assert.Equal(i == 2, grid.ColumnClues[i][0].Completed);
            }

            // Fill in the expected cells
            FillInGridSolution();

            // Now all clues should be completed
            for (int i = 0; i < grid.Width; i++)
            {
                Assert.True(grid.ColumnClues[i][0].Completed);
            }

            // Finally, if we place a cell at the expected empty one, this should no longer be completed
            grid.SetCell(2, 0, CellType.FILLED);
            Assert.False(grid.ColumnClues[2][0].Completed);
        }

        [Fact]
        public void TestCluesCompletedRow()
        {
            // The grid is not filled in, we expected these to not be completed
            for (int i = 0; i < grid.Height; i++)
            {
                Assert.False(grid.RowClues[i][0].Completed);
                Assert.False(grid.RowClues[i][1].Completed);
            }

            FillInGridSolution();

            // Now we expected all clues to be completed
            for (int i = 0; i < grid.Height; i++)
            {
                Assert.True(grid.RowClues[i][0].Completed);
                Assert.True(grid.RowClues[i][1].Completed);
            }

            // Test for completion if only the last clues are filled in
            grid.SetCell(0, 0, CellType.BLANK);
            grid.SetCell(1, 0, CellType.BLANK);
            Assert.True(grid.RowClues[0][1].Completed);
        }

        [Fact]
        public void TestCluesCompletedWithCrosses()
        {
            FillInGridSolution();
            grid.SetCell(2, 0, CellType.CROSS);

            for (int i = 0; i < grid.Height; i++)
            {
                Assert.True(grid.RowClues[i][0].Completed);
                Assert.True(grid.RowClues[i][1].Completed);
            }

            grid.SetCell(1, 0, CellType.CROSS);
            for (int i = 0; i < grid.Height; i++)
            {
                Assert.False(grid.RowClues[i][0].Completed);
                Assert.True(grid.RowClues[i][1].Completed);
            }

            grid.SetCell(4, 0, CellType.CROSS);
            for (int i = 0; i < grid.Height; i++)
            {
                Assert.False(grid.RowClues[i][0].Completed);
                Assert.False(grid.RowClues[i][1].Completed);
            }
        }

        [Fact]
        public void TestCluesCompletedWithMultipleBetweenCrosses()
        {
            grid = GetBigGrid();

            Assert.False(grid.IsSolved());
            Assert.False(grid.RowClues[0][0].Completed);
            Assert.False(grid.RowClues[0][1].Completed);


            grid.SetCell(2, 0, CellType.FILLED);
            grid.SetCell(3, 0, CellType.FILLED);
            grid.SetCell(6, 0, CellType.FILLED);
            grid.SetCell(7, 0, CellType.FILLED);
            Assert.True(grid.IsSolved());

            grid.SetCell(0, 0, CellType.CROSS);
            grid.SetCell(1, 0, CellType.CROSS);
            grid.SetCell(4, 0, CellType.CROSS);
            grid.SetCell(5, 0, CellType.CROSS);
            Assert.True(grid.IsSolved());

            Assert.True(grid.RowClues[0][0].Completed);

            // Our specifications for when a user knows this should be filled requires all clues after the first/last to be between crosses or other cells
            Assert.False(grid.RowClues[0][1].Completed);

            grid.SetCell(8, 0, CellType.CROSS);
            Assert.True(grid.RowClues[0][1].Completed);

            for (int i = 9; i < grid.Width; i++)
            {
                grid.SetCell(i, 0, CellType.CROSS);
            }
            Assert.True(grid.IsSolved());
            Assert.True(grid.RowClues[0][0].Completed);
            Assert.True(grid.RowClues[0][1].Completed);

            // Check if clue completeness is still true if we know that it was handled from the back
            grid.SetCell(0, 0, CellType.BLANK);
            Assert.True(grid.IsSolved());
            Assert.True(grid.RowClues[0][0].Completed);
            Assert.True(grid.RowClues[0][1].Completed);

            grid.SetCell(7, 0, CellType.BLANK);
            Assert.False(grid.IsSolved());
        }

        [Fact]
        public void TestClue_SingleFilled_TwoInSolution()
        {
            // Solution
            // [O][X][O]

            Grid grid = new(3, 1);
            HashSet<CellPosition> sol = [new CellPosition(0, 0), new CellPosition(2, 0)];
            grid.SetSolution(sol);

            // [O][X][X]
            grid.SetCell(0, 0, CellType.FILLED);
            grid.SetCell(1, 0, CellType.CROSS);
            grid.SetCell(2, 0, CellType.CROSS);

            // Since the first cell is filled in, we expect the first clue to be completed as it makes more sense intuitively
            Assert.True(grid.RowClues[0][0].Completed);
            Assert.False(grid.RowClues[0][1].Completed);

            // Now check single filled in cell at the end
            // [X][X][O]
            grid.SetCell(0, 0, CellType.CROSS);
            grid.SetCell(2, 0, CellType.FILLED);

            // It does not really matter which clue is completed, both make sense in a way, as long as one is completed and the other is not
            bool completed0 = grid.RowClues[0][0].Completed;
            bool completed1 = grid.RowClues[0][1].Completed;

            // Since these are bools, c0 != c1 implies that one is true and the other is false
            // This is enough for what we want to test as above
            Assert.NotEqual(completed0, completed1);
        }
    }
}
