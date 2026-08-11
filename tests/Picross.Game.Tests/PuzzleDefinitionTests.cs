using System;
using System.Collections.Generic;
using System.Text;

namespace Picross.Game.Tests
{
    public class PuzzleDefinitionTests
    {
        // Basic solution with corresponding puzzle def
        private static readonly bool[] sol = [
            false, true, false, false, false, true, true, true,
            true, false, true, false, false, false, true];
        private readonly PuzzleDefinition puzzle = new(sol.Length, 1, sol);

        [Fact]
        public void TestConvertSolutionToBytesSingleByte()
        {
            PuzzleDefinition p = new(4, 1, [true, true, false, true]);
            // 11010000 base 2 = 208 base 10

            byte[] bytes = p.ConvertSolutionToBytes();
            Assert.Single(bytes);
            Assert.Equal(208, bytes[0]);
        }

        [Fact]
        public void TestConvertSolutionToBytesMultipleBytes()
        {

            PuzzleDefinition p = new(sol.Length, 1, sol);
            // 01000111 , 10100010 base 2 = 71 162 base 10

            byte[] bytes = p.ConvertSolutionToBytes();
            Assert.Equal(2, bytes.Length);
            Assert.Equal(71, bytes[0]);
            Assert.Equal(162, bytes[1]);
        }

        [Fact]
        public void TestSerialize()
        {
            byte[] serialized = puzzle.Serialize();
            
            // 3 * 32 bits for version, width, height (=3*4 bytes) and the bytes for the solution
            Assert.Equal(3 * 4 + 2, serialized.Length);

            // Version
            Assert.Equal(puzzle.Version, BitConverter.ToInt32(serialized.AsSpan()[0..4]));

            // Width & height
            Assert.Equal(sol.Length, BitConverter.ToInt32(serialized.AsSpan()[4..8]));
            Assert.Equal(1, BitConverter.ToInt32(serialized.AsSpan()[8..12]));

            // Serialized solution, tested before
            Assert.Equal(71, serialized[12]);
            Assert.Equal(162, serialized[13]);
        }
    }
}
