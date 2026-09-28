using System;
using System.Collections.Generic;
using System.Text;
using NonoSharp.Events;

namespace NonoSharp.Tests
{
    public class NonogramEventsTests
    {
        private NonogramOptions optsAutoCorrect = new() { EnableAutoCorrect = true };

        [Fact]
        public void TestCellStateChangedEvent()
        {
            Nonogram nonogram = new(new Puzzle(1, 1, null));
            bool eventFired = false;
            object? sender = null;
            CellStateEventArgs? args = null;

            nonogram.CellStateChanged += (s, e) =>
            {
                eventFired = true;
                sender = s;
                args = e;
            };

            nonogram.FillCell(0, 0);
            Assert.True(eventFired);

            // Check whether the sender was the nonogram
            Assert.Same(nonogram, sender);

            // Check if only the 1x1 was changed and check if the args reflect this
            Assert.NotNull(args);
            Assert.Single(args.Cells);
            Assert.Equal(0, args.Cells[0].X);
            Assert.Equal(0, args.Cells[0].Y);
        }

        [Fact]
        public void TestPuzzleSolvedEvent()
        {
            // Create simple puzzle with only (0, 0) filled being correct
            Puzzle p = new(1, 1, [new(0, 0)]);
            Nonogram nonogram = new(p);

            bool eventFired = false;
            object? sender = null;

            nonogram.PuzzleSolved += (s, e) =>
            {
                eventFired = true;
                sender = s;
            };

            nonogram.FillCell(0, 0);

            Assert.True(eventFired);
            Assert.Same(nonogram, sender);
        }

        [Fact]
        public void TestPuzzleSolvedEventFromClues()
        {
            Clues[] a = [Clues.FromString("1")];
            Clues[] b = [Clues.FromString("1")];

            Nonogram nonogram = new(1, 1, a, b);
            bool eventFired = false;
            nonogram.PuzzleSolved += (s, e) => { eventFired = true; };
            nonogram.FillCell(0, 0);
            Assert.True(eventFired);
        }

        [Fact]
        public void TestCorrectedEventFromClues()
        {
            Clues[] a = [Clues.FromString("1")];
            Clues[] b = [Clues.FromString("1")];

            Nonogram nonogram = new(1, 1, a, b, optsAutoCorrect);
            bool eventFired = false;
            nonogram.CellCorrected += (s, e) => { eventFired = true; };
            nonogram.CrossCell(0, 0);
            Assert.True(eventFired);
        }

        [Fact]
        public void TestCorrectionEventCrossToFill()
        {
            // Create simple grid with only (0, 1) filled being correct
            Puzzle p = new(1, 2, [new(0, 1)]);
            
            Nonogram nonogram = new(p) { Options = optsAutoCorrect };

            bool eventFired = false;
            object? sender = null;
            CorrectionEventArgs? args = null;

            nonogram.CellCorrected += (s, e) =>
            {
                eventFired = true;
                sender = s;
                args = e;
            };

            nonogram.CrossCell(0, 1);

            Assert.True(eventFired);
            Assert.Same(nonogram, sender);
            Assert.NotNull(args);

            Assert.Equal(0, args.Cell.X);
            Assert.Equal(1, args.Cell.Y);

            Assert.Equal(CellType.Cross, args.Before);
            Assert.Equal(CellType.Filled, args.After);
        }

        [Fact]
        public void TestCorrectionEventFillToCross()
        {
            // Create simple grid with only (1, 1) filled being correct
            Puzzle p = new(2, 2, [new(1, 1)]);

            Nonogram nonogram = new(p) { Options = optsAutoCorrect };

            bool eventFired = false;
            object? sender = null;
            CorrectionEventArgs? args = null;

            nonogram.CellCorrected += (s, e) =>
            {
                eventFired = true;
                sender = s;
                args = e;
            };

            nonogram.FillCell(1, 0);

            Assert.True(eventFired);
            Assert.Same(nonogram, sender);
            Assert.NotNull(args);

            Assert.Equal(1, args.Cell.X);
            Assert.Equal(0, args.Cell.Y);

            Assert.Equal(CellType.Filled, args.Before);
            Assert.Equal(CellType.Cross, args.After);
        }

        [Fact]
        public void TestCorrectionEventNonEmptyToEmpty()
        {
            // Emptying a cell should not trigger correction

            // Create simple grid with only (0, 1) filled being correct
            Puzzle p = new(1, 2, [new(0, 1)]);
            Nonogram nonogram = new(p) { Options = optsAutoCorrect };

            bool eventFired = false;

            nonogram.CellCorrected += (s, e) =>
            {
                eventFired = true;
            };

            nonogram.EmptyCell(0, 1);
            Assert.False(eventFired);
        }

        [Fact]
        public void TestCorrectionEventDisabled()
        {
            // Create simple grid with only (0, 1) filled being correct
            Puzzle p = new(1, 2, [new(0,1)]);
            Nonogram nonogram = new(p) { Options = new() { EnableAutoCross = false } };

            bool eventFired = false;

            nonogram.CellCorrected += (s, e) =>
            {
                eventFired = true;
            };

            nonogram.CrossCell(0, 1);
            Assert.False(eventFired);
        }
    }
}
