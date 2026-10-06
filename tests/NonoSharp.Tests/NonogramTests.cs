using System;
using System.Collections.Generic;
using System.Text;
using NonoSharp;

namespace NonoSharp.Tests
{
    public class NonogramTests
    {
        private readonly Nonogram nonogram;

        public NonogramTests()
        {
            var options = new NonogramOptions() { EnableAutoCorrect = false, EnableAutoCross = false };
            // Nonogram API for a 15x15 grid with an empty solution.
            // Don't generate random solvable solutions as this increases test time.
            nonogram = new Nonogram(new Puzzle(15, 15, null), options);
        }

        [Fact]
        public void TestEmptyStart()
        {
            for (int i = 0; i < nonogram.Width; i++)
            {
                for (int j = 0; j < nonogram.Height; j++)
                {
                    Assert.False(nonogram.IsCellFilled(i, j));
                    Assert.False(nonogram.IsCellCrossed(i, j));
                    Assert.True(nonogram.IsCellEmpty(i, j));
                }
            }
        }

        [Fact]
        public void TestFill()
        {
            Assert.True(nonogram.IsCellEmpty(0, 0));
            Assert.False(nonogram.IsCellFilled(0, 0));
            Assert.False(nonogram.IsCellCrossed(0, 0));

            nonogram.FillCell(0, 0);

            Assert.False(nonogram.IsCellEmpty(0, 0));
            Assert.True(nonogram.IsCellFilled(0, 0));
            Assert.False(nonogram.IsCellCrossed(0, 0));
        }

        [Fact]
        public void TestCross()
        {
            Assert.True(nonogram.IsCellEmpty(0, 0));
            Assert.False(nonogram.IsCellFilled(0, 0));
            Assert.False(nonogram.IsCellCrossed(0, 0));

            nonogram.CrossCell(0, 0);

            Assert.False(nonogram.IsCellEmpty(0, 0));
            Assert.False(nonogram.IsCellFilled(0, 0));
            Assert.True(nonogram.IsCellCrossed(0, 0));
        }

        [Fact]
        public void TestEmpty()
        {
            Assert.True(nonogram.IsCellEmpty(0, 0));
            Assert.False(nonogram.IsCellFilled(0, 0));
            Assert.False(nonogram.IsCellCrossed(0, 0));

            nonogram.EmptyCell(0, 0);

            Assert.True(nonogram.IsCellEmpty(0, 0));
            Assert.False(nonogram.IsCellFilled(0, 0));
            Assert.False(nonogram.IsCellCrossed(0, 0));

            // Also check that cell is empty after it was filled in
            nonogram.FillCell(0, 0);
            Assert.False(nonogram.IsCellEmpty(0, 0));
            nonogram.EmptyCell(0, 0);
            Assert.True(nonogram.IsCellEmpty(0, 0));
        }

        [Fact]
        public void TestUndo()
        {
            Assert.False(nonogram.CanUndo);
            nonogram.FillCell(0, 0);
            // Don't check fill working correctly, that's not the purpose here

            Assert.True(nonogram.CanUndo);
            nonogram.Undo();

            // Check if move successfully undone, i.e. filled in cell is now empty again
            Assert.False(nonogram.CanUndo);
            Assert.True(nonogram.IsCellEmpty(0, 0));
            Assert.False(nonogram.IsCellFilled(0, 0));
        }

        [Fact]
        public void TestRedo()
        {
            Assert.False(nonogram.CanRedo);
            nonogram.FillCell(0, 0);
            Assert.False(nonogram.CanRedo);

            nonogram.Undo();
            Assert.True(nonogram.CanRedo);

            nonogram.Redo();

            // Check if redo successfully undid the undo, i.e. change the now empty cell back to filled
            Assert.False(nonogram.CanRedo);
            Assert.True(nonogram.IsCellFilled(0, 0));
            Assert.False(nonogram.IsCellEmpty(0, 0));
        }

        [Fact]
        public void TestAutoCross()
        {
            Puzzle p = new(5, 1, null);
            Nonogram autoCrossAPI = new(p);

            // [O][X][O][O][X]
            HashSet<int> solXCoords = [0, 2, 3];
            p.SetSolution([.. solXCoords.Select(x => new CellPosition(x, 0))]);
            autoCrossAPI.FillCell(0, 0);

            // Check if auto cross didn't trigger
            for (int i = 1; i < 5; i++)
            {
                Assert.True(autoCrossAPI.IsCellEmpty(i, 0));
            }

            // Set rest of solution, and check for only blank space to be crossed
            autoCrossAPI.FillCell(2, 0); autoCrossAPI.FillCell(3, 0);
            for (int i = 0; i < 5; i++)
            {
                if (solXCoords.Contains(i))
                {
                    Assert.True(autoCrossAPI.IsCellFilled(i, 0));
                }
                else
                {
                    Assert.True(autoCrossAPI.IsCellCrossed(i, 0));
                }
            }
        }

        [Fact]
        public void TestAutoCrossDisabled()
        {
            Puzzle p = new(5, 1, null);

            NonogramOptions opts = new() { EnableAutoCross = false };
            Nonogram autoCrossAPI = new(p) { Options = opts };

            // [O][X][O][O][X]
            HashSet<int> solXCoords = [0, 2, 3];
            p.SetSolution([.. solXCoords.Select(x => new CellPosition(x, 0))]);
            autoCrossAPI.FillCell(0, 0);
            autoCrossAPI.FillCell(2, 0); autoCrossAPI.FillCell(3, 0);

            for (int i = 0; i < 5; i++)
            {
                if (solXCoords.Contains(i))
                {
                    Assert.True(autoCrossAPI.IsCellFilled(i, 0));
                }
                else
                {
                    // Checks if autocross successfully did not trigger
                    Assert.True(autoCrossAPI.IsCellEmpty(i, 0));
                }
            }
        }

        [Fact]
        public void TestRNGSeed()
        {
            var nonogram_1 = Nonogram.CreateRandomPuzzle(15, 15, 12345);
            var nonogram_2 = Nonogram.CreateRandomPuzzle(15, 15, 12345);

            Assert.True(nonogram_1.Solution.SetEquals(nonogram_2.Solution));
        }

        [Fact]
        public async Task TestRNGSeedAsync()
        {
            var nonogram_1 = await Nonogram.CreateRandomPuzzleAsync(15, 15, 12345);
            var nonogram_2 = await Nonogram.CreateRandomPuzzleAsync(15, 15, 12345);

            Assert.True(nonogram_1.Solution.SetEquals(nonogram_2.Solution));
        }

        [Fact]
        public void TestFromClues()
        {
            Clues[] colClues = [[new(3)], [new(1), new(1)], [new(3)]];
            Clues[] rowClues = [[new(3)], [new(1), new(1)], [new(3)]];
            Nonogram nonogram = new(3, 3, colClues, rowClues);
            HashSet<CellPosition> expected = [
                new(0,0), new(1,0), new(2, 0),
                new(0,1), new(2,1),
                new(0,2), new(1,2), new(2,2)];


            Assert.Equal(expected, nonogram.Solution);
        }

        [Fact]
        public void TestIsCellCorrect()
        {
            CellPosition cell = new(0, 0);
            HashSet<CellPosition> solution = [cell];
            var nonogram = new Nonogram(1, 1, solution);

            Assert.False(nonogram.IsCellCorrect(cell.X, cell.Y));
            Assert.False(nonogram.IsCellCorrect(cell));

            nonogram.FillCell(cell);

            Assert.True(nonogram.IsCellCorrect(cell.X, cell.Y));
            Assert.True(nonogram.IsCellCorrect(cell));

            nonogram.CrossCell(cell);
            Assert.False(nonogram.IsCellCorrect(cell.X, cell.Y));
            Assert.False(nonogram.IsCellCorrect(cell));
        }

        [Fact]
        public async Task TestGenerateRandomCancellation()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await Nonogram.CreateRandomPuzzleAsync(15, 15, cancellationToken: cts.Token);
            });

            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await Nonogram.CreateRandomPuzzleAsync(15, 15, 12345, cancellationToken: cts.Token);
            });
        }

        [Fact]
        public async Task TestPathLoadingCancellation()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            
            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            {
                await Nonogram.LoadPuzzleAsync("totally real path", cancellationToken: cts.Token);
            });
        }
    }
}
