using System;
using System.Collections.Generic;
using System.Text;
using Picross.Game;

namespace Picross.Game.Tests
{
    public class GameAPITest
    {
        private readonly GameAPI api;

        public GameAPITest()
        {
            api = new GameAPI(15, 15);
        }

        [Fact]
        public void TestEmptyStart()
        {
            for (int i = 0; i < api.Width; i++)
            {
                for (int j = 0; j < api.Height; j++)
                {
                    Assert.False(api.IsSquareFilled(i, j));
                    Assert.False(api.IsSquareCrossed(i, j));
                    Assert.True(api.IsSquareEmpty(i, j));
                }
            }
        }

        [Fact]
        public void TestFill()
        {
            Assert.True(api.IsSquareEmpty(0, 0));
            Assert.False(api.IsSquareFilled(0, 0));
            Assert.False(api.IsSquareCrossed(0, 0));

            api.FillCell(0, 0);

            Assert.False(api.IsSquareEmpty(0, 0));
            Assert.True(api.IsSquareFilled(0, 0));
            Assert.False(api.IsSquareCrossed(0, 0));
        }

        [Fact]
        public void TestCross()
        {
            Assert.True(api.IsSquareEmpty(0, 0));
            Assert.False(api.IsSquareCrossed(0, 0));
            Assert.False(api.IsSquareCrossed(0, 0));

            api.CrossCell(0, 0);

            Assert.False(api.IsSquareEmpty(0, 0));
            Assert.False(api.IsSquareFilled(0, 0));
            Assert.True(api.IsSquareCrossed(0, 0));
        }

        [Fact]
        public void TestEmpty()
        {
            Assert.True(api.IsSquareEmpty(0, 0));
            Assert.False(api.IsSquareFilled(0, 0));
            Assert.False(api.IsSquareCrossed(0, 0));

            api.EmptyCell(0, 0);

            Assert.True(api.IsSquareEmpty(0, 0));
            Assert.False(api.IsSquareFilled(0, 0));
            Assert.False(api.IsSquareCrossed(0, 0));

            // Also check that square is empty after it was filled in
            api.FillCell(0, 0);
            Assert.False(api.IsSquareEmpty(0, 0));
            api.EmptyCell(0, 0);
            Assert.True(api.IsSquareEmpty(0, 0));
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
            Assert.True(api.IsSquareEmpty(0, 0));
            Assert.False(api.IsSquareFilled(0, 0));
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
            Assert.True(api.IsSquareFilled(0, 0));
            Assert.False(api.IsSquareEmpty(0, 0));
        }
    }
}
