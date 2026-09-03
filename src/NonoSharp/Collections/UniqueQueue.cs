using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace NonoSharp.Collections
{
    /// <summary>
    /// A queue where each element can only appear once in the queue. Works on a FIFO basis.
    /// This class is based on <see cref="Queue"/> and is expected to behave similary, except for the restriction
    /// on uniqueness.
    /// </summary>
    /// <typeparam name="T">The type of the elements to use in the queue.</typeparam>
    internal class UniqueQueue<T> : IEnumerable<T>
    {
        private readonly Queue<T> queue;
        private readonly HashSet<T> enqueued;

        /// <summary>
        /// The total number of elements currently in the queue.
        /// </summary>
        public int Count { get { return queue.Count; } }

        public UniqueQueue()
        {
            queue = [];
            enqueued = [];
        }

        /// <summary>
        /// Enqueues <paramref name="element"/>. If <paramref name="element"/> is already in the queue, nothing changes.
        /// </summary>
        /// <param name="element">The element to enqueue.</param>
        public void Enqueue(T element)
        {
            if (enqueued.Add(element)) // Returns true if element was not present yet
            {
                queue.Enqueue(element);
            }

        }

        /// <summary>
        /// Removes the first element from the queue.
        /// </summary>
        /// <returns>The dequeued element</returns>
        /// <exception cref="InvalidOperationException">Thrown when there was no element to dequeue.</exception>
        public T Dequeue()
        {
            T element = queue.Dequeue();
            enqueued.Remove(element);
            return element;
        }

        /// <summary>
        /// Returns the first element of the queue without removing it.
        /// </summary>
        /// <returns>The first element of the queue, if any.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the queue is empty.</exception>
        public T Peek()
        {
            return queue.Peek();
        }

        /// <summary>
        /// Determines whether <paramref name="element"/> is currently in the queue.
        /// </summary>
        /// <param name="element">The element to check for.</param>
        /// <returns><see langword="true"/> if the element is in the queue, <see langword="false"/> otherwise</returns>
        public bool Contains(T element)
        {
            return enqueued.Contains(element);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return queue.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
