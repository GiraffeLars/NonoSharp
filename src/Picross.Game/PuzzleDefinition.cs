using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Picross.Game
{
    internal class PuzzleDefinition
    {
        internal const string magic = "NONO"; // To check file format, in ASCII
        public static int Version { get; } = 0;

        /// <summary>
        /// Title of the puzzle
        /// </summary>
        public string Title { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        /// <summary>
        /// The solution in bool, going from left-to-right, top-to-bottom.
        /// </summary>
        public bool[] Solution { get; private set; }
        // Bools take 1 bit, ints 32. Depending on the density and dimensions of the puzzle, one can be more optimal for storage space
        // but considering that usually >50% of cells are filled, storing the expected state of each cell is very defendable

        /// <summary>
        /// Total amount of bytes used by solution, rounded up
        /// </summary>
        private int SolutionLengthBytes => (int)Math.Ceiling((double)Solution.Length / 8);

        internal PuzzleDefinition(int width, int height, bool[] solution, string? title = null)
        {
            this.Width = width;
            this.Height = height;
            this.Solution = solution;
            this.Title = title ?? "";
        }

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

        public byte[] Serialize()
        {
            MemoryStream ms = new();

            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write(Encoding.ASCII.GetBytes(magic));
            bw.Write(Version);

            bw.Write(Title);
            bw.Write(Width);
            bw.Write(Height);

            bw.Write(ConvertSolutionToBytes());

            bw.Close();
            ms.Close();
            return ms.ToArray();
        }

        internal byte[] ConvertSolutionToBytes()
        {
            byte[] bytes = new byte[SolutionLengthBytes];

            for (int i = 0; i < Width * Height; i++)
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
        public static PuzzleDefinition Deserialize(byte[] serializedPuzzle)
        {
            MemoryStream ms = new(serializedPuzzle);
            BinaryReader br = new BinaryReader(ms);

            string readMagic = Encoding.ASCII.GetString(br.ReadBytes(Encoding.ASCII.GetByteCount(magic)));

            if (readMagic != magic)
            {
                throw new InvalidFileFormatException($"The provided file is not supported!");
            }

            int readVersion = br.ReadInt32();
            if (readVersion != Version)
            {
                throw new NotSupportedException(
                    $"The given puzzle's version ({readVersion}) does not match the current version ({Version})!");
            }

            string readTitle = br.ReadString();
            int readWidth = br.ReadInt32();
            int readHeight = br.ReadInt32();

            // Read the bytes of the puzzle. These are the total number of cells converted to bytes, round up
            byte[] remaining = br.ReadBytes((int)Math.Ceiling((double)(readWidth * readHeight) / 8)); 
            bool[] readSolution = ConvertBytesToSolution(remaining, readWidth, readHeight);

            return new(readWidth, readHeight, readSolution, readTitle);
        }

        internal static bool[] ConvertBytesToSolution(byte[] bytes, int width, int height)
        {
            bool[] sol = new bool[width * height];

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
        public void SavePuzzle(string path)
        {
            byte[] serialized = Serialize();
            File.WriteAllBytes(path, serialized);
        }


        /// <summary>
        /// Saves this puzzle asynchronously at <paramref name="path"/>. Specifically, the expected solution and dimension are stored.
        /// See also <seealso cref="File.WriteAllBytesAsync(string, byte[], CancellationToken)"/>.
        /// </summary>
        /// <param name="path">The path to save the puzzle to</param>
        public async Task SavePuzzleAsync(string path)
        {
            byte[] serialized = Serialize();
            // No SerializeAsync method as puzzles are usually small in terms of bytes.
            // This is also the reason that there are no async binarywriter/readers available in .NET

            await File.WriteAllBytesAsync(path, serialized);
        }

        /// <summary>
        /// Loads the puzzle located at <paramref name="path"/>
        /// </summary>
        /// <param name="path">Path of the puzzle to load</param>
        /// <returns>A PuzzleDefinition of the requested puzzle, if available</returns>
        public static PuzzleDefinition LoadPuzzle(string path)
        {
            byte[] serializedPuzzle = File.ReadAllBytes(path);
            return Deserialize(serializedPuzzle);
        }


        /// <summary>
        /// Loads the puzzle located at <paramref name="path"/> asynchronously
        /// </summary>
        /// <param name="path">Path of the puzzle to load</param>
        /// <returns>A PuzzleDefinition of the requested puzzle, if available</returns>
        public static async Task<PuzzleDefinition> LoadPuzzleAsync(string path)
        {
            byte[] serializedPuzzle = await File.ReadAllBytesAsync(path);
            return Deserialize(serializedPuzzle);
        }
    }

    [Serializable]
    public class InvalidFileFormatException : Exception
    {
        public InvalidFileFormatException()
        { }

        public InvalidFileFormatException(string message)
            : base(message)
        { }

        public InvalidFileFormatException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }

}
