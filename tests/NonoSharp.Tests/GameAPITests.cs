using System;
using System.Collections.Generic;
using System.Text;
using NonoSharp;

namespace NonoSharp.Tests
{
    public class GameAPITests
    {
        private readonly GameAPI api;

        public GameAPITests()
        {
            // API for a 15x15 grid with an empty solution.
            // Don't generate random solvable solutions as this increases test time.
            api = new GameAPI(new Grid(15, 15));
        }

        [Fact]
        public void TestEmptyStart()
        {
            for (int i = 0; i < api.Width; i++)
            {
                for (int j = 0; j < api.Height; j++)
                {
                    Assert.False(api.IsCellFilled(i, j));
                    Assert.False(api.IsCellCrossed(i, j));
                    Assert.True(api.IsCellEmpty(i, j));
                }
            }
        }

        [Fact]
        public void TestFill()
        {
            Assert.True(api.IsCellEmpty(0, 0));
            Assert.False(api.IsCellFilled(0, 0));
            Assert.False(api.IsCellCrossed(0, 0));

            api.FillCell(0, 0);

            Assert.False(api.IsCellEmpty(0, 0));
            Assert.True(api.IsCellFilled(0, 0));
            Assert.False(api.IsCellCrossed(0, 0));
        }

        [Fact]
        public void TestCross()
        {
            Assert.True(api.IsCellEmpty(0, 0));
            Assert.False(api.IsCellFilled(0, 0));
            Assert.False(api.IsCellCrossed(0, 0));

            api.CrossCell(0, 0);

            Assert.False(api.IsCellEmpty(0, 0));
            Assert.False(api.IsCellFilled(0, 0));
            Assert.True(api.IsCellCrossed(0, 0));
        }

        [Fact]
        public void TestEmpty()
        {
            Assert.True(api.IsCellEmpty(0, 0));
            Assert.False(api.IsCellFilled(0, 0));
            Assert.False(api.IsCellCrossed(0, 0));

            api.EmptyCell(0, 0);

            Assert.True(api.IsCellEmpty(0, 0));
            Assert.False(api.IsCellFilled(0, 0));
            Assert.False(api.IsCellCrossed(0, 0));

            // Also check that cell is empty after it was filled in
            api.FillCell(0, 0);
            Assert.False(api.IsCellEmpty(0, 0));
            api.EmptyCell(0, 0);
            Assert.True(api.IsCellEmpty(0, 0));
        }

        [Fact]
        public void TestUndo()
        {
            Assert.False(api.CanUndo);
            api.FillCell(0, 0);
            // Don't check fill working correctly, that's not the purpose here

            Assert.True(api.CanUndo);
            api.Undo();

            // Check if move successfully undone, i.e. filled in cell is now empty again
            Assert.False(api.CanUndo);
            Assert.True(api.IsCellEmpty(0, 0));
            Assert.False(api.IsCellFilled(0, 0));
        }

        [Fact]
        public void TestRedo()
        {
            Assert.False(api.CanRedo);
            api.FillCell(0, 0);
            Assert.False(api.CanRedo);

            api.Undo();
            Assert.True(api.CanRedo);

            api.Redo();

            // Check if redo successfully undid the undo, i.e. change the now empty cell back to filled
            Assert.False(api.CanRedo);
            Assert.True(api.IsCellFilled(0, 0));
            Assert.False(api.IsCellEmpty(0, 0));
        }

        [Fact]
        public void TestAutoCross()
        {
            Grid g = new Grid(5, 1);
            GameAPI autoCrossAPI = new(g);

            // [O][X][O][O][X]
            List<int> solXCoords = new() { 0, 2, 3 };
            g.SetSolution(solXCoords.Select(x => new CellPosition(x, 0)).ToList());
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
    }
}
