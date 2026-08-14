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
        private readonly PuzzleDefinition titledPuzzle = new(sol.Length, 1, sol, title: "test");

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
            int byteNum = 0;
            
            // 4 for magic, 3 * 32 bits for version, width, height (=4*4 bytes), 1 byte for empty title and the bytes for the solution
            Assert.Equal(4 + 3 * 4 + 1 + 2, serialized.Length);

            // Magic
            Assert.Equal(Encoding.ASCII.GetBytes(PuzzleDefinition.MAGIC), serialized[byteNum..(byteNum + 4)]);
            byteNum += 4;

            // Version
            Assert.Equal(PuzzleDefinition.Version, BitConverter.ToInt32(serialized.AsSpan()[byteNum.. (byteNum + 4)]));
            byteNum += 4;

            // Title, should be 1 byte of 0's as the string is empty
            Assert.Equal(0, serialized[byteNum]);
            byteNum += 1;

            // Width & height
            Assert.Equal(sol.Length, BitConverter.ToInt32(serialized.AsSpan()[byteNum..(byteNum + 4)]));
            byteNum += 4;

            Assert.Equal(1, BitConverter.ToInt32(serialized.AsSpan()[byteNum..(byteNum + 4)]));
            byteNum += 4;

            // Serialized solution, tested before
            Assert.Equal(71, serialized[byteNum]);
            byteNum += 1;

            Assert.Equal(162, serialized[byteNum]);
        }

        [Fact]
        public void TestSerializeTitledPuzzle()
        {
            byte[] serialized = titledPuzzle.Serialize();
            int byteNum = 0;

            // 4 for magic, 3 * 32 bits for version, width, height (=4*4 bytes),
            // 1 byte for title length, 4 for title and the bytes for the solution
            Assert.Equal(4 + 3 * 4 + 1 + 4 + 2, serialized.Length);

            // Magic
            Assert.Equal(Encoding.ASCII.GetBytes(PuzzleDefinition.MAGIC), serialized[byteNum..(byteNum + 4)]);
            byteNum += 4;

            // Version
            Assert.Equal(PuzzleDefinition.Version, BitConverter.ToInt32(serialized.AsSpan()[byteNum..(byteNum + 4)]));
            byteNum += 4;

            // Title length, should be 4 as the title is "test". Each character fits in the first 7 bits 
            Assert.Equal(4, serialized[byteNum]);
            byteNum += 1;

            // Title
            Assert.Equal((byte)'t', serialized[byteNum]);
            Assert.Equal((byte)'e', serialized[++byteNum]);
            Assert.Equal((byte)'s', serialized[++byteNum]);
            Assert.Equal((byte)'t', serialized[++byteNum]);
            byteNum++;

            // Width & height
            Assert.Equal(sol.Length, BitConverter.ToInt32(serialized.AsSpan()[byteNum..(byteNum + 4)]));
            byteNum += 4;

            Assert.Equal(1, BitConverter.ToInt32(serialized.AsSpan()[byteNum..(byteNum + 4)]));
            byteNum += 4;

            // Serialized solution, tested before
            Assert.Equal(71, serialized[byteNum]);
            byteNum += 1;

            Assert.Equal(162, serialized[byteNum]);
        }

        [Fact]
        public void TestConvertBytesToSolution()
        {
            byte[] bytes = [71, 162];

            bool[] determinedSolution = PuzzleDefinition.ConvertBytesToSolution(bytes, sol.Length, 1);

            Assert.Equal(sol.Length, determinedSolution.Length);
            Assert.Equal(sol, determinedSolution);
        }

        [Fact]
        public void TestDeserialize()
        {
            PuzzleDefinition receivedDefinition = PuzzleDefinition.Deserialize(puzzle.Serialize());

            Assert.Equal(puzzle.Title, receivedDefinition.Title);
            Assert.Equal(puzzle.Width, receivedDefinition.Width);
            Assert.Equal(puzzle.Height, receivedDefinition.Height);
            Assert.Equal(sol, receivedDefinition.Solution);
        }

        [Fact]
        public void TestDeserializeTitledPuzzle()
        {
            PuzzleDefinition receivedDefinition = PuzzleDefinition.Deserialize(puzzle.Serialize());

            Assert.Equal(puzzle.Title, receivedDefinition.Title);
            Assert.Equal(puzzle.Width, receivedDefinition.Width);
            Assert.Equal(puzzle.Height, receivedDefinition.Height);
            Assert.Equal(sol, receivedDefinition.Solution);
        }
    }
}
