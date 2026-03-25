using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
    public class Hint
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
    }
}
