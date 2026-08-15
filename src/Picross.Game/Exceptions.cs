using System;
using System.Collections.Generic;
using System.Text;

namespace Picross.Game
{
    [Serializable]
    public class InvalidFileFormatException : Exception
    {
        public InvalidFileFormatException()
        { }

        public InvalidFileFormatException(string? message)
            : base(message)
        { }

        public InvalidFileFormatException(string? message, Exception? innerException)
            : base(message, innerException)
        { }
    }

    [Serializable]
    public class PuzzleDeserializationFailedException : Exception
    {
        public PuzzleDeserializationFailedException()
        { }

        public PuzzleDeserializationFailedException(string? message)
            : base(message)
        { }

        public PuzzleDeserializationFailedException(string? message, Exception? innerException)
            : base(message, innerException)
        { }
    }

    [Serializable]
    public class PuzzleSerializationFailedException : Exception
    {
        public PuzzleSerializationFailedException()
        { }

        public PuzzleSerializationFailedException(string? message)
            : base(message)
        { }

        public PuzzleSerializationFailedException(string? message, Exception? innerException)
            : base(message, innerException)
        { }
    }

    [Serializable]
    public class PuzzleSavingFailedException : Exception
    {
        public PuzzleSavingFailedException()
        { }

        public PuzzleSavingFailedException(string? message)
            : base(message)
        { }

        public PuzzleSavingFailedException(string? message, Exception? innerException)
            : base(message, innerException)
        { }
    }

    [Serializable]
    public class PuzzleLoadingFailedException : Exception
    {
        public PuzzleLoadingFailedException()
        { }

        public PuzzleLoadingFailedException(string? message)
            : base(message)
        { }

        public PuzzleLoadingFailedException(string? message, Exception? innerException)
            : base(message, innerException)
        { }
    }
}
