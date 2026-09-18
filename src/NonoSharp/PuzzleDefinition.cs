using NonoSharp.Exceptions;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Text;

namespace NonoSharp
{
    internal class PuzzleDefinition
    {
        /// <summary>
        /// Title of the puzzle
        /// </summary>
        public string? Title { get; internal set; }
        public const int MAX_TITLE_LENGTH = 100;
        public int Width { get; private set; }
        public int Height { get; private set; }

        public FrozenSet<CellPosition>? Solution { get; protected set; }

        /// <summary>
        /// Creates a puzzle definition
        /// </summary>
        /// <param name="width">Width of the puzzle</param>
        /// <param name="height">Height of the puzzle</param>
        /// <param name="solution">The solution of the puzzle</param>
        /// <param name="title">Optional title of the puzzle</param>
        internal PuzzleDefinition(int width, int height, IEnumerable<CellPosition>? solution, string? title = null)
        {
            this.Width = width;
            this.Height = height;
            this.Solution = solution?.ToFrozenSet();
            this.Title = title;
        }

        /// <summary>
        /// Determines if (<paramref name="x"/>, <paramref name="y"/>) is expected to be filled in the solution.
        /// </summary>
        /// <param name="x">x-coordinate to check.</param>
        /// <param name="y">y-coordinate to check.</param>
        /// <returns><c>true</c> if cell is expected to be filled in the solution, <c>false</c> otherwise.</returns>
        internal bool GetSolutionAt(int x, int y)
        {
            if (Solution == null) throw new InvalidOperationException("There is no solution for this puzzle!");
            return Solution.Contains(new(x, y));
        }

        /// <summary>
        /// Sets the dimensions of this puzzle definition. This is done by adding new empty cells to the right and bottom.
        /// Cannot shrink the puzzle.
        /// </summary>
        /// <param name="width">New width for the puzzle</param>
        /// <param name="height">New height for the puzzle</param>
        /// <exception cref="OverflowException">When <paramref name="width"/> * <paramref name="height"/> causes overflow</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="width"/> or <paramref name="height"/> are
        /// less than their respective old value</exception>
        /// <exception cref="ArgumentException">When <paramref name="width"/> is less than the old width or <paramref name="height"/>
        /// is less than the old height</exception>
        internal void SetDimensions(int width, int height)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(width, Width, nameof(width));
            ArgumentOutOfRangeException.ThrowIfLessThan(height, Height, nameof(height));
            Width = width;
            Height = height;
        }

        /// <summary>
        /// Saves this puzzle at <paramref name="path"/>. Specifically, the expected solution and dimension are stored.
        /// See also <seealso cref="File.WriteAllBytes(string, byte[])"/>.
        /// </summary>
        /// <param name="path">The path to save the puzzle to</param>
        /// <exception cref="PuzzleSerializationFailedException">Thrown when serialization fails. For example, when the given title is too long, or an I/O exception occurs.
        /// Usually, there is an inner exception giving more details.</exception>
        /// <exception cref="PuzzleSavingFailedException">Thrown when saving files fails, e.g. because of an I/O Exception. See the inner exception for more details</exception>
        public void SavePuzzle(string path)
        {
            byte[] serialized = PuzzleSerializer.Serialize(this);

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
        /// <exception cref="PuzzleSerializationFailedException">Thrown when serialization fails. For example, when the given title is too long, or an I/O exception occurs.
        /// Usually, there is an inner exception giving more details.</exception>
        /// <exception cref="PuzzleSavingFailedException">Thrown when saving files fails, e.g. because of an I/O Exception. See the inner exception for more details</exception>
        public async Task SavePuzzleAsync(string path)
        {
            byte[] serialized = PuzzleSerializer.Serialize(this);
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
        /// Loads the puzzle contained in <paramref name="stream"/>.
        /// </summary>
        /// <remarks>
        /// <paramref name="stream"/> is left open. Do not forget to close it.
        /// </remarks>
        /// <param name="stream">Stream to read the puzzle from. 
        /// To avoid false positives on InvalidFileFormatException exceptions, the stream must consist of ONLY one valid puzzle, such as one provided by <see cref="SavePuzzle(string)"/>.</param>
        /// <returns><c>PuzzleDefinition</c> of the requested puzzle</returns>
        /// <exception cref="InvalidFileFormatException">Thrown when the given file format is not supported</exception>
        /// <exception cref="NotSupportedException">Thrown when the version of the save system is not supported</exception>
        public static PuzzleDefinition LoadPuzzle(Stream stream)
        {
            return PuzzleSerializer.Deserialize(stream);
        }

        /// <summary>
        /// Loads the puzzle located at <paramref name="path"/>
        /// </summary>
        /// <param name="path">Path of the puzzle to load</param>
        /// <returns><c>PuzzleDefinition</c> of the requested puzzle</returns>
        /// <exception cref="InvalidFileFormatException">Thrown when the given file format is not supported</exception>
        /// <exception cref="NotSupportedException">Thrown when the version of the save system is not supported</exception>
        /// <exception cref="PuzzleLoadingFailedException">Thrown when loading the file fails, e.g. because of an I/O Exception. See the inner exception for more details</exception>
        public static PuzzleDefinition LoadPuzzle(string path)
        {
            FileStream fs;
            try
            {
                fs = File.OpenRead(path);
            }
            catch (Exception e)
            {
                throw new PuzzleLoadingFailedException($"Failed to open file {path}!", e);
            }

            try
            {
                PuzzleDefinition puzzle = LoadPuzzle(fs);
                return puzzle;
            }
            finally
            {
                fs.Close();
            }
        }

        /// <summary>
        /// Loads the puzzle located at <paramref name="path"/> asynchronously. 
        /// </summary>
        /// <remarks>
        /// Contents of <paramref name="path"/> are expected to be relatively small. 
        /// Larger files might cause noticeable blocking.
        /// Asynchronous puzzle reading of a <see cref="Stream"/> is not supported. Use the synchronous <see cref="LoadPuzzle(Stream)"/> instead.
        /// </remarks>
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

            // Do not use an asynchronous deserializer as contents are expected to be small.
            return PuzzleSerializer.Deserialize(serializedPuzzle);
        }
    }
}
