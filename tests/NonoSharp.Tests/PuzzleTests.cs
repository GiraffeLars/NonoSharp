namespace NonoSharp.Tests
{
    public class PuzzleTests
    {
        private Puzzle puzzle;

        public PuzzleTests()
        {
            puzzle = new(5, 1, null);

            // Solution where [O][O][X][O][O] is correct
            HashSet<CellPosition> sol = [];
            for (int i = 0; i < 5; i++)
            {
                if (i == 2) { continue; }
                sol.Add(new CellPosition(i, 0));
            }
            puzzle.SetSolution(sol);
        }

        private void FillInPuzzleSolution()
        {
            for (int i = 0; i < puzzle.Width; i++)
            {
                if (i == 2) { continue; }
                puzzle.Grid.SetCell(i, 0, CellType.FILLED);
            }
        }

        [Fact]
        public void TestIsSolved()
        {         
            // Test empty solution
            Assert.False(puzzle.IsSolved());

            // One cell filled
            puzzle.Grid.SetCell(0, 0, CellType.FILLED);
            Assert.False(puzzle.IsSolved());


            // All filled
            for (int i = 0; i < 5; i++)
            {
                puzzle.Grid.SetCell(i, 0, CellType.FILLED);
            }
            Assert.False(puzzle.IsSolved());

            // The correct solution
            puzzle.Grid.SetCell(2, 0, CellType.BLANK);
            Assert.True(puzzle.IsSolved());
        }

        [Fact]
        public void TestIsSolvedWithoutPresetSolution()
        {
            Clues[] rowClues = [Clues.FromString("1 1")];
            Clues[] colClues = [Clues.FromString("1"), Clues.FromString("0"), Clues.FromString("1")];
            Puzzle p = new(3, 1, colClues, rowClues, null);

            Assert.False(p.IsSolved());

            p.Grid.SetCell(0, 0, CellType.FILLED);
            p.Grid.SetCell(2, 0, CellType.FILLED);

            Assert.True(p.IsSolved());
        }

        [Fact]
        public void TestCluesNumbersColumn()
        {
            for (int i = 0; i < puzzle.Width; i++)
            {
                if (i == 2)
                {
                    continue;
                }
                Assert.Equal(1, puzzle.ColumnClues[i].Count);
                Assert.Equal(1, puzzle.ColumnClues[i][0].Number);
            }

            // Check that column clues for 2 is 0
            Assert.Equal(1, puzzle.ColumnClues[2].Count);
            Assert.Equal(0, puzzle.ColumnClues[2][0].Number);
        }

        [Fact]
        public void TestCluesNumbersRow()
        {
            Clues clues = Assert.Single(puzzle.RowClues);
            Assert.Equal(2, clues.Count);
            Assert.Equal(2, clues[0].Number);
            Assert.Equal(2, clues[1].Number);
        }

        [Fact]
        public void TestCluesCompletedColumn()
        {
            // Column clues using a grid of 5 x 1 with the current solution
            // should only be correct at index 2 (i.e. where no cell is expected)
            for (int i = 0; i < puzzle.Width; i++)
            {
                Assert.Equal(i == 2, puzzle.ColumnClues[i][0].Completed);
            }

            // Fill in the expected cells
            FillInPuzzleSolution();

            // Now all clues should be completed
            for (int i = 0; i < puzzle.Width; i++)
            {
                Assert.True(puzzle.ColumnClues[i][0].Completed);
            }

            // Finally, if we place a cell at the expected empty one, this should no longer be completed
            puzzle.Grid.SetCell(2, 0, CellType.FILLED);
            Assert.False(puzzle.ColumnClues[2][0].Completed);
        }

        [Fact]
        public void TestCluesCompletedRow()
        {
            // The grid is not filled in, we expected these to not be completed
            for (int i = 0; i < puzzle.Height; i++)
            {
                Assert.False(puzzle.RowClues[i][0].Completed);
                Assert.False(puzzle.RowClues[i][1].Completed);
            }

            FillInPuzzleSolution();

            // Now we expected all clues to be completed
            for (int i = 0; i < puzzle.Height; i++)
            {
                Assert.True(puzzle.RowClues[i][0].Completed);
                Assert.True(puzzle.RowClues[i][1].Completed);
            }

            // Test for completion if only the last clues are filled in
            puzzle.Grid.SetCell(0, 0, CellType.BLANK);
            puzzle.Grid.SetCell(1, 0, CellType.BLANK);
            Assert.True(puzzle.RowClues[0][1].Completed);
        }

        [Fact]
        public void TestCluesCompletedWithCrosses()
        {
            FillInPuzzleSolution();
            puzzle.Grid.SetCell(2, 0, CellType.CROSS);

            for (int i = 0; i < puzzle.Height; i++)
            {
                Assert.True(puzzle.RowClues[i][0].Completed);
                Assert.True(puzzle.RowClues[i][1].Completed);
            }

            puzzle.Grid.SetCell(1, 0, CellType.CROSS);
            for (int i = 0; i < puzzle.Height; i++)
            {
                Assert.False(puzzle.RowClues[i][0].Completed);
                Assert.True(puzzle.RowClues[i][1].Completed);
            }

            puzzle.Grid.SetCell(4, 0, CellType.CROSS);
            for (int i = 0; i < puzzle.Height; i++)
            {
                Assert.False(puzzle.RowClues[i][0].Completed);
                Assert.False(puzzle.RowClues[i][1].Completed);
            }
        }

        [Fact]
        public void TestClone()
        {
            puzzle.Grid.SetCell(0, 0, CellType.FILLED);
            puzzle.Grid.SetCell(1, 0, CellType.CROSS);
            Puzzle clone = (Puzzle)puzzle.Clone();

            Assert.Equal(puzzle.Width, clone.Width);
            Assert.Equal(puzzle.Height, clone.Height);

            // Check for deep clones
            Assert.NotSame(puzzle.Grid, clone.Grid);
            Assert.NotSame(puzzle.ColumnClues, clone.ColumnClues);
            Assert.NotSame(puzzle.RowClues, clone.RowClues);
            Assert.NotSame(puzzle.Solution, clone.Solution);

            for (int i = 0; i < puzzle.Width; i++)
            {
                for (int j = 0; j < puzzle.Height; j++)
                {
                    Assert.Equal(
                        puzzle.Grid.GetCell(i, j),
                        clone.Grid.GetCell(i, j));
                }
            }

            Assert.Equal(puzzle.Solution, clone.Solution);

            ComparePuzzleCluesArrays(puzzle.ColumnClues, clone.ColumnClues);
            ComparePuzzleCluesArrays(puzzle.RowClues, clone.RowClues);
        }

        private static void ComparePuzzleCluesArrays(PuzzleClues[] expected, PuzzleClues[] actual)
        {
            Assert.Equal(expected.Length, actual.Length);
            for (int i = 0; i < expected.Length; i++)
            {
                // Check every property
                PuzzleClues expectedClues = expected[i];
                PuzzleClues actualClues = actual[i];
                Assert.Equal(expectedClues.Count, actualClues.Count);
                Assert.Equal(expectedClues.Position, actualClues.Position);
                Assert.Equal(expectedClues.FullyCompleted, actualClues.FullyCompleted);

                // Finally check every clue
                for (int j = 0; j < expectedClues.Count; j++)
                {
                    Assert.Equal(expectedClues[j].Completed, actualClues[j].Completed);
                    Assert.Equal(expectedClues[j].Number, actualClues[j].Number);
                }
            }
        }
    }
}
