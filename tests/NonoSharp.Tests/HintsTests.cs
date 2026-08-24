using System;
using System.Collections.Generic;
using System.Text;

namespace Picross.Game.Tests
{
    public class HintsTests
    {
        [Fact]
        public void TestCompletenessAfterFirstHint()
        {
            // This tests an edge case where a second hint is marked as complete
            // even if the user did not separate the groups of cells with a cross, only adding
            // a cross after the second group.
            // While in-line with first hint does not need a cross to be marked as complete, it looks strange
            Hints hints = new(false, 0, [new(1), new(1), new(1)]);

            // Setup a mock line
            CellType[] line = new CellType[6];

            // Check that no hints are completed
            hints.DoCompletion(line);
            Assert.DoesNotContain(hints, h => h.Completed);

            // Fill in the cells to test the described test case. Here, we do that by settings the 0th and 2nd cell as filled
            line[0] = CellType.FILLED;
            line[2] = CellType.FILLED;

            // Now, only 0 should be completed, as we do not require a cross for the first group
            hints.DoCompletion(line);
            Assert.Single(hints, h => h.Completed);
            Assert.True(hints[0].Completed);

            // Finally, check if adding a cross after the second group does not change the 2nd groups completed state
            line[3] = CellType.CROSS;
            hints.DoCompletion(line);

            Assert.Single(hints, h => h.Completed);
            Assert.True(hints[0].Completed);
        }
    }
}
