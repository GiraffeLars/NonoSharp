using System;
using System.Collections.Generic;
using System.Text;

namespace NonoSharp.Tests
{
    public class PuzzleBuilderTests
    {
        PuzzleBuilder builder;
        public PuzzleBuilderTests()
        {
            builder = new(5, 5);
        }

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
            for (int x = 0; x < builder.Width; x++)
            {
                for (int y = 0; y < builder.Height; y++)
                {
                    Assert.False(builder.IsCellFilled(x, y));
                    Assert.True(builder.IsCellEmpty(x, y));
                }
            }
        }

        [Fact]
        public void TestInvalidCoordinatesException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => builder.IsCellEmpty(-1, builder.Height-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => builder.IsCellEmpty(0, builder.Height));

            Assert.Throws<ArgumentOutOfRangeException>(() => builder.IsCellFilled(-1, builder.Height-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => builder.IsCellFilled(0, builder.Height));
        }

        [Fact]
        public void TestFillCell()
        {
            builder.FillCell(0, 0);
            Assert.True(builder.GetCell(0, 0));
        }

        [Fact]
        public void TestEmptyCell()
        {
            // Assume fill cell works as intended, is also tested
            builder.FillCell(0, 0);
            builder.EmptyCell(0, 0);
            Assert.False(builder.GetCell(0, 0));
        }

        [Fact]
        public void TestIsCellFilled()
        {
            builder.FillCell(0, 0);
            Assert.True(builder.IsCellFilled(0, 0));
            builder.EmptyCell(0, 0);
            Assert.False(builder.IsCellFilled(0, 0));
        }

        [Fact]
        public void TestIsCellEmpty()
        {
            builder.FillCell(0, 0);
            Assert.False(builder.IsCellEmpty(0, 0));
            builder.EmptyCell(0, 0);
            Assert.True(builder.IsCellEmpty(0, 0));
        }

    }
}
