using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Picross.Game
{
    internal class PuzzleDefinition
    {
        private int width;
        private int height;

        /// <summary>
        /// The solution in bool, going from left-to-right, top-to-bottom.
        /// </summary>
        private bool[] solution;
        // Bools take 1 bit, ints 32. Depending on the density and dimensions of the puzzle, one can be more optimal for storage space
        // but considering that usually >50% of cells are filled, storing the expected state of each cell is very defendable

        private int version = 0;

        /// <summary>
        /// Total amount of bytes used by solution, rounded up
        /// </summary>
        private int SolutionLengthBytes => (int)Math.Ceiling((double)solution.Length / 8);

        internal PuzzleDefinition(int width, int height, bool[] solution)
        {
            this.width = width;
            this.height = height;
            this.solution = solution;
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
            bw.Write(version);
            bw.Write(width);
            bw.Write(height);

            bw.Write(ConvertSolutionToBytes());

            bw.Close();
            ms.Close();
            return ms.ToArray();
        }

        public byte[] ConvertSolutionToBytes()
        {
            byte[] bytes = new byte[SolutionLengthBytes];

            for (int i = 0; i < width * height; i++)
            {
                bytes[i / 8] = (byte)((bytes[i / 8] << 1) | (solution[i] ? 1 : 0));
            }
            return bytes;
        }
    }
}
