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
        public void TestHintFilledIn()
        {

        }
    }
}
