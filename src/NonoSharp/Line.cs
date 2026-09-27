using NonoSharp.Enums;
using System;
using System.Collections;

namespace NonoSharp
{
    /// <summary>
    /// A line of a <see cref="NonogramAPI"/> puzzle.
    /// </summary>
    public class Line : IEnumerable<CellType>
    {
        /// <summary>
        /// The index of this line on the associated nonogram grid.
        /// </summary>
        public int Position { get; }

        /// <summary>
        /// The type of this line.
        /// </summary>
        public LineType Type { get; }

        /// <summary>
        /// The total amount of cells this line has.
        /// </summary>
        public int Length => Type == LineType.Column ? nonogram.Height : nonogram.Width;

        /// <summary>
        /// The <see cref="NonoSharp.Clues"/> of the line.
        /// </summary>
        public Clues Clues => Type == LineType.Column ? nonogram.ColumnClues[Position] : nonogram.RowClues[Position];

        private readonly NonogramAPI nonogram;

        internal Line(int position, LineType type, NonogramAPI nonogram)
        {
            Position = position;
            Type = type;
            this.nonogram = nonogram;
        }

        public CellType this[int index]
        {
            get
            {
                return Type == LineType.Column ?
                    nonogram.GetCell(Position, index) : nonogram.GetCell(index, Position); 
            }
            set
            {
                if (Type == LineType.Column)
                {
                    nonogram.SetCell(Position, index, value);
                } else
                {
                    nonogram.SetCell(index, Position, value);
                }
            }
        }

        /// <summary>
        /// Gets the enumerator over the cells in this line.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<CellType> GetEnumerator()
        {
            for (int i = 0; i < Length; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
