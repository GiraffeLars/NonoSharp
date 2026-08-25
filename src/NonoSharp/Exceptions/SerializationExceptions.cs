using System;
using System.Collections.Generic;
using System.Text;

namespace NonoSharp.Exceptions
{
    /// <summary>
    /// Raised when a given file was not of the expected format and is thus incompatible.
    /// </summary>
    [Serializable]
    public class InvalidFileFormatException : Exception
    {
        internal InvalidFileFormatException()
        { }

        internal InvalidFileFormatException(string? message)
            : base(message)
        { }

        internal InvalidFileFormatException(string? message, Exception? innerException)
            : base(message, innerException)
        { }
    }

    /// <summary>
    /// Thrown when deserialization of a puzzle fails.
    /// </summary>
    [Serializable]
    public class PuzzleDeserializationFailedException : Exception
    {
        internal PuzzleDeserializationFailedException()
        { }

        internal PuzzleDeserializationFailedException(string? message)
            : base(message)
        { }

        internal PuzzleDeserializationFailedException(string? message, Exception? innerException)
            : base(message, innerException)
        { }
    }

    /// <summary>
    /// Thrown when serialization of a puzzle fails.
    /// </summary>
    [Serializable]
    public class PuzzleSerializationFailedException : Exception
    {
        internal PuzzleSerializationFailedException()
        { }

        internal PuzzleSerializationFailedException(string? message)
            : base(message)
        { }

        internal PuzzleSerializationFailedException(string? message, Exception? innerException)
            : base(message, innerException)
        { }
    }

    /// <summary>
    /// Thrown when a puzzle could not be saved to a file.
    /// </summary>
    [Serializable]
    public class PuzzleSavingFailedException : Exception
    {
        internal PuzzleSavingFailedException()
        { }

        internal PuzzleSavingFailedException(string? message)
            : base(message)
        { }

        internal PuzzleSavingFailedException(string? message, Exception? innerException)
            : base(message, innerException)
        { }
    }

    /// <summary>
    /// Thrown when the puzzle could not be loaded from e.g. a file.
    /// </summary>
    [Serializable]
    public class PuzzleLoadingFailedException : Exception
    {
        internal PuzzleLoadingFailedException()
        { }

        internal PuzzleLoadingFailedException(string? message)
            : base(message)
        { }

        internal PuzzleLoadingFailedException(string? message, Exception? innerException)
            : base(message, innerException)
        { }
    }
}
