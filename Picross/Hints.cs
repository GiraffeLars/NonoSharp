namespace Picross
{
    internal class Hints
    {
        private List<Hint> hints;
        private bool vertical;
        private int position;

        internal Hints(bool vertical, int position)
        {
            hints = new List<Hint>();
            this.vertical = vertical;
            this.position = position;
        }

        internal void Add(Hint hint)
        {
            hints.Add(hint);
        }

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

            DoCompletionForward(line);
            DoCompletionBackward(line);
        }

        private void DoCompletionForward(LinkedList<SquareType> line)
        {
            LinkedListNode<SquareType>? node = line.First;
            int hintIndex = 0;
            int expectedValue = hints[hintIndex].number;
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
                    expectedValue = hints[hintIndex].number;
                    startedHandling = false;
                    node = node.Next;
                    continue;
                }

                // Else, this square is filled in. We adjust the remaining expected square count
                // and move to the next node
                expectedValue++;
                startedHandling = true;
                node = node.Next;
            }

            DoSanityCheck(node, hintIndex, true);
        }

        private void DoCompletionBackward(LinkedList<SquareType> line)
        {
            LinkedListNode<SquareType>? node = line.Last;
            int hintIndex = hints.Count - 1;
            int expectedValue = hints[hintIndex].number;
            bool startedHandling = false;

            while (node != null && hintIndex >= 0 && !hints[hintIndex].completed)
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
                    expectedValue = hints[hintIndex].number;
                    startedHandling = false;
                    node = node.Next;
                    continue;
                }
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
