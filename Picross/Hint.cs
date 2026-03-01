using System;
using System.Collections.Generic;
using System.Text;

namespace Core
{
    public class Hint
    {
        public int number { get; }
        public bool completed { get { return _completed; } }
        internal bool _completed;

        internal Hint(int num) {
            ArgumentOutOfRangeException.ThrowIfNegative(num);

            number = num;
            if (number == 0)
            {
                _completed = true;
            } else
            {
                _completed = false;
            }
        }
    }
}
