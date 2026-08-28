using System;
using System.Collections.Generic;
using System.Text;

namespace NonoSharp
{
    /// <summary>
    /// A singular hint, usually part of a group of multiple Hint instances contained in <see cref="Hints"/>.
    /// </summary>
    public class Hint : ICloneable
    {
        /// <summary>
        /// Amount of expected filled cells this singular hint corresponds to
        /// </summary>
        public int Number { get; }
        /// <summary>
        /// Whether this hint is marked as complete, i.e. this hint's corresponding cell group is correct.
        /// </summary>
        public bool Completed { get { return _completed; } }
        internal bool _completed;

        internal Hint(int num) {
            ArgumentOutOfRangeException.ThrowIfNegative(num);

            Number = num;
            if (Number == 0)
            {
                _completed = true;
            } else
            {
                _completed = false;
            }
        }

        private Hint(int num, bool completed)
        {
            this._completed = completed;
            this.Number = num;
        }

        /// <summary>
        /// Deep copies this instance.
        /// </summary>
        /// <returns>New, deep-copied instance of this instance</returns>
        public object Clone()
        {
            return new Hint(Number, Completed);
        }
    }
}
