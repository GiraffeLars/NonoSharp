using System;
using System.Collections.Generic;
using System.Text;

namespace NonoSharp
{
    /// <summary>
    /// Deprecated: Use <see cref="Clue"/> instead.
    /// A singular hint, usually part of a group of multiple Hint instances contained in <see cref="Hints"/>.
    /// </summary>
    [Obsolete("Hint has been renamed and is thus deprecated. Use Clue instead. Hint will be removed after v0.5.*")]
    public class Hint : Clue
    {
        internal Hint(int num) : base(num)
        {
        }
    }
}
