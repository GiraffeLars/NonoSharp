using System;
using System.Collections.Generic;
using System.Text;

namespace NonoSharp.Exceptions
{
    /// <summary>
    /// Raised when a given file was not of the expected format and is thus incompatible.
    /// </summary>
    public class InvalidFileFormatException : Exception
    {
        /// <summary>
        /// Creates a new InvalidFileFormatException
        /// </summary>
        public InvalidFileFormatException()
        { }

        /// <summary>
        /// Creates a new InvalidFileFormatException with an error message <paramref name="message"/>.
        /// </summary>
        /// <param name="message">Error message to add</param>
        public InvalidFileFormatException(string? message)
            : base(message)
        { }

        /// <summary>
        /// Creates a new InvalidFileFormatException together with an error message <paramref name="message"/> and an
        /// inner exception <paramref name="innerException"/>.
        /// </summary>
        /// <param name="message">Error message to add</param>
        /// <param name="innerException">Inner exception to add</param>
        public InvalidFileFormatException(string? message, Exception? innerException)
            : base(message, innerException)
        { }
    }

    /// <summary>
    /// Thrown when deserialization of a puzzle fails.
    /// </summary>
    public class PuzzleDeserializationFailedException : Exception
    {
        /// <summary>
        /// Creates a new PuzzleDeserializationFailedException
        /// </summary>
        public PuzzleDeserializationFailedException()
        { }

        /// <summary>
        /// Creates a new PuzzleDeserializationFailedException with an error message <paramref name="message"/>.
        /// </summary>
        /// <param name="message">Error message to add</param>
        public PuzzleDeserializationFailedException(string? message)
            : base(message)
        { }

        /// <summary>
        /// Creates a new PuzzleDeserializationFailedException together with an error message <paramref name="message"/> and an
        /// inner exception <paramref name="innerException"/>.
        /// </summary>
        /// <param name="message">Error message to add</param>
        /// <param name="innerException">Inner exception to add</param>
        public PuzzleDeserializationFailedException(string? message, Exception? innerException)
            : base(message, innerException)
        { }
    }

    /// <summary>
    /// Thrown when serialization of a puzzle fails.
    /// </summary>
    public class PuzzleSerializationFailedException : Exception
    {
        /// <summary>
        /// Creates a new PuzzleSerializationFailedException
        /// </summary>
        public PuzzleSerializationFailedException()
        { }

        /// <summary>
        /// Creates a new PuzzleSerializationFailedException with an error message <paramref name="message"/>.
        /// </summary>
        /// <param name="message">Error message to add</param>
        public PuzzleSerializationFailedException(string? message)
            : base(message)
        { }

        /// <summary>
        /// Creates a new PuzzleSerializationFailedException together with an error message <paramref name="message"/> and an
        /// inner exception <paramref name="innerException"/>.
        /// </summary>
        /// <param name="message">Error message to add</param>
        /// <param name="innerException">Inner exception to add</param>
        public PuzzleSerializationFailedException(string? message, Exception? innerException)
            : base(message, innerException)
        { }
    }

    /// <summary>
    /// Thrown when a puzzle could not be saved to a file.
    /// </summary>
    public class PuzzleSavingFailedException : Exception
    {
        /// <summary>
        /// Creates a new PuzzleSavingFailedException
        /// </summary>
        public PuzzleSavingFailedException()
        { }

        /// <summary>
        /// Creates a new PuzzleSavingFailedException with an error message <paramref name="message"/>.
        /// </summary>
        /// <param name="message">Error message to add</param>
        public PuzzleSavingFailedException(string? message)
            : base(message)
        { }

        /// <summary>
        /// Creates a new PuzzleSavingFailedException together with an error message <paramref name="message"/> and an
        /// inner exception <paramref name="innerException"/>.
        /// </summary>
        /// <param name="message">Error message to add</param>
        /// <param name="innerException">Inner exception to add</param>
        public PuzzleSavingFailedException(string? message, Exception? innerException)
            : base(message, innerException)
        { }
    }

    /// <summary>
    /// Thrown when the puzzle could not be loaded from e.g. a file.
    /// </summary>
    public class PuzzleLoadingFailedException : Exception
    {
        /// <summary>
        /// Creates a new PuzzleLoadingFailedException
        /// </summary>
        public PuzzleLoadingFailedException()
        { }

        /// <summary>
        /// Creates a new PuzzleLoadingFailedException with an error message <paramref name="message"/>.
        /// </summary>
        /// <param name="message">Error message to add</param>
        public PuzzleLoadingFailedException(string? message)
            : base(message)
        { }

        /// <summary>
        /// Creates a new PuzzleLoadingFailedException together with an error message <paramref name="message"/> and an
        /// inner exception <paramref name="innerException"/>.
        /// </summary>
        /// <param name="message">Error message to add</param>
        /// <param name="innerException">Inner exception to add</param>
        public PuzzleLoadingFailedException(string? message, Exception? innerException)
            : base(message, innerException)
        { }
    }
}
