namespace NonoSharp.Tests
{
    public class GridTests
    {
        private readonly Grid grid;
        public GridTests() 
        {
            grid = new(5, 5);
        }

        [Fact]
        public void TestSetCell()
        {
            foreach (CellType type in Enum.GetValues<CellType>())
            {
                grid.SetCell(0, 0, type);
                Assert.Equal(type, grid[0, 0]);
            }
        }

        [Fact]
        public void TestSetCellOutOfBoundsExceptions()
        {
            Action[] acts = [
                () => grid.SetCell(-1, 0, CellType.Filled),
                () => grid.SetCell(grid.Width, 0, CellType.Filled),
                () => grid.SetCell(0, -1, CellType.Filled),
                () => grid.SetCell(0, grid.Height, CellType.Filled),
                () => grid.SetCell(-1, -1, CellType.Filled)
            ];

            foreach (Action act in acts)
            {
                Assert.Throws<ArgumentOutOfRangeException>(act);
            }
        }

        [Fact]
        public void TestGetCell()
        {
            grid[0, 0] = CellType.Filled;
            Assert.Equal(CellType.Filled, grid.GetCell(0, 0));
        }

        [Fact]
        public void TestGetCellOutOfBoundsExceptions()
        {
            Action[] acts = [
                () => grid.GetCell(-1, 0),
                () => grid.GetCell(grid.Width, 0),
                () => grid.GetCell(0, -1),
                () => grid.GetCell(0, grid.Height),
                () => grid.GetCell(-1, -1)
            ];

            foreach (Action act in acts)
            {
                Assert.Throws<ArgumentOutOfRangeException>(act);
            }
        }

        [Fact]
        public void TestFilledCorrect()
        {
            Assert.Equal(0, grid.Filled);
            grid.SetCell(0, 0, CellType.Filled);
            Assert.Equal(1, grid.Filled);
        }

        [Fact]
        public void TestGroups()
        {
            grid.SetCell(0, 0, CellType.Filled);
            grid.SetCell(2, 0, CellType.Filled);
            grid.SetCell(3, 0, CellType.Filled);

            LinkedList<int> groups = grid.GetGroupsInRow(0);
            Assert.Equal(2, groups.Count);
            Assert.Equal(1, groups.First());
            Assert.Equal(2, groups.Last());
        }
    }
}
