using System;
using System.Collections.Generic;
using System.Text;

namespace NonoSharp
{
    /// <summary>
    /// A singular clue, usually part of a group of multiple Clue instances contained in <see cref="Clues"/>.
    /// </summary>
    public class Clue : ICloneable
    {
        /// <summary>
        /// Amount of expected filled cells this singular clue corresponds to
        /// </summary>
        public int Number { get; }
        /// <summary>
        /// Whether this clue is marked as complete, i.e. this clue's corresponding cell group is correct.
        /// </summary>
        public bool Completed { get { return _completed; } }
        internal bool _completed;

        internal Clue(int num) {
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

        private Clue(int num, bool completed)
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
            return new Clue(Number, Completed);
        }
    }
}
