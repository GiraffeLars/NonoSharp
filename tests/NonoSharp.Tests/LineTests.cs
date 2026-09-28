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
            var nonogram = new Nonogram(5, 5, new HashSet<CellPosition>());
            nonogram.FillCell(0, 1);
            nonogram.CrossCell(3, 0);

            Assert.Equal(LineType.Row, nonogram.Rows[0].Type);
            Assert.Equal(LineType.Column, nonogram.Columns[0].Type);
            Assert.Equal(CellType.Cross, nonogram.Rows[0][3]);
            Assert.Equal(CellType.Filled, nonogram.Columns[0][1]);
        }

        [Fact]
        public void TestCluesMatching()
        {
            var rowClues = Clues.FromString("1");
            var columnClues = Clues.FromString("1");

            var nonogram = new Nonogram(1, 1, [columnClues], [rowClues]);
            Assert.Same(nonogram.RowClues[0], nonogram.Rows[0].Clues);
            Assert.Same(nonogram.ColumnClues[0], nonogram.Columns[0].Clues);
        }

        [Fact]
        public void TestPosition()
        {
            var nonogram = new Nonogram(5, 5, new HashSet<CellPosition>());
            var row = nonogram.Rows[2];
            var column = nonogram.Columns[3];
            Assert.Equal(2, row.Position);
            Assert.Equal(3, column.Position);
        }

        [Fact]
        public void TestSetFromLine()
        {
            var nonogram = new Nonogram(5, 5, new HashSet<CellPosition>());
            var row = nonogram.Rows[1];
            var column = nonogram.Columns[2];
            row[2] = CellType.Filled;
            column[3] = CellType.Cross;
            Assert.Equal(CellType.Filled, nonogram.GetCell(2, 1));
            Assert.Equal(CellType.Cross, nonogram.GetCell(2, 3));

            // Test if these are properly added to the history
            nonogram.Undo();
            Assert.Equal(CellType.Empty, nonogram.GetCell(2, 3));
        }
    }
}
