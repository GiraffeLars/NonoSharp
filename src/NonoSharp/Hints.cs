using System;
using System.Collections.Generic;
using System.Text;

namespace NonoSharp
{
    /// <summary>
    /// The hints of a line. Each Hint/Clue in Hints can be accessed by using the indexer.
    /// </summary>
    [Obsolete("Hints has been renamed and is thus deprecated. Use Clues instead. Hints will be removed after v0.5.*")]
    public class Hints : Clues, IEnumerable<Hint>
    {
        internal Hints(bool isColumnClues, int position) : base(isColumnClues, position)
        {
        }

        internal Hints(Clues clues) : this(clues.colClues, clues.pos) { }

        IEnumerator<Hint> IEnumerable<Hint>.GetEnumerator()
        {
            for (int i = 0; i < clues.Count; i++)
            {
                yield return new Hint(clues[i]);
            }
        }

        /// <summary>
        /// Gets the <see cref="Hint"/> instance located at position <paramref name="index"/> in this instance.
        /// </summary>
        /// <param name="index">Hint to get</param>
        /// <returns><see cref="Hint"/> instance at <paramref name="index"/></returns>
        [Obsolete("Hints has been renamed and is thus deprecated, including the index getter." +
            "Use Clues instead. Hints will be removed after v0.5.*")]
        public new Hint this[int index]
        {
            get
            {
                return new(clues[index]);
            }
        }
    }
}
