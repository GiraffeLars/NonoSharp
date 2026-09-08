using System;
using System.Collections.Generic;
using System.Text;

namespace NonoSharp.Tests
{
    public class CluesTests
    {
        [Fact]
        public void TestCompletenessAfterFirstClue()
        {
            // This tests an edge case where a second clue is marked as complete
            // even if the user did not separate the groups of cells with a cross, only adding
            // a cross after the second group.
            // While in-line with first clue does not need a cross to be marked as complete, it looks strange
            Clues clues = new([new(1), new(1), new(1)]);

            // Setup a mock line
            CellType[] line = new CellType[6];

            // Check that no clues are completed
            clues.DoCompletion(line);
            Assert.DoesNotContain(clues, h => h.Completed);

            // Fill in the cells to test the described test case. Here, we do that by settings the 0th and 2nd cell as filled
            line[0] = CellType.FILLED;
            line[2] = CellType.FILLED;

            // Now, only 0 should be completed, as we do not require a cross for the first group
            clues.DoCompletion(line);
            Assert.Single(clues, h => h.Completed);
            Assert.True(clues[0].Completed);

            // Finally, check if adding a cross after the second group does not change the 2nd groups completed state
            line[3] = CellType.CROSS;
            clues.DoCompletion(line);

            Assert.Single(clues, h => h.Completed);
            Assert.True(clues[0].Completed);
        }
    }
}
