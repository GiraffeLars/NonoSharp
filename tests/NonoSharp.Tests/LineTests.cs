using System;
using System.Collections.Generic;
using System.Text;

namespace NonoSharp.Tests
{
    public class LineTests
    {
        [Fact]
        public void TestColumnAndRowGetters()
        {
            var api = new NonogramAPI(5, 5, new HashSet<CellPosition>());
            api.FillCell(0, 1);
            api.CrossCell(3, 0);

            Assert.Equal(LineType.Row, api.Rows[0].Type);
            Assert.Equal(LineType.Column, api.Columns[0].Type);
            Assert.Equal(CellType.Cross, api.Rows[0][3]);
            Assert.Equal(CellType.Filled, api.Columns[0][1]);
        }

        [Fact]
        public void TestCluesMatching()
        {
            var rowClues = Clues.FromString("1");
            var columnClues = Clues.FromString("1");

            var nonogram = new NonogramAPI(1, 1, [columnClues], [rowClues]);
            Assert.Same(nonogram.RowClues[0], nonogram.Rows[0].Clues);
            Assert.Same(nonogram.ColumnClues[0], nonogram.Columns[0].Clues);
        }

        [Fact]
        public void TestPosition()
        {
            var api = new NonogramAPI(5, 5, new HashSet<CellPosition>());
            var row = api.Rows[2];
            var column = api.Columns[3];
            Assert.Equal(2, row.Position);
            Assert.Equal(3, column.Position);
        }

        [Fact]
        public void TestSetFromLine()
        {
            var api = new NonogramAPI(5, 5, new HashSet<CellPosition>());
            var row = api.Rows[1];
            var column = api.Columns[2];
            row[2] = CellType.Filled;
            column[3] = CellType.Cross;
            Assert.Equal(CellType.Filled, api.GetCell(2, 1));
            Assert.Equal(CellType.Cross, api.GetCell(2, 3));

            // Test if these are properly added to the history
            api.Undo();
            Assert.Equal(CellType.Empty, api.GetCell(2, 3));
        }
    }
}
