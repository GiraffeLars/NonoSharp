namespace Core
{
    public class Hints
    {
        private List<Hint> hints;
        private bool vertical;
        private int position;

        // Filled squares that have not yet been considered in a (completed) hint
        private int remainingUncheckedSquares;

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

        public Hint GetHint(int position) { return hints[position]; }

        /// <summary>
        /// Resets the all hints by setting their completed status to false.
        /// Also resets how many squares we have not handled yet.
        /// </summary>
        internal void Reset()
        {
            remainingUncheckedSquares = 0;
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

            
            while (node != null)
            {
                if (node.Value == SquareType.FILLED)
                {
                    remainingUncheckedSquares++;
                }
                node = node.Next;
            }

            DoCompletionForward(line);
            DoCompletionBackward(line);
        }

        private void DoCompletionForward(LinkedList<SquareType> line)
        {
            LinkedListNode<SquareType>? node = line.First;
            int hintIndex = 0;
            int expectedValue = hints[hintIndex].Number;
            bool startedHandling = false;

            while (node != null && hintIndex < hints.Count)
            {
                if (node.Value == SquareType.BLANK)
                {
                    // Do not give out information that the player might not know
                    break;
                }

                if (node.Value == SquareType.CROSS && !startedHandling)
                {
                    node = node.Next;
                    continue;
                }
                else if (node.Value == SquareType.CROSS)
                {
                    // Check if this hint is filled in correctly
                    if (expectedValue == 0)
                    {
                        hints[hintIndex]._completed = true;
                    }

                    hintIndex++;

                    if (hintIndex >= hints.Count)
                    {
                        break;
                    }
                    expectedValue = hints[hintIndex].Number;
                    startedHandling = false;
                    node = node.Next;
                    continue;
                }

                // Else, this square is filled in. We adjust the remaining expected square count
                // and move to the next node
                expectedValue--;
                startedHandling = true;
                remainingUncheckedSquares--;
                node = node.Next;
            }

            if (node == null && hintIndex == hints.Count - 1 && expectedValue == 0)
            {
                hints[hintIndex]._completed = true;
            }

            DoSanityCheck(node, hintIndex, true);
        }

        private void DoCompletionBackward(LinkedList<SquareType> line)
        {
            LinkedListNode<SquareType>? node = line.Last;
            int hintIndex = hints.Count - 1;
            int expectedValue = hints[hintIndex].Number;
            bool startedHandling = false;

            while (node != null && hintIndex >= 0 && !hints[hintIndex].Completed && remainingUncheckedSquares > 0)
            {
                if (node.Value == SquareType.BLANK)
                {
                    // Do not give out information that the player might not know
                    break;
                }

                if (node.Value == SquareType.CROSS && !startedHandling)
                {
                    node = node.Previous;
                    continue;
                }
                else if (node.Value == SquareType.CROSS)
                {
                    // Check if this hint is filled in correctly
                    if (expectedValue == 0)
                    {
                        hints[hintIndex]._completed = true;
                    }

                    hintIndex--;

                    if (hintIndex < 0)
                    {
                        break;
                    }
                    expectedValue = hints[hintIndex].Number;
                    startedHandling = false;
                    node = node.Previous;
                    remainingUncheckedSquares--;
                    continue;
                }


                // Else, this square is filled in. We adjust the remaining expected square count
                // and move to the next node
                expectedValue--;
                startedHandling = true;
                node = node.Previous;
            }

            DoSanityCheck(node, hintIndex, false);
        }

        private void DoSanityCheck(LinkedListNode<SquareType>? node, int idx, bool forwards)
        {
            // Final sanity check if all next line nodes are crosses
            if ((forwards && idx == hints.Count) || (!forwards && idx == -1))
            {
                while (node != null)
                {
                    if (node.Value != SquareType.CROSS)
                    {
                        if (forwards)
                        {
                            hints[idx - 1]._completed = false;
                        } else
                        {
                            hints[0]._completed = false;
                        }
                        return;
                    }

                    if (forwards)
                    {
                        node = node.Next;
                    } else
                    {
                        node = node.Previous;
                    }
                }
            }
        }
    }
}
