using NonoSharp.Exceptions;
using System.Text;

namespace NonoSharp
{
    internal static class PuzzleSerializer
    {
        internal const string MAGIC = "NONO"; // To check file format, in ASCII
        public static int Version { get; } = 0;

        /// <summary>
        /// Serializes <paramref name="puzzle"/>.
        /// </summary>
        /// <returns>Array of bytes which represent a serialized PuzzleDefinition and can be reconstructed to be equal to this instance</returns>
        /// <exception cref="PuzzleSerializationFailedException">Thrown when serialization fails. 
        /// For example, when the given title is too long, or an I/O exception occurs</exception>
        /// <returns>Serialized puzzle in an array of bytes</returns>
        public static byte[] Serialize(PuzzleDefinition puzzle)
        {
            int totalCells;
            try
            {
                totalCells = checked(puzzle.Width * puzzle.Height);
            } catch (OverflowException)
            {
                throw new PuzzleSerializationFailedException("This puzzle is too large to serialize!");
            }

            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true);

            bw.Write(Encoding.ASCII.GetBytes(MAGIC));
            bw.Write(Version);

            if (puzzle.Title != null && puzzle.Title.Length > PuzzleDefinition.MAX_TITLE_LENGTH)
            {
                throw new PuzzleSerializationFailedException($"The title is too long! " +
                    $"Should be at most {PuzzleDefinition.MAX_TITLE_LENGTH} characters!");
            }

            try
            {
                bw.Write(puzzle.Title ?? "");

                bw.Write(puzzle.Width);
                bw.Write(puzzle.Height);

                bw.Write(ConvertPuzzleSolutionToBytes(puzzle, totalCells));
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
            catch (InvalidOperationException)
            {
                throw new PuzzleSerializationFailedException("This is not a serializable puzzle!");
            }
            catch (ArgumentNullException e)
            {
                // Shouldn't happen, here for completeness
                throw new PuzzleSerializationFailedException("Received a null argument!", e);
            }

            return ms.ToArray();
        }

        /// <summary>
        /// Converts <paramref name="puzzle"/>'s solution to bytes.
        /// </summary>
        /// <returns>Byte array of the solution</returns>
        /// <param name="puzzle">Puzzle containing solution to serialize.</param>
        /// <param name="totalCells">Total amount of cells in the puzzle that the solution is part of.</param>
        internal static byte[] ConvertPuzzleSolutionToBytes(PuzzleDefinition puzzle, int totalCells)
        {
            if (puzzle.Solution == null) throw new InvalidOperationException("There is no solution to serialize!");

            // Bools take 1 bit, ints 32. Depending on the density and dimensions of the puzzle,
            // one can be more optimal for storage space but considering that usually >50% of cells are filled,
            // storing the expected state of each cell seems to be the way to go

            byte[] bytes = new byte[ConvertBitCountToByteCount(totalCells)];

            foreach (CellPosition position in puzzle.Solution)
            {
                int cellIndex = position.X + position.Y * puzzle.Width;
                int byteIndex = cellIndex / 8;

                // Set the solution bit at the byte of its location
                // The byte array reads from left to right. Hence, we need to shift the positive bit
                // with the << operator by 7 - cellIndex%8. This ensures we keep the left to right reading direction.
                bytes[byteIndex] |= (byte)(1 << 7 - cellIndex % 8);
            }
            return bytes;
        }

        /// <summary>
        /// Deserializes <paramref name="stream"/> into a <c>PuzzleDefinition</c>.
        /// </summary>
        /// <remarks>
        /// <paramref name="stream"/> is left open. Do not forget to close it.
        /// </remarks>
        /// <param name="stream">Stream to read.</param>
        /// <returns><c>PuzzleDefinition</c> loaded from <paramref name="stream"/>.</returns>
        /// <exception cref="InvalidFileFormatException">Thrown when the given file format is not supported.</exception>
        /// <exception cref="NotSupportedException">Thrown when the version of the save system is not supported.</exception>
        /// <exception cref="PuzzleDeserializationFailedException">Thrown when the file format is valid, but other issues occur. Usually, there is an
        /// inner exception giving more details.</exception>
        public static PuzzleDefinition Deserialize(Stream stream)
        {
            using var br = new BinaryReader(stream, Encoding.UTF8, leaveOpen: true);
            ValidateMagic(br);

            try
            {
                int readVersion = br.ReadInt32();
                if (readVersion != Version)
                {
                    throw new NotSupportedException(
                        $"The given puzzle's version ({readVersion}) does not match the current version ({Version})!");
                }

                string? readTitle = br.ReadString();
                if (readTitle.Length > PuzzleDefinition.MAX_TITLE_LENGTH)
                {
                    throw new InvalidFileFormatException("The provided file is not supported!");
                }
                else if (readTitle == "")
                {
                    readTitle = null;
                }

                int readWidth = br.ReadInt32();
                int readHeight = br.ReadInt32();

                if (readWidth <= 0 || readHeight <= 0)
                {
                    throw new InvalidFileFormatException("The provided file's puzzle dimensions are invalid!");
                }

                // The expected total of bytes to read for the puzzle. This is the total number of cells (w * h), converted to bytes, rounded up
                int remainingByteCount = ConvertBitCountToByteCount(checked(readWidth * readHeight));

                // Read the bytes of the puzzle. These are the total number of cells converted to bytes, round up
                byte[] remaining = br.ReadBytes(remainingByteCount);

                // Only check if the given input was shorter than expected for a correct file
                // Do not check if there are remaining elements in the stream, as we will not be using them either way
                // and Stream.Position is not supported by all streams
                if (remaining.Length < remainingByteCount)
                {
                    // Check for extra or missing data after the file should have been fully read
                    // If that is the case, this is not a supported file as the dimensions do not match
                    throw new InvalidFileFormatException("The provided file is not supported!");
                }

                HashSet<CellPosition> readSolution = ConvertBytesToSolution(remaining, readWidth, readHeight);
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
        }

        /// <summary>
        /// Deserializes <paramref name="serializedPuzzle"/> into a <c>PuzzleDefinition</c>
        /// </summary>
        /// <param name="serializedPuzzle">Puzzle to deserialize</param>
        /// <returns>PuzzleDefinition deserialized from <paramref name="serializedPuzzle"/>.</returns>
        /// <exception cref="InvalidFileFormatException">Thrown when the given file format is not supported</exception>
        /// <exception cref="NotSupportedException">Thrown when the version of the save system is not supported</exception>
        /// <exception cref="PuzzleDeserializationFailedException">Thrown when the file format is valid, but other issues occur. Usually, there is an
        /// inner exception giving more details.</exception>
        public static PuzzleDefinition Deserialize(byte[] serializedPuzzle)
        {
            // A note on asynchronous puzzle deserialization:
            /* As also noted on https://stackoverflow.com/questions/10315316/asynchronous-binaryreader-and-binarywriter-in-net
             * content read/written using BinaryReader/Writer is usually very small. This is also the case for our puzzle format, each cell only taking 1 bit,
             * plus some extra metadata. Loading the file can take longer, and should therefore be loaded before hand using an async read.
             * As the size of this is expected to be small, using blocking Deserialize is fine for now. But as larger files can cause the thread to be blocked, a disclaimer
             * is added in the documentation of the library. 
             * This is also why async reading from a stream is not supported in Serializer, as we'd only make a call to a blocking method in an async method, which is silly */


            using var ms = new MemoryStream(serializedPuzzle);

            return Deserialize(ms);
        }

        /// <summary>
        /// Validates the magic, i.e. the first x bytes taken by <c>MAGIC</c>.
        /// </summary>
        /// <param name="br">BinaryReader to read</param>
        /// <returns>True if successful. Otherwise, exceptions are thrown.</returns>
        /// <exception cref="InvalidFileFormatException">Thrown when the magic is invalid</exception>
        private static bool ValidateMagic(BinaryReader br)
        {
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
                if (e is InvalidFileFormatException)
                {
                    throw;
                }
                // Catch all exceptions possibly thrown by the read magic line
                else if (e is ArgumentException || e is ArgumentNullException || e is DecoderFallbackException
                    || e is IOException || e is ObjectDisposedException || e is ArgumentOutOfRangeException ||
                    e is EncoderFallbackException)
                {
                    throw new InvalidFileFormatException("The provided file is not supported!");
                }
                else
                {
                    // In case of missed exceptions
                    throw;
                }
            }

            return true;
        }

        /// <summary>
        /// Converts bytes to a solution array. Reverse of <see cref="ConvertPuzzleSolutionToBytes"/>
        /// </summary>
        /// <param name="bytes">Array of bytes to convert</param>
        /// <param name="width">Width of the puzzle to convert</param>
        /// <param name="height">Height of the puzzle to convert</param>
        /// <returns>Array of bools where for each location with an expected filled cell, the corresponding index is set</returns>
        /// <exception cref="OverflowException">Thrown when the puzzle dimension are too large to calculate the number of cells</exception>
        internal static HashSet<CellPosition> ConvertBytesToSolution(byte[] bytes, int width, int height)
        {
            int totalCells = checked(width * height);
            HashSet<CellPosition> solutionSet = [];

            for (int i = 0; i < totalCells; i++)
            {
                int byteIndex = i / 8;
                int bitIndex = i % 8;

                byte b = bytes[byteIndex];

                // Check if bit at corresponding position is set (this num is 0 if they do not match, otherwise, this is 2^x)
                if ((b & (byte)(1 << 7 - bitIndex)) != 0)
                {
                    int x = i % width;
                    int y = i / width;
                    solutionSet.Add(new(x, y));
                }
            }
            return solutionSet;
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
}
