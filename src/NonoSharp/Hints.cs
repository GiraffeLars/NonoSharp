using System.Collections;

namespace NonoSharp
{
    /// <summary>
    /// The hints of a line. Each Hint in Hints can be accessed by using the indexer.
    /// E.g.: Hint hintAtI = (Hints) hints[i].
    /// </summary>
    public class Hints : ICloneable, IEnumerable<Hint>
    {
        private List<Hint> hints;
        private bool isColumnHints;
        private int position;

        /// <summary>
        /// The total amount of filled cells these hints concern.
        /// </summary>
        public int TotalCellsInHints {  get; private set; }

        /// <summary>
        /// Whether this line of Hints is fully completed (i.e. all hints are completed).
        /// </summary>
        public bool FullyCompleted { get; private set; }

        /// <summary>
        /// The total number of <see cref="Hint"/> instances contained in this Hints instance.
        /// </summary>
        public int Count { get { return hints.Count; } }

        internal Hints(bool isColumnHints, int position)
        {
            hints = new List<Hint>();
            this.isColumnHints = isColumnHints;
            this.position = position;
            TotalCellsInHints = 0;
        }

        internal Hints(bool isColumnHints, int position, List<Hint> hints)
        {
            this.hints = hints;
            this.isColumnHints = isColumnHints;
            this.position = position;
            TotalCellsInHints = 0;
        }

        /// <summary>
        /// Adds a <c>Hint</c> to this Hints instance. It is appended to the end.
        /// </summary>
        /// <param name="hint">The hint to add</param>
        internal void Add(Hint hint)
        {
            hints.Add(hint);
            TotalCellsInHints += hint.Number;
        }

        /// <summary>
        /// Resets the all hints by setting their completed status to false.
        /// Also resets how many cells we have not handled yet.
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
            LinkedList<CellType> line = isColumnHints ? grid.GetColumn(position) : grid.GetRow(position);
            LinkedListNode<CellType>? node = line.First;

            int leftOffAt = DoCompletionForward(line);
            DoCompletionBackward(line, leftOffAt);

            SetFullyCompleted();
        }

        internal void DoCompletion(CellType[] line)
        {
            if (hints.Count == 0) return;
            Reset();
            LinkedList<CellType> linked = new();
            foreach(CellType cell in line)
            {
                linked.AddLast(cell);
            }

            LinkedListNode<CellType>? node = linked.First;

            int leftOffAt = DoCompletionForward(linked);
            DoCompletionBackward(linked, leftOffAt);
        }

        /// <summary>
        /// Checks hint completion by starting from the front
        /// </summary>
        /// <param name="line">The row/column to check</param>
        /// <returns>The final hint which was marked as completed</returns>
        /// <seealso cref="DoCompletionBackward(LinkedList{CellType}, int)"/>
        private int DoCompletionForward(LinkedList<CellType> line)
        {
            LinkedListNode<CellType>? node = line.First;
            bool startedFromFirst = true; // Whether we started from the first cell in the current iteration of checking
            int hintIndex = 0;
            int cellsFound = 0; // The total cells we have found that are filled in for this hint

            while (node != null && hintIndex < hints.Count)
            {
                if (node.Value == CellType.CROSS && startedFromFirst && cellsFound == 0)
                {
                    // The player knows that all cells from the start should not be filled, then we treat this
                    // as if the first cell is placed at the first cell
                    node = node.Next;
                    continue;
                }

                // Check if we are allowed to mark this hint as completed
                // A hint is allowed to be completed if it started from the first possible cell in the grid
                // Or it has a cross
                bool firstGroupCompleteFollowedByBlank = node.Value == CellType.BLANK && startedFromFirst;
                if (node.Value == CellType.CROSS || firstGroupCompleteFollowedByBlank)
                {
                    if (cellsFound == hints[hintIndex].Number)
                    {
                        hints[hintIndex]._completed = true;
                    }
                    else if (node.Value == CellType.CROSS && cellsFound == 0)
                    {
                        // Check if this is a cross while we have not yet started processing a new hint, then this can still be completed,
                        // As we have not invalidated any hint since crosses are like blank spaces
                        node = node.Next;
                        continue;
                    }
                    else
                    {
                        // This hint is not completed, meaning all other hints are incorrect as well
                        // We stop this loop
                        return hintIndex - 1;
                    }

                    if (firstGroupCompleteFollowedByBlank)
                    {
                        // Immediatly stop checking if the first hint group was not seperated by a cross
                        return hintIndex;
                    }

                    // Reset variables for next iteration
                    startedFromFirst = false;
                    node = node.Next;
                    cellsFound = 0;
                    hintIndex++;
                    continue;
                }

                if (node.Value == CellType.BLANK && !startedFromFirst)
                {
                    // Now, we do not know whether the player knows that these hints are correct or not,
                    // as we require crosses between cells for cells not starting at the first index
                    // We return as we have no other garauntees on other hints
                    return hintIndex - 1;
                }

                // This cell is filled in and should be correct, do the proper variable increments
                node = node.Next;
                cellsFound++;
            }


            // We can reach a situation where the hint is completed at the end of the grid (i.e. all hints are correct)
            // Then, we should do a final check whether this hint is completed
            if (hintIndex <  hints.Count && hints[hintIndex].Number == cellsFound)
            {
                hints[hintIndex]._completed = true;
                hintIndex++; // Increase hint index as this hint is completed
            }
            return hintIndex;
        }

        /// <summary>
        /// Checks hint completion by starting from the back. Only checks upto (not including) <paramref name="forwardsFinalCheck"/>.
        /// </summary>
        /// <param name="line">The row/column to check</param>
        /// <param name="forwardsFinalCheck">The last hint index which <c>DoCompletionForward</c> left off at</param>
        /// <seealso cref="DoCompletionForward(LinkedList{CellType})"/>
        private void DoCompletionBackward(LinkedList<CellType> line, int forwardsFinalCheck)
        {
            LinkedListNode<CellType>? node = line.Last;
            bool startedFromFirst = true; // Whether we started from the first cell in the current iteration of checking
            int hintIndex = hints.Count - 1;
            int cellsFound = 0; // The total cells we have found that are filled in for this hint   

            while (node != null && hintIndex >= 0 && hintIndex > forwardsFinalCheck)
            {
                if (node.Value == CellType.CROSS && startedFromFirst && cellsFound == 0)
                {
                    // The player knows that all cells from the start should not be filled, then we treat this
                    // as if the first cell is placed at the first cell
                    node = node.Previous;
                    continue;
                }

                // Check if we are allowed to mark this hint as completed
                // A hint is allowed to be completed if it started from the first possible cell in the grid
                // Or it has a cross
                if (node.Value == CellType.CROSS || (node.Value == CellType.BLANK && startedFromFirst))
                {
                    if (cellsFound == hints[hintIndex].Number)
                    {
                        hints[hintIndex]._completed = true;
                    }
                    else if (node.Value == CellType.CROSS && cellsFound == 0)
                    {
                        // Check if this is a cross while we have not yet started processing a new hint, then this can still be completed,
                        // As we have not invalidated any hint since crosses are like blank spaces
                        node = node.Previous;
                        continue;
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
                    cellsFound = 0;
                    hintIndex--;
                    continue;
                }

                if (node.Value == CellType.BLANK)
                {
                    // Now, we do not know whether the player knows that these hints are correct or not,
                    // as we require crosses between cells for cells not starting at the first index
                    // We return as we have no other guarantees on other hints. If this was the first checked hint,
                    // we still return to avoid unintuitively marking following hints as complete
                    return;
                }

                // This cell is filled in and should be correct, do the proper variable increments
                node = node.Previous;
                cellsFound++;
            }

            // We can reach a situation where the hint is completed at the end of the grid (i.e. all hints are correct)
            // Then, we should do a final check whether this hint is completed
            if (hintIndex >= 0 && hintIndex > forwardsFinalCheck && hints[hintIndex].Number == cellsFound)
            {
                hints[hintIndex]._completed = true;
            }
        }

        private void SetFullyCompleted()
        {
            foreach (Hint h in this)
            {
                if (!h.Completed)
                {
                    FullyCompleted = false;
                    return;
                }
            }

            FullyCompleted = true;
        }

        /// <summary>
        /// Deep copies this Hints instance.
        /// </summary>
        /// <returns>Deep copy of this instance</returns>
        public object Clone()
        {
            var hintCopy = new List<Hint>();

            foreach(Hint h in hints)
            {
                // Create deep copy of the hints, otherwise interference might occur with user playing
                hintCopy.Add((Hint) h.Clone());
            }

            return new Hints(isColumnHints, position, hintCopy);
        }
        
        /// <summary>
        /// Gets the Enumerator over the <see cref="Hint"/> instances contained in this instance.
        /// </summary>
        /// <returns>Enumerator as above</returns>
        public IEnumerator<Hint> GetEnumerator()
        {
            for (int i = 0; i < hints.Count; i++)
            {
                yield return hints[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets the <see cref="Hint"/> instance located at position <paramref name="index"/> in this instance.
        /// </summary>
        /// <param name="index">Hint to get</param>
        /// <returns><see cref="Hint"/> instance at <paramref name="index"/></returns>
        public Hint this[int index]
        {
            get
            {
                return hints[index];
            }
        }
    }
}
