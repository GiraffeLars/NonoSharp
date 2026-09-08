using System.Collections;

namespace NonoSharp
{
    /// <summary>
    /// The clues of a line. Each Clue in Clues can be accessed by using the indexer.
    /// E.g.: Clue clueAtI = (Clues) clues[i].
    /// </summary>
    public class Clues : ICloneable, IEnumerable<Clue>
    {
        /// TODO: Switch to private/private protected if necessary when removing Hints
        internal List<Clue> clues;

        /// <summary>
        /// The total amount of filled cells these clues concern.
        /// </summary>
        public int TotalCellsInClues { get; private protected set; } // TODO: Set to private set when removing Hints

        /// <summary>
        /// Whether this line of Clues is fully completed (i.e. all clues are completed).
        /// </summary>
        public bool FullyCompleted { get; private set; } = false;

        /// <summary>
        /// The total number of <see cref="Clue"/> instances contained in this Clues instance.
        /// </summary>
        public int Count { get { return clues.Count; } }

        /// <summary>
        /// Creates an empty Clues instance.
        /// </summary>
        public Clues()
        {
            this.clues = [];
            TotalCellsInClues = 0;
        }
        

        /// <summary>
        /// Creates Clues instance, representing a collection of <see cref="Clue"/>s from <paramref name="clues"/>.
        /// </summary>
        /// <param name="clues">List containing Clue instances to group in this instance</param>
        public Clues(List<Clue> clues)
        {
            this.clues = [.. clues]; // Copy list as we do not want unexpected modifications
            TotalCellsInClues = clues.Select(clue => clue.Number).Sum();
            SetFullyCompleted();
        }
        

        /// <summary>
        /// Adds a <see cref="Clue"/> to this Clues instance. It is appended to the end.
        /// </summary>
        /// <param name="clue">The clue to add.</param>
        public void Add(Clue clue)
        {
            clues.Add(clue);
            TotalCellsInClues += clue.Number;

            if (!clue.Completed)
            {
                FullyCompleted = false;
            }

            // If the clue was completed, whether this set of clues is fully completed still only depends on
            // the clues that were here before adding this clue. As such, FullyCompleted does not need to change
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

        /// <summary>
        /// Checks for completed clues, from front to back and back to front. If a clue can not be completed,
        /// the following clues are not checked and assumed to be incomplete.
        /// </summary>
        /// <param name="line">The line to check the clues on</param>
        /// <exclude />
        protected internal void DoCompletion(CellType[] line)
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
            SetFullyCompleted();
        }

        /// <summary>
        /// Checks clue completion by starting from the front
        /// </summary>
        /// <param name="line">The row/column to check</param>
        /// <returns>The final clue which was marked as completed</returns>
        /// <seealso cref="DoCompletionBackward(LinkedList{CellType}, int)"/>
        private protected int DoCompletionForward(LinkedList<CellType> line)
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
        private protected void DoCompletionBackward(LinkedList<CellType> line, int forwardsFinalCheck)
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

        /// <summary>
        /// Checks if all clues in this instance are completed and sets 
        /// </summary>
        /// <exclude />
        protected void SetFullyCompleted()
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
        /// Converts a string in a form like "X X X X", where each X is an integer, to a Clues instance.
        /// Each X is converted into a <see cref="Clue"/> with Clue.Number matching X.
        /// </summary>
        /// <remarks>
        /// X must be non-negative. If constructing an empty Clues instance (one without any
        /// filled cells in the line), 0 must be the only number in the string. 0 is only allowed to appear <paramref name="str"/>
        /// on its own, with no other numbers.
        /// </remarks>
        /// <param name="str">The string to convert</param>
        /// <returns>A Clues instance as above.</returns>
        /// <exception cref="FormatException">Thrown when any of the numbers is not a valid integer or 
        /// is of an incorrect format in any other way</exception>
        /// <exception cref="OverflowException">Thrown when any of the numbers is not between <see cref="Int32.MinValue"/>
        /// and <see cref="Int32.MaxValue"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when any parsed number X is negative or 0 appears in a string
        /// with other clues.</exception>
        public static Clues FromString(string str)
        {
            string[] splitted = str.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            Clues clues = new();

            foreach (string s in splitted)
            {
                int parsed = int.Parse(s);
                if (parsed < 0)
                {
                    throw new ArgumentException($"Parsed number {parsed} in {nameof(str)} must be non-negative!", nameof(str));
                }
                if (parsed == 0 && splitted.Length > 1)
                {
                    throw new ArgumentException("Can not have a clue with 0 filled cells if there are other clues!", nameof(str));
                }

                clues.Add(new(parsed));
            }

            return clues;
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

            return new Clues(clueCopy);
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
