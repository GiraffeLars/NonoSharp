using System;
using System.Collections.Generic;
using System.Text;
using Picross.Game.Events;

namespace Picross.Game.Tests
{
    public class GameAPIEventsTests
    {
        [Fact]
        public async Task TestCellStateChangedEvent()
        {
            GameAPI api = await GameAPI.CreateRandomPuzzle(1, 1);
            bool eventFired = false;
            object? sender = null;
            CellStateEventArgs? args = null;

            api.CellStateChanged += (s, e) =>
            {
                eventFired = true;
                sender = s;
                args = e;
            };

            api.FillCell(0, 0);
            Assert.True(eventFired);

            // Check whether the sender was the api
            Assert.Same(api, sender);

            // Check if only the 1x1 was changed and check if the args reflect this
            Assert.NotNull(args);
            Assert.Single(args.Cells);
            Assert.Equal(0, args.Cells[0].X);
            Assert.Equal(0, args.Cells[0].Y);
        }

        [Fact]
        public void TestPuzzleSolvedEvent()
        {
            // Create simple grid with only (0, 0) filled being correct
            Grid g = new(1, 1);
            g.SetSolution([new(0, 0)]);
            GameAPI api = new(g);

            bool eventFired = false;
            object? sender = null;

            api.PuzzleSolved += (s, e) =>
            {
                eventFired = true;
                sender = s;
            };

            api.FillCell(0, 0);

            Assert.True(eventFired);
            Assert.Same(api, sender);
        }
    }
}
