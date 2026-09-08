using System.Collections;

namespace NonoSharp
{
    /// <summary>
    /// The clues of a line. Each Clue in Clues can be accessed by using the indexer.
    /// E.g.: Clue clueAtI = (Clues) clues[i].
    /// </summary>
    public class Clues : ICloneable, IEnumerable<Clue>
    {
        private List<Clue> clues;
        private bool isColumnClues;
        private int position;

        /// <summary>
        /// The total amount of filled cells these clues concern.
        /// </summary>
        public int TotalCellsInClues {  get; private set; }

        /// <summary>
        /// Whether this line of Clues is fully completed (i.e. all clues are completed).
        /// </summary>
        public bool FullyCompleted { get; private set; }

        /// <summary>
        /// The total number of <see cref="Clue"/> instances contained in this Clues instance.
        /// </summary>
        public int Count { get { return clues.Count; } }

        internal Clues(bool isColumnClues, int position)
        {
            clues = new List<Clue>();
            this.isColumnClues = isColumnClues;
            this.position = position;
            TotalCellsInClues = 0;
        }

        internal Clues(bool isColumnClues, int position, List<Clue> clues)
        {
            this.clues = clues;
            this.isColumnClues = isColumnClues;
            this.position = position;
            TotalCellsInClues = 0;
        }

        /// <summary>
        /// Adds a <c>Clue</c> to this Clues instance. It is appended to the end.
        /// </summary>
        /// <param name="clue">The clue to add</param>
        internal void Add(Clue clue)
        {
            clues.Add(clue);
            TotalCellsInClues += clue.Number;
        }

        /// <summary>
        /// Resets the all clues by setting their completed status to false.
        /// Also resets how many cells we have not handled yet.
        /// </summary>
        internal void Reset()
        {
            foreach (Clue clue in clues)
            {
                clue._completed = false;
            }
        }

        internal void DoCompletion(Grid grid) {
            if (clues.Count == 0) return;
            Reset();
            LinkedList<CellType> line = isColumnClues ? grid.GetColumn(position) : grid.GetRow(position);
            LinkedListNode<CellType>? node = line.First;

            int leftOffAt = DoCompletionForward(line);
            DoCompletionBackward(line, leftOffAt);

            SetFullyCompleted();
        }

        internal void DoCompletion(CellType[] line)
        {
            if (clues.Count == 0) return;
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
        /// Checks clue completion by starting from the front
        /// </summary>
        /// <param name="line">The row/column to check</param>
        /// <returns>The final clue which was marked as completed</returns>
        /// <seealso cref="DoCompletionBackward(LinkedList{CellType}, int)"/>
        private int DoCompletionForward(LinkedList<CellType> line)
        {
            LinkedListNode<CellType>? node = line.First;
            bool startedFromFirst = true; // Whether we started from the first cell in the current iteration of checking
            int clueIndex = 0;
            int cellsFound = 0; // The total cells we have found that are filled in for this clue

            while (node != null && clueIndex < clues.Count)
            {
                if (node.Value == CellType.CROSS && startedFromFirst && cellsFound == 0)
                {
                    // The player knows that all cells from the start should not be filled, then we treat this
                    // as if the first cell is placed at the first cell
                    node = node.Next;
                    continue;
                }

                // Check if we are allowed to mark this clue as completed
                // A clue is allowed to be completed if it started from the first possible cell in the grid
                // Or it has a cross
                bool firstGroupCompleteFollowedByBlank = node.Value == CellType.BLANK && startedFromFirst;
                if (node.Value == CellType.CROSS || firstGroupCompleteFollowedByBlank)
                {
                    if (cellsFound == clues[clueIndex].Number)
                    {
                        clues[clueIndex]._completed = true;
                    }
                    else if (node.Value == CellType.CROSS && cellsFound == 0)
                    {
                        // Check if this is a cross while we have not yet started processing a new clue, then this can still be completed,
                        // As we have not invalidated any clue since crosses are like blank spaces
                        node = node.Next;
                        continue;
                    }
                    else
                    {
                        // This clue is not completed, meaning all other clues are incorrect as well
                        // We stop this loop
                        return clueIndex - 1;
                    }

                    if (firstGroupCompleteFollowedByBlank)
                    {
                        // Immediatly stop checking if the first clue group was not seperated by a cross
                        return clueIndex;
                    }

                    // Reset variables for next iteration
                    startedFromFirst = false;
                    node = node.Next;
                    cellsFound = 0;
                    clueIndex++;
                    continue;
                }

                if (node.Value == CellType.BLANK && !startedFromFirst)
                {
                    // Now, we do not know whether the player knows that these clues are correct or not,
                    // as we require crosses between cells for cells not starting at the first index
                    // We return as we have no other garauntees on other clues
                    return clueIndex - 1;
                }

                // This cell is filled in and should be correct, do the proper variable increments
                node = node.Next;
                cellsFound++;
            }


            // We can reach a situation where the clue is completed at the end of the grid (i.e. all clues are correct)
            // Then, we should do a final check whether this clue is completed
            if (clueIndex <  clues.Count && clues[clueIndex].Number == cellsFound)
            {
                clues[clueIndex]._completed = true;
                clueIndex++; // Increase clue index as this clue is completed
            }
            return clueIndex;
        }

        /// <summary>
        /// Checks clue completion by starting from the back. Only checks upto (not including) <paramref name="forwardsFinalCheck"/>.
        /// </summary>
        /// <param name="line">The row/column to check</param>
        /// <param name="forwardsFinalCheck">The last clue index which <c>DoCompletionForward</c> left off at</param>
        /// <seealso cref="DoCompletionForward(LinkedList{CellType})"/>
        private void DoCompletionBackward(LinkedList<CellType> line, int forwardsFinalCheck)
        {
            LinkedListNode<CellType>? node = line.Last;
            bool startedFromFirst = true; // Whether we started from the first cell in the current iteration of checking
            int clueIndex = clues.Count - 1;
            int cellsFound = 0; // The total cells we have found that are filled in for this clue   

            while (node != null && clueIndex >= 0 && clueIndex > forwardsFinalCheck)
            {
                if (node.Value == CellType.CROSS && startedFromFirst && cellsFound == 0)
                {
                    // The player knows that all cells from the start should not be filled, then we treat this
                    // as if the first cell is placed at the first cell
                    node = node.Previous;
                    continue;
                }

                // Check if we are allowed to mark this clue as completed
                // A clue is allowed to be completed if it started from the first possible cell in the grid
                // Or it has a cross
                if (node.Value == CellType.CROSS || (node.Value == CellType.BLANK && startedFromFirst))
                {
                    if (cellsFound == clues[clueIndex].Number)
                    {
                        clues[clueIndex]._completed = true;
                    }
                    else if (node.Value == CellType.CROSS && cellsFound == 0)
                    {
                        // Check if this is a cross while we have not yet started processing a new clue, then this can still be completed,
                        // As we have not invalidated any clue since crosses are like blank spaces
                        node = node.Previous;
                        continue;
                    }
                    else
                    {
                        // This clue is not completed, meaning all other clues are incorrect as well
                        // We stop this loop
                        return;
                    }

                    // Reset variables for next iteration
                    startedFromFirst = false;
                    node = node.Previous;
                    cellsFound = 0;
                    clueIndex--;
                    continue;
                }

                if (node.Value == CellType.BLANK)
                {
                    // Now, we do not know whether the player knows that these clues are correct or not,
                    // as we require crosses between cells for cells not starting at the first index
                    // We return as we have no other guarantees on other clues. If this was the first checked clue,
                    // we still return to avoid unintuitively marking following clues as complete
                    return;
                }

                // This cell is filled in and should be correct, do the proper variable increments
                node = node.Previous;
                cellsFound++;
            }

            // We can reach a situation where the clue is completed at the end of the grid (i.e. all clues are correct)
            // Then, we should do a final check whether this clue is completed
            if (clueIndex >= 0 && clueIndex > forwardsFinalCheck && clues[clueIndex].Number == cellsFound)
            {
                clues[clueIndex]._completed = true;
            }
        }

        private void SetFullyCompleted()
        {
            foreach (Clue h in this)
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
        /// Deep copies this Clues instance.
        /// </summary>
        /// <returns>Deep copy of this instance</returns>
        public object Clone()
        {
            var clueCopy = new List<Clue>();

            foreach(Clue h in clues)
            {
                // Create deep copy of the clues, otherwise interference might occur with user playing
                clueCopy.Add((Clue) h.Clone());
            }

            return new Clues(isColumnClues, position, clueCopy);
        }
        
        /// <summary>
        /// Gets the Enumerator over the <see cref="Clue"/> instances contained in this instance.
        /// </summary>
        /// <returns>Enumerator as above</returns>
        public IEnumerator<Clue> GetEnumerator()
        {
            for (int i = 0; i < clues.Count; i++)
            {
                yield return clues[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets the <see cref="Clue"/> instance located at position <paramref name="index"/> in this instance.
        /// </summary>
        /// <param name="index">Clue to get</param>
        /// <returns><see cref="Clue"/> instance at <paramref name="index"/></returns>
        public Clue this[int index]
        {
            get
            {
                return clues[index];
            }
        }
    }
}
