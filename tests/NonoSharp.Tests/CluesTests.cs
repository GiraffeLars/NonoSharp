using System;
using System.Collections.Generic;
using System.Text;

namespace NonoSharp.Tests
{
    public class CluesTests
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

        [Fact]
        public void TestCluesCompletedWithMultipleGroupsBetweenCrosses()
        {
            Clues clues = Clues.FromString("2 2");
            CellType[] line = new CellType[10];

            Assert.False(clues[0].Completed);
            Assert.False(clues[1].Completed);


            line[2] = CellType.FILLED;
            line[3] = CellType.FILLED;
            line[6] = CellType.FILLED;
            line[7] = CellType.FILLED;
            clues.DoCompletion(line);
            Assert.False(clues[0].Completed);
            Assert.False(clues[1].Completed);

            line[0] = CellType.CROSS;
            line[1] = CellType.CROSS;
            clues.DoCompletion(line);
            Assert.True(clues[0].Completed);
            Assert.False(clues[1].Completed);

            line[4] = CellType.CROSS;
            line[5] = CellType.CROSS;
            clues.DoCompletion(line);
            Assert.True(clues[0].Completed);

            // Our specifications for when a user knows this should be filled requires all clues after the first/last to be between crosses or other cells
            Assert.False(clues[1].Completed);

            line[8] = CellType.CROSS;
            clues.DoCompletion(line);
            Assert.True(clues[1].Completed);

            for (int i = 9; i < line.Length; i++)
            {
                line[i] = CellType.CROSS;
            }

            clues.DoCompletion(line);
            Assert.True(clues[0].Completed);
            Assert.True(clues[1].Completed);
            Assert.True(clues.FullyCompleted);

            // Check if clue completeness is still true if we know that it was handled from the back
            line[0] = CellType.BLANK;

            clues.DoCompletion(line);
            Assert.True(clues[0].Completed);
            Assert.True(clues[1].Completed);
            Assert.True(clues.FullyCompleted);
        }

        [Fact]
        public void TestClue_SingleFilled_TwoInSolution()
        {
            // Only possible solution, if line length is 3
            // [O][X][O]
            Clues clues = Clues.FromString("1 1");
            CellType[] line = new CellType[3];

            // [O][X][X]
            line[0] = CellType.FILLED;
            line[1] = CellType.CROSS;
            line[2] = CellType.CROSS;
            clues.DoCompletion(line);

            // Since the first cell is filled in, we expect the first clue to be completed as it makes more sense intuitively
            Assert.True(clues[0].Completed);
            Assert.False(clues[1].Completed);

            // Now check single filled in cell at the end
            // [X][X][O]
            line[0] = CellType.CROSS;
            line[2] = CellType.FILLED;
            clues.DoCompletion(line);

            // It does not really matter which clue is completed, both make sense in a way, as long as one is completed and the other is not
            bool completed0 = clues[0].Completed;
            bool completed1 = clues[1].Completed;

            // Since these are bools, c0 != c1 implies that one is true and the other is false
            // This is enough for what we want to test as above
            Assert.NotEqual(completed0, completed1);
        }
    }
}
