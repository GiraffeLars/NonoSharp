using System;
using System.Collections.Generic;
using System.Text;

namespace NonoSharp.Exceptions
{
    /// <summary>
    /// Raised when a built puzzle is not solvable but a playable instance was requested.
    /// </summary>
    public class PuzzleNotSolvableException : Exception
    {
        /// <summary>
        /// Creates a new PuzzleNotSolvableException
        /// </summary>
        public PuzzleNotSolvableException()
        { }

        /// <summary>
        /// Creates a new PuzzleNotSolvableException with an error message <paramref name="message"/>.
        /// </summary>
        /// <param name="message">Error message to add</param>
        public PuzzleNotSolvableException(string? message)
            : base(message)
        { }

        /// <summary>
        /// Creates a new PuzzleNotSolvableException together with an error message <paramref name="message"/> and an
        /// inner exception <paramref name="innerException"/>.
        /// </summary>
        /// <param name="message">Error message to add</param>
        /// <param name="innerException">Inner exception to add</param>
        public PuzzleNotSolvableException(string? message, Exception? innerException)
            : base(message, innerException)
        { }
    }
}
