using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Picross.Game
{
    internal class PuzzleDefinition
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        /// <summary>
        /// The solution in bool, going from left-to-right, top-to-bottom.
        /// </summary>
        public bool[] Solution { get; private set; }
        // Bools take 1 bit, ints 32. Depending on the density and dimensions of the puzzle, one can be more optimal for storage space
        // but considering that usually >50% of cells are filled, storing the expected state of each cell is very defendable

        public static int Version { get; } = 0;

        /// <summary>
        /// Total amount of bytes used by solution, rounded up
        /// </summary>
        private int SolutionLengthBytes => (int)Math.Ceiling((double)Solution.Length / 8);

        internal PuzzleDefinition(int width, int height, bool[] solution)
        {
            this.Width = width;
            this.Height = height;
            this.Solution = solution;
        }

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

        public byte[] Serialize()
        {
            // 3 for width, height and version (ints are 32 bits = 4 bytes), and size of boolean solution in bytes, rounded up
            int expectedCapacity = 3 * 4 + SolutionLengthBytes;
            MemoryStream ms = new();

            BinaryWriter bw = new BinaryWriter(ms);
            bw.Write(Version);
            bw.Write(Width);
            bw.Write(Height);

            bw.Write(ConvertSolutionToBytes());

            bw.Close();
            ms.Close();
            return ms.ToArray();
        }

        public byte[] ConvertSolutionToBytes()
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

        public static PuzzleDefinition Deserialize(byte[] serializedPuzzle)
        {
            MemoryStream ms = new(serializedPuzzle);
            BinaryReader br = new BinaryReader(ms);


            int readVersion = br.ReadInt32();
            if (readVersion != Version)
            {
                throw new NotSupportedException($"The given puzzle's version ({readVersion}) does not match the current version ({Version})!");
            }

            int readWidth = br.ReadInt32();
            int readHeight = br.ReadInt32();

            byte[] remaining = br.ReadBytes((int)Math.Ceiling((double)(readWidth * readHeight) / 8));
            bool[] readSolution = ConvertBytesToSolution(remaining, readWidth, readHeight);

            return new(readWidth, readHeight, readSolution);
        }

        public static bool[] ConvertBytesToSolution(byte[] bytes, int width, int height)
        {
            bool[] sol = new bool[width * height];

            for (int i = 0; i < sol.Length; i++)
            {
                byte b = bytes[i / 8];

                if ((b & (byte)(1 << 7 - i % 8)) != 0) // Check if bit at corresponding position is set (this num is 0 if they do not match, otherwise, this is 2^x)
                {
                    sol[i] = true;
                }
            }
            return sol;
        }
    }
}
