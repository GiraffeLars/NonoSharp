namespace Core
{
    public class Hints
    {
        private List<Hint> hints;
        private bool vertical;
        private int position;

        public int Count { get { return hints.Count; } }

        internal Hints(bool vertical, int position)
        {
            hints = new List<Hint>();
            this.vertical = vertical;
            this.position = position;
        }

        /// <summary>
        /// Adds a <c>Hint</c> to this Hints instance. It is appended to the end.
        /// </summary>
        /// <param name="hint">The hint to add</param>
        internal void Add(Hint hint)
        {
            hints.Add(hint);
        }

        // TODO rework this in some way such that GetHint is removed, it adds confusion and overhead
        public Hint GetHint(int position) { return hints[position]; }

        /// <summary>
        /// Resets the all hints by setting their completed status to false.
        /// Also resets how many squares we have not handled yet.
        /// </summary>
        internal void Reset()
        {
            foreach (Hint hint in hints)
            {
                hint._completed = false;
            }
        }

        internal void DoCompletion(Grid grid) {
            if (hints.Count == 0) return;
            Reset();
            LinkedList<SquareType> line = vertical ? grid.GetColumn(position) : grid.GetRow(position);
            LinkedListNode<SquareType>? node = line.First;

            int leftOffAt = DoCompletionForward(line);
            DoCompletionBackward(line, leftOffAt);
        }

        /// <summary>
        /// Checks hint completion by starting from the front
        /// </summary>
        /// <param name="line">The row/column to check</param>
        /// <returns>The final hint which was marked as completed</returns>
        /// <seealso cref="DoCompletionBackward(LinkedList{SquareType}, int)"/>
        private int DoCompletionForward(LinkedList<SquareType> line)
        {
            LinkedListNode<SquareType>? node = line.First;
            bool startedFromFirst = true; // Whether we started from the first square in the current iteration of checking
            int hintIndex = 0;
            int squaresFound = 0; // The total squares we have found that are filled in for this hint

            while (node != null && hintIndex < hints.Count)
            {
                if (node.Value == SquareType.CROSS && startedFromFirst && squaresFound == 0)
                {
                    // The player knows that all cells from the start should not be filled, then we treat this
                    // as if the first square is placed at the first cell
                    node = node.Next;
                    continue;
                }

                // Check if we are allowed to mark this hint as completed
                // A hint is allowed to be completed if it started from the first possible square in the grid
                // Or it has a cross
                if (node.Value == SquareType.CROSS || (node.Value == SquareType.BLANK && startedFromFirst))
                {
                    if (squaresFound == hints[hintIndex].Number)
                    {
                        hints[hintIndex]._completed = true;
                    }
                    else
                    {
                        // This hint is not completed, meaning all other hints are incorrect as well
                        // We stop this loop
                        return hintIndex - 1;
                    }

                    // Reset variables for next iteration
                    startedFromFirst = false;
                    node = node.Next;
                    squaresFound = 0;
                    hintIndex++;
                    continue;
                }

                if (node.Value == SquareType.BLANK && !startedFromFirst)
                {
                    // Now, we do not know whether the player knows that these hints are correct or not,
                    // as we require crosses between squares for squares not starting at the first index
                    // We return as we have no other garauntees on other hints
                    return hintIndex - 1;
                }

                // This square is filled in and should be correct, do the proper variable increments
                node = node.Next;
                squaresFound++;
            }


            // We can reach a situation where the hint is completed at the end of the grid (i.e. all hints are correct)
            // Then, we should do a final check whether this hint is completed
            if (hintIndex <  hints.Count && hints[hintIndex].Number == squaresFound)
            {
                hints[hintIndex]._completed = true;
            }
            return hintIndex;
        }

        /// <summary>
        /// Checks hint completion by starting from the back. Only checks upto (not including) <paramref name="forwardsFinalCheck"/>.
        /// </summary>
        /// <param name="line">The row/column to check</param>
        /// <param name="forwardsFinalCheck">The last hint index which <c>DoCompletionForward</c> left off at</param>
        /// <seealso cref="DoCompletionBackward(LinkedList{SquareType})"/>
        private void DoCompletionBackward(LinkedList<SquareType> line, int forwardsFinalCheck)
        {
            LinkedListNode<SquareType>? node = line.Last;
            bool startedFromFirst = true; // Whether we started from the first square in the current iteration of checking
            int hintIndex = hints.Count - 1;
            int squaresFound = 0; // The total squares we have found that are filled in for this hint   

            while (node != null && hintIndex >= 0 && hintIndex > forwardsFinalCheck)
            {
                if (node.Value == SquareType.CROSS && startedFromFirst && squaresFound == 0)
                {
                    // The player knows that all cells from the start should not be filled, then we treat this
                    // as if the first square is placed at the first cell
                    node = node.Previous;
                    continue;
                }

                // Check if we are allowed to mark this hint as completed
                // A hint is allowed to be completed if it started from the first possible square in the grid
                // Or it has a cross
                if (node.Value == SquareType.CROSS || (node.Value == SquareType.BLANK && startedFromFirst))
                {
                    if (squaresFound == hints[hintIndex].Number)
                    {
                        hints[hintIndex]._completed = true;
                    }
                    else
                    {
                        // This hint is not completed, meaning all other hints are incorrect as well
                        // We stop this loop
                        return;
                    }

                    // Reset variables for next iteration
                    startedFromFirst = false;
                    node = node.Previous;
                    squaresFound = 0;
                    hintIndex--;
                    continue;
                }

                if (node.Value == SquareType.BLANK && !startedFromFirst)
                {
                    // Now, we do not know whether the player knows that these hints are correct or not,
                    // as we require crosses between squares for squares not starting at the first index
                    // We return as we have no other garauntees on other hints
                    return;
                }

                // This square is filled in and should be correct, do the proper variable increments
                node = node.Previous;
                squaresFound++;
            }

            // We can reach a situation where the hint is completed at the end of the grid (i.e. all hints are correct)
            // Then, we should do a final check whether this hint is completed
            if (hintIndex >= 0 && hintIndex > forwardsFinalCheck && hints[hintIndex].Number == squaresFound)
            {
                hints[hintIndex]._completed = true;
            }
        }
    }
}
