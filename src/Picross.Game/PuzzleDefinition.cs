using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;

namespace Picross.Game
{
    internal class PuzzleDefinition
    {
        internal const string MAGIC = "NONO"; // To check file format, in ASCII
        public static int Version { get; } = 0;

        /// <summary>
        /// Title of the puzzle
        /// </summary>
        public string? Title { get; private set; }
        public const int MAX_TITLE_LENGTH = 100;
        public int Width { get; private set; }
        public int Height { get; private set; }

        /// <summary>
        /// The solution in bool, going from left-to-right, top-to-bottom.
        /// </summary>
        public bool[] Solution { get; private set; }
        // Bools take 1 bit, ints 32. Depending on the density and dimensions of the puzzle, one can be more optimal for storage space
        // but considering that usually >50% of cells are filled, storing the expected state of each cell is very defendable

        /// <summary>
        /// Creates a puzzle definition
        /// </summary>
        /// <param name="width">Width of the puzzle</param>
        /// <param name="height">Height of the puzzle</param>
        /// <param name="solution">The solution, of length width * height, where for each filled cell in the solution, the array's element is true and false otherwise</param>
        /// <param name="title">Optional title of the puzzle</param>
        /// <exception cref="ArgumentException">Thrown when <code>solution.Length != width * height</code></exception>
        /// <exception cref="OverflowException">Thrown when calculating <paramref name="width"/> * <paramref name="height"/> overflows.</exception>
        internal PuzzleDefinition(int width, int height, bool[] solution, string? title = null)
        {
            if (solution.Length != checked(width * height))
            {
                throw new ArgumentException("The length of solution must match the total number of cells (width * height)!");
            }

            this.Width = width;
            this.Height = height;
            this.Solution = solution;
            this.Title = title;
        }

        /// <summary>
        /// Creates a puzzle definition
        /// </summary>
        /// <param name="width">Width of the puzzle</param>
        /// <param name="height">Height of the puzzle</param>
        /// <param name="solution">The solution, of length width * height, where for each filled cell in the solution, there is a Point element in the list of the marked coordinates</param>
        /// <param name="title">Optional title of the puzzle</param>
        /// <exception cref="ArgumentException">Thrown when there are more filled cells in <paramref name="solution"/> than possible given <paramref name="width"/> and <paramref name="height"/></exception>
        /// <exception cref="OverflowException">Thrown when calculating <paramref name="width"/> * <paramref name="height"/> overflows.</exception>
        internal PuzzleDefinition(int width, int height, List<Point> solution, string? title = null) : 
            this(width, height, ConvertPointSolutionToBools(width, height, solution), title) { }

        public static bool[] ConvertPointSolutionToBools(int width, int height, List<Point> solution)
        {
            bool[] boolSol = new bool[width * height];

            foreach (Point p in solution)
            {
                // Set the corresponding location of the point in boolSol to true
                int loc = p.X + p.Y * width;
                boolSol[loc] = true;
            }

            return boolSol;
        }

        internal List<Point> ConvertBoolSolutionToPoints()
        { 
            List<Point> points = new List<Point>();
            for (int i = 0; i < Solution.Length; i++)
            {
                if (Solution[i])
                {
                    int x = i % Width;
                    int y = i / Height;
                    points.Add(new(x, y));
                }
            }
            return points;
        }

        /// <summary>
        /// Serializes this instance
        /// </summary>
        /// <returns>Array of bytes which represent a serialized PuzzleDefinition and can be reconstructed to be equal to this instance</returns>
        /// <exception cref="PuzzleSerializationFailedException">Thrown when serialization is fails. For example, when the given title is too long, or an I/O exception occurs</exception>
        public byte[] Serialize()
        {
            MemoryStream ms = new();

            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write(Encoding.ASCII.GetBytes(MAGIC));
            bw.Write(Version);

            if (Title != null && Title.Length > MAX_TITLE_LENGTH)
            {
                throw new PuzzleSerializationFailedException($"The title is too long! Should be at most {MAX_TITLE_LENGTH} characters!");
            }

            try
            {
                bw.Write(Title ?? "");

                bw.Write(Width);
                bw.Write(Height);

                bw.Write(ConvertSolutionToBytes());
            }
            catch (IOException e)
            {
                throw new PuzzleSerializationFailedException("Encountered an I/O error while serializing puzzle!", e);
            }
            catch (ObjectDisposedException e)
            {
                // Shouldn't happen, here for completeness
                throw new PuzzleSerializationFailedException("The BinaryWriter was pre-emptively closed!", e);
            }
            catch (ArgumentNullException e)
            {
                // Shouldn't happen, here for completeness
                throw new PuzzleSerializationFailedException("Received a null argument!", e);
            }
            finally
            {
                bw.Close();
                ms.Close();
            }

            return ms.ToArray();
        }

        /// <summary>
        /// Converts the solution, in instance variable <see cref="Solution"/>, to bytes
        /// </summary>
        /// <returns>Byte array of the solution</returns>
        internal byte[] ConvertSolutionToBytes()
        {
            byte[] bytes = new byte[ConvertBitCountToByteCount(Solution.Length)];

            for (int i = 0; i < Solution.Length; i++)
            {
                // Append the solution bit to the byte of its location
                // Both arrays read from left to right, as done by the << operator and 7 - i%8, resulting in the first
                // character being the leading character in the byte as well, etc.

                // Check if the solution is true, otherwise skip this appending step, as it is already 0 by default
                if (Solution[i])
                {
                    bytes[i / 8] |= (byte) (1 << 7 - i % 8);
                }
            }
            return bytes;
        }

        /// <summary>
        /// Deserializes <paramref name="serializedPuzzle"/> into a <c>PuzzleDefinition</c>
        /// </summary>
        /// <param name="serializedPuzzle">Puzzle to deserialize</param>
        /// <returns></returns>
        /// <exception cref="InvalidFileFormatException">Thrown when the given file format is not supported</exception>
        /// <exception cref="NotSupportedException">Thrown when the version of the save system is not supported</exception>
        /// <exception cref="PuzzleDeserializationFailedException">Thrown when the file format is valid, but other issues occur. Usually, there is an
        /// inner exception giving more details.</exception>
        public static PuzzleDefinition Deserialize(byte[] serializedPuzzle)
        {
            MemoryStream ms = new(serializedPuzzle);
            BinaryReader br = new BinaryReader(ms);

            try
            {
                string readMagic = Encoding.ASCII.GetString(br.ReadBytes(Encoding.ASCII.GetByteCount(MAGIC)));

                if (readMagic != MAGIC)
                {
                    throw new InvalidFileFormatException("The provided file is not supported!");
                }
            } 
            catch (Exception e)
            {
                br.Close();
                ms.Close();

                if (e is InvalidFileFormatException)
                {
                    throw;
                }
                // Catch all exceptions possibly thrown by the read magic line
                else if (e is ArgumentException || e is ArgumentNullException || e is DecoderFallbackException
                    || e is IOException || e is ObjectDisposedException || e is ArgumentOutOfRangeException ||
                    e is EncoderFallbackException)
                {
                    throw new InvalidFileFormatException("The provided file is not supported");
                } else
                {
                    // In case of missed exceptions
                    throw;
                }
            }

            try
            {
                int readVersion = br.ReadInt32();
                if (readVersion != Version)
                {
                    throw new NotSupportedException(
                        $"The given puzzle's version ({readVersion}) does not match the current version ({Version})!");
                }

                string? readTitle = br.ReadString();
                if (readTitle.Length > MAX_TITLE_LENGTH)
                {
                    throw new InvalidFileFormatException("The provided file is not supported!");
                } else if (readTitle == "")
                {
                    readTitle = null;
                }

                int readWidth = br.ReadInt32();
                int readHeight = br.ReadInt32();

                if (readWidth <= 0 || readHeight <= 0)
                {
                    throw new InvalidFileFormatException("The provided file is not supported!");
                }

                // The expected total of bytes to read for the puzzle. This is the total number of cells (w * h), converted to bytes, rounded up
                int remainingByteCount = ConvertBitCountToByteCount(checked(readWidth * readHeight));

                // Read the bytes of the puzzle. These are the total number of cells converted to bytes, round up
                byte[] remaining = br.ReadBytes(remainingByteCount);

                if (ms.Position != ms.Length || remaining.Length < remainingByteCount)
                {
                    // Check for extra or missing data after the file should have been fully read
                    // If that is the case, this is not a supported file as the dimensions do not match
                    throw new InvalidFileFormatException("The provided file is not supported!");
                }

                bool[] readSolution = ConvertBytesToSolution(remaining, readWidth, readHeight);
                return new(readWidth, readHeight, readSolution, readTitle);
            }
            catch (InvalidFileFormatException)
            {
                throw;
            }
            catch (NotSupportedException)
            {
                throw;
            }
            catch (ObjectDisposedException e)
            {
                // Shouldn't happen, here for completeness
                throw new PuzzleDeserializationFailedException("The BinaryReader was pre-emptively closed!", e);
            }
            catch (ArgumentNullException e)
            {
                // Shouldn't happen, here for completeness
                throw new PuzzleDeserializationFailedException("Received a null argument!", e);
            }
            catch (EndOfStreamException)
            {
                throw new InvalidFileFormatException("The given puzzle's data is incomplete!");
            }

            catch (OverflowException)
            {
                throw new PuzzleDeserializationFailedException("The puzzle dimensions are too large!");
            }
            finally
            {
                br.Close();
                ms.Close();
            }
        }

        /// <summary>
        /// Converts bytes to a solution array. Reverse of <see cref="ConvertSolutionToBytes"/>
        /// </summary>
        /// <param name="bytes">Array of bytes to convert</param>
        /// <param name="width">Width of the puzzle to convert</param>
        /// <param name="height">Height of the puzzle to convert</param>
        /// <returns>Array of bools where for each location with an expected filled cell, the corresponding index is set</returns>
        /// <exception cref="OverflowException">Thrown when the puzzle dimension are too large to calculate the number of cells</exception>
        internal static bool[] ConvertBytesToSolution(byte[] bytes, int width, int height)
        {
            int totalCells = checked(width * height);
            bool[] sol = new bool[totalCells];

            for (int i = 0; i < sol.Length; i++)
            {
                byte b = bytes[i / 8];

                // Check if bit at corresponding position is set (this num is 0 if they do not match, otherwise, this is 2^x)
                if ((b & (byte)(1 << 7 - i % 8)) != 0) {
                    sol[i] = true;
                }
            }
            return sol;
        }

        /// <summary>
        /// Saves this puzzle at <paramref name="path"/>. Specifically, the expected solution and dimension are stored.
        /// See also <seealso cref="File.WriteAllBytes(string, byte[])"/>.
        /// </summary>
        /// <param name="path">The path to save the puzzle to</param>
        /// <exception cref="PuzzleSerializationFailedException">Thrown when serialization is fails. For example, when the given title is too long, or an I/O exception occurs.
        /// Usually, there is an inner exception giving more details.</exception>
        /// <exception cref="PuzzleSavingFailedException">Thrown when saving files fails, e.g. because of an I/O Exception. See the inner exception for more details</exception>
        public void SavePuzzle(string path)
        {
            byte[] serialized = Serialize();

            try
            {
                File.WriteAllBytes(path, serialized);
            }
            catch (Exception e)
            {
                throw new PuzzleSavingFailedException("Failed to save puzzle!", e);
            }
        }


        /// <summary>
        /// Saves this puzzle asynchronously at <paramref name="path"/>. Specifically, the expected solution and dimension are stored.
        /// See also <seealso cref="File.WriteAllBytesAsync(string, byte[], CancellationToken)"/>.
        /// </summary>
        /// <param name="path">The path to save the puzzle to</param>
        /// <exception cref="PuzzleSerializationFailedException">Thrown when serialization is fails. For example, when the given title is too long, or an I/O exception occurs.
        /// Usually, there is an inner exception giving more details.</exception>
        /// <exception cref="PuzzleSavingFailedException">Thrown when saving files fails, e.g. because of an I/O Exception. See the inner exception for more details</exception>
        public async Task SavePuzzleAsync(string path)
        {
            byte[] serialized = Serialize();
            // No SerializeAsync method as puzzles are usually small in terms of bytes.
            // This is also the reason that there are no async binarywriter/readers available in .NET

            try
            {
                await File.WriteAllBytesAsync(path, serialized);
            }
            catch (Exception e)
            {
                throw new PuzzleSavingFailedException("Failed to save puzzle!", e);
            }
        }

        /// <summary>
        /// Loads the puzzle located at <paramref name="path"/>
        /// </summary>
        /// <param name="path">Path of the puzzle to load</param>
        /// <returns>A PuzzleDefinition of the requested puzzle, if available</returns>
        /// <exception cref="InvalidFileFormatException">Thrown when the given file format is not supported</exception>
        /// <exception cref="NotSupportedException">Thrown when the version of the save system is not supported</exception>
        /// <exception cref="PuzzleLoadingFailedException">Thrown when loading files fails, e.g. because of an I/O Exception. See the inner exception for more details</exception>
        public static PuzzleDefinition LoadPuzzle(string path)
        {
            byte[] serializedPuzzle;

            try
            {
                serializedPuzzle = File.ReadAllBytes(path);
            }
            catch (Exception e)
            {
                throw new PuzzleLoadingFailedException("Failed to load puzzle!", e);
            }
            return Deserialize(serializedPuzzle);
        }


        /// <summary>
        /// Loads the puzzle located at <paramref name="path"/> asynchronously
        /// </summary>
        /// <param name="path">Path of the puzzle to load</param>
        /// <returns>A PuzzleDefinition of the requested puzzle, if available</returns>
        /// <exception cref="InvalidFileFormatException">Thrown when the given file format is not supported</exception>
        /// <exception cref="NotSupportedException">Thrown when the version of the save system is not supported</exception>
        /// <exception cref="PuzzleLoadingFailedException">Thrown when loading files fails, e.g. because of an I/O Exception. See the inner exception for more details</exception>
        public static async Task<PuzzleDefinition> LoadPuzzleAsync(string path)
        {
            byte[] serializedPuzzle;

            try
            {
                serializedPuzzle = await File.ReadAllBytesAsync(path);
            }
            catch (Exception e)
            {
                throw new PuzzleLoadingFailedException("Failed to load puzzle!", e);
            }
            return Deserialize(serializedPuzzle);
        }

        /// <summary>
        /// Converts <paramref name="count"/> to how many bytes these would take to store, rounded upwards
        /// </summary>
        /// <param name="count">Total amount of bits to convert. To avoid floating point issues, must be non-negative</param>
        /// <returns>The total amount of bytes these bits take. Rounded up.</returns>
        private static int ConvertBitCountToByteCount(int count)
        {
            return (int)Math.Ceiling((double)count / 8);
        }
    }

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
    public class InvalidPuzzleException : Exception 
    {
        public InvalidPuzzleException()
        { }

        public InvalidPuzzleException(string? message)
            : base(message)
        { }

        public InvalidPuzzleException(string? message, Exception? innerException)
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
