using System;
using System.Collections.Generic;
using System.Text;

namespace Picross.Game.Tests
{
    public class PuzzleDefinitionTests
    {
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
            bool[] sol = [
                false, true, false, false, false, true, true, true,
                true, false, true, false, false, false, true];

            PuzzleDefinition p = new(sol.Length, 1, sol);
            // 01000111 , 10100010 base 2 = 71 162 base 10

            byte[] bytes = p.ConvertSolutionToBytes();
            Assert.Equal(2, bytes.Length);
            Assert.Equal(71, bytes[0]);
            Assert.Equal(162, bytes[1]);
        }
    }
}
