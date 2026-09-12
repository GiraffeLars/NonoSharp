using System;
using System.Collections.Generic;
using System.Text;

namespace NonoSharp.Tests
{
    public class CluesTest
    {
        [Fact]
        public void TestFromString()
        {
            int[] nums = { 1, 2, 3 };
            string str = string.Join(' ', nums);
            Clues clues = Clues.FromString(str);

            Assert.Equal(nums.Length, clues.Count);
            for (int i = 0; i < nums.Length; i++)
            {
                Assert.Equal(nums[i], clues[i].Number);
            }
        }

        [Fact]
        public void TestFromStringTrailingSpace()
        {
            int[] nums = { 1, 2, 3 };
            string str = string.Join(' ', nums) + " ";
            Clues clues = Clues.FromString(str);

            Assert.Equal(nums.Length, clues.Count);
        }

        [Fact]
        public void TestFromStringMultipleCluesAndZero()
        {
            string str = "1 4 6 0 2 5";
            Assert.Throws<ArgumentException>(() => Clues.FromString(str));
        }

        [Fact]
        public void TestFromStringNegativeHints()
        {
            string str = "-4";
            Assert.Throws<ArgumentException>( () => Clues.FromString(str));
        }

        [Fact]
        public void TestFromStringZero()
        {
            string str = "0";
            Clues clues = Clues.FromString(str);
            Assert.Single(clues);
            Assert.Equal(0, clues[0].Number);
        }

        [Fact]
        public void TestFromList()
        {
            List<Clue> clues = [new(1), new(2)];
            Clues instance = [.. clues];
            Assert.Equal(2, instance.Count);
            Assert.Equal(1, instance[0].Number);
            Assert.Equal(2, instance[1].Number);
        }
    }
}
