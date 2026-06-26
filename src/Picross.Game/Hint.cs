using System;
using System.Collections.Generic;
using System.Text;

namespace Picross.Game
{
    public class Hint : ICloneable
    {
        public int Number { get; }
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

        public object Clone()
        {
            return new Hint(Number, Completed);
        }
    }
}
