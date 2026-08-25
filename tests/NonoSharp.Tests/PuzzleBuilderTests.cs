using System;
using System.Collections.Generic;
using System.Text;

namespace NonoSharp.Tests
{
    public class PuzzleBuilderTests
    {
        [Fact]
        public void TestInitInvalidDimensionsException()
        {
            PuzzleBuilder builder;
            Assert.Throws<ArgumentOutOfRangeException>(() => builder = new(-1, 5));
            Assert.Throws<ArgumentOutOfRangeException>(() => builder = new(5, -1));
        }

        [Fact]
        public void TestInitValidDimensions()
        {
            PuzzleBuilder builder = new(1, 2);
            Assert.Equal(1, builder.Width);
            Assert.Equal(2, builder.Height);
        }

        [Fact]
        public void TestInitiallyAllEmpty()
        {
            PuzzleBuilder builder = new(5, 5);
            for (int x = 0; x < 5; x++)
            {
                for (int y = 0; y < 5; y++)
                {
                    Assert.False(builder.IsCellFilled(x, y));
                    Assert.True(builder.IsCellEmpty(x, y));
                }
            }
        }

        [Fact]
        public void TestInvalidCoordinatesException()
        {
            PuzzleBuilder builder = new(5, 5);
            Assert.Throws<ArgumentOutOfRangeException>(() => builder.IsCellEmpty(-1, 4));
            Assert.Throws<ArgumentOutOfRangeException>(() => builder.IsCellEmpty(0, 5));

            Assert.Throws<ArgumentOutOfRangeException>(() => builder.IsCellFilled(-1, 4));
            Assert.Throws<ArgumentOutOfRangeException>(() => builder.IsCellFilled(0, 5));
        }
    }
}
