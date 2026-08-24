using System;
using System.Collections.Generic;
using System.Text;

namespace NonoSharp.Tests
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
            PuzzleDefinition receivedDefinition = PuzzleDefinition.Deserialize(titledPuzzle.Serialize());


            Assert.Equal(titledPuzzle.Title, receivedDefinition.Title);
            Assert.Equal(titledPuzzle.Width, receivedDefinition.Width);
            Assert.Equal(titledPuzzle.Height, receivedDefinition.Height);
            Assert.Equal(sol, receivedDefinition.Solution);
        }

        [Fact]
        public void TestSerializeTitleTooLongException()
        {
            PuzzleDefinition longTitle = new(sol.Length, 1, sol, new('a', 1000));
            Assert.True(longTitle.Title!.Length > PuzzleDefinition.MAX_TITLE_LENGTH);
            Assert.Throws<PuzzleSerializationFailedException>(longTitle.Serialize);
        }

        [Fact]
        public void TestDeserializeInvalidMagicException()
        {
            byte[] serialized = puzzle.Serialize();

            // Change magic, first 4 bytes of every expected file
            serialized[0] = (byte)0;

            void act() => PuzzleDefinition.Deserialize(serialized);
            Assert.Throws<InvalidFileFormatException>(act);
        }

        [Fact]
        public void TestDeserializeInvalidVersionException()
        {
            byte[] serialized = puzzle.Serialize();

            // Version are the first 32 bits (4 bytes) after MAGIC in the current version
            // Since the version is stored in a signed int, we can switch the leading bit to make it negative,
            // invalidating any possible version, as version >= 0
            serialized[Encoding.ASCII.GetByteCount(PuzzleDefinition.MAGIC)] |= (byte)128; // 128 = 0b10000000

            void act() => PuzzleDefinition.Deserialize(serialized);
            Assert.Throws<NotSupportedException>(act);
        }

        [Fact]
        public void TestDeserializeTitleTooLongException()
        {
            byte[] serialized = puzzle.Serialize();

            // Insert a title length larger than the maximum allowed and fill with dummy bytes
            int titleLengthIndex = Encoding.ASCII.GetByteCount(PuzzleDefinition.MAGIC) + 4; // magic (4) + version (4)
            int originalTitleLengthBytes = 1; // empty title was serialized as a single 0 byte
            int newTitleLength = PuzzleDefinition.MAX_TITLE_LENGTH + 1;

            byte[] modified = new byte[serialized.Length + newTitleLength];

            // Copy head up to the title length byte
            Array.Copy(serialized, modified, titleLengthIndex);

            // Write the (too large) title length and the title bytes
            modified[titleLengthIndex] = (byte)newTitleLength;
            for (int i = 0; i < newTitleLength; i++)
            {
                modified[titleLengthIndex + 1 + i] = (byte)'a';
            }

            // Copy the remainder (width, height, solution) after the inserted title
            Array.Copy(serialized, titleLengthIndex + originalTitleLengthBytes,
                modified, titleLengthIndex + 1 + newTitleLength,
                serialized.Length - (titleLengthIndex + originalTitleLengthBytes));

            void act() => PuzzleDefinition.Deserialize(modified);
            Assert.Throws<InvalidFileFormatException>(act);
        }

        [Fact]
        public void TestDeserializeInvalidWidthHeightException()
        {
            byte[] serialized = puzzle.Serialize();

            int widthStartIndex = Encoding.ASCII.GetByteCount(PuzzleDefinition.MAGIC) + 4 + 1; // magic + version (int32) + 1 for empty title

            // Switch bit to make it negative
            serialized[widthStartIndex] ^= (byte)128;
            void act() => PuzzleDefinition.Deserialize(serialized);
            Assert.Throws<InvalidFileFormatException>(act);

            // Unswitch width negativity and check if negative height throws error
            serialized[widthStartIndex] ^= (byte)128;
            serialized[widthStartIndex + 4] ^= (byte)128;
            Assert.Throws<InvalidFileFormatException>(act);
        }

        [Fact]
        public void TestDeserializeBytesTooShortException()
        {
            byte[] serialized = puzzle.Serialize();

            // Remove a byte from the serialized puzzle
            byte[] truncated = new byte[serialized.Length - 1];
            Array.Copy(serialized, truncated, truncated.Length);

            void act() => PuzzleDefinition.Deserialize(truncated);
            Assert.Throws<InvalidFileFormatException>(act);
        }

        [Fact]
        public void TestDeserializeBytesTooLargeException()
        {
            byte[] serialized = puzzle.Serialize();

            // Add a byte to the serialized puzzle
            byte[] truncated = new byte[serialized.Length + 1];
            Array.Copy(serialized, truncated, serialized.Length);

            void act() => PuzzleDefinition.Deserialize(truncated);
            Assert.Throws<InvalidFileFormatException>(act);
        }
    }
}
