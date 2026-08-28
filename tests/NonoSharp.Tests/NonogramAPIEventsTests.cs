using System;
using System.Collections.Generic;
using System.Text;
using NonoSharp.Events;

namespace NonoSharp.Tests
{
    public class NonogramAPIEventsTests
    {
        [Fact]
        public void TestCellStateChangedEvent()
        {
            NonogramAPI api = new(new Grid(1, 1));
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
            NonogramAPI api = new(g);

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


        [Fact]
        public void TestCorrectionEventCrossToFill()
        {
            // Create simple grid with only (0, 1) filled being correct
            Grid g = new(1, 2);
            g.SetSolution([new(0, 1)]);
            NonogramAPI api = new(g) { EnableAutoCorrection = true };

            bool eventFired = false;
            object? sender = null;
            CorrectionEventArgs? args = null;

            api.CellCorrected += (s, e) =>
            {
                eventFired = true;
                sender = s;
                args = e;
            };

            api.CrossCell(0, 1);

            Assert.True(eventFired);
            Assert.Same(api, sender);
            Assert.NotNull(args);

            Assert.Equal(0, args.Cell.X);
            Assert.Equal(1, args.Cell.Y);

            Assert.Equal(CellType.CROSS, args.Before);
            Assert.Equal(CellType.FILLED, args.After);
        }

        [Fact]
        public void TestCorrectionEventFillToCross()
        {
            // Create simple grid with only (0, 1) filled being correct
            Grid g = new(2, 2);
            g.SetSolution([new(1, 1)]);
            NonogramAPI api = new(g) { EnableAutoCorrection = true };

            bool eventFired = false;
            object? sender = null;
            CorrectionEventArgs? args = null;

            api.CellCorrected += (s, e) =>
            {
                eventFired = true;
                sender = s;
                args = e;
            };

            api.FillCell(1, 0);

            Assert.True(eventFired);
            Assert.Same(api, sender);
            Assert.NotNull(args);

            Assert.Equal(1, args.Cell.X);
            Assert.Equal(0, args.Cell.Y);

            Assert.Equal(CellType.FILLED, args.Before);
            Assert.Equal(CellType.CROSS, args.After);
        }

        [Fact]
        public void TestCorrectionEventNonEmptyToEmpty()
        {
            // Emptying a cell should not trigger correction

            // Create simple grid with only (0, 1) filled being correct
            Grid g = new(1, 2);
            g.SetSolution([new(0, 1)]);
            NonogramAPI api = new(g) { EnableAutoCorrection = true };

            bool eventFired = false;

            api.CellCorrected += (s, e) =>
            {
                eventFired = true;
            };

            api.EmptyCell(0, 1);
            Assert.False(eventFired);
        }

        [Fact]
        public void TestCorrectionEventDisabled()
        {
            // Create simple grid with only (0, 1) filled being correct
            Grid g = new(1, 2);
            g.SetSolution([new(0, 1)]);
            NonogramAPI api = new(g) { EnableAutoCorrection = false };

            bool eventFired = false;

            api.CellCorrected += (s, e) =>
            {
                eventFired = true;
            };

            api.CrossCell(0, 1);
            Assert.False(eventFired);
        }
    }
}
