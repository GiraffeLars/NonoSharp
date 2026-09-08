using System;
using System.Collections.Generic;
using System.Text;

namespace NonoSharp
{
    /// <summary>
    /// The hints of a line. Each Hint/Clue in Hints can be accessed by using the indexer.
    /// </summary>
    [Obsolete("Hints has been renamed and is thus deprecated. Use Clues instead. Hints will be removed after v0.5.*")]
    public class Hints : Clues
    {
        internal Hints(bool isColumnClues, int position) : base(isColumnClues, position)
        {
        }

        internal Hints(Clues clues) : this(clues.colClues, clues.pos) { }
    }
}
