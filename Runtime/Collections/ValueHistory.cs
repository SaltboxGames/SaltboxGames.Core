// SPDX-License-Identifier: MPL-2.0
/*
 * Copyright (c) 2024-2026 Saltbox Games Cooperative
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Collections;
using System.Collections.Generic;
using SaltboxGames.Core.Extensions;

namespace SaltboxGames.Core.Collections
{
    /// <summary>
    /// Provides a navigatable history of values.
    /// </summary>
    public class ValueHistory<T> : IEnumerable<T>
    {
        private LinkedList<T> history = new LinkedList<T>();
        private LinkedListNode<T> currentNode;
        private bool navigationStarted = false;

        /// <summary>
        /// Count limit of values in history.
        /// </summary>
        public int Limit;
        /// <summary>
        /// Number of values currently in history.
        /// </summary>
        public int Count => history.Count;


        /// <summary>
        /// Creates a new instance of value history.
        /// </summary>
        /// <param name="limit">Count limit of values in history.</param>
        public ValueHistory(int limit = 100)
        {
            this.Limit = limit;
        }

        /// <summary>
        /// Navigates the currently focused node to the next node and returns the node's value. 
        /// </summary>
        /// <returns>Next node's value. If the current node is the last node in the collection it will not navigate and return current node value..</returns>
        public T MoveToNext()
        {
            if (!navigationStarted)
            {
                navigationStarted = true;

                currentNode = history.First;

                if (currentNode == null)
                {
                    return default;
                }

                return currentNode.Value;
            }

            if (currentNode == null)
            {
                return default;
            }

            if (currentNode.Next != null)
            {
                currentNode = currentNode.Next;
            }

            return currentNode.Value;
        }

        /// <summary>
        /// Navigates the currently focused node to the previous node and returns the node's value. 
        /// </summary>
        /// <returns>Previous node's value. If the current node is the first node in the collection it will not navigate and return current node value..</returns>
        public T MoveToPrevious()
        {
            if (currentNode == null)
            {
                return default;
            }

            if (currentNode.Previous != null)
            {
                currentNode = currentNode.Previous;

                return currentNode.Value;
            }

            navigationStarted = false;

            return default;
        }

        /// <summary>
        /// Add a value to the history. By default does not allow duplicate neighbour values.
        /// </summary>
        /// <param name="value">Value to add to the history</param>
        /// <param name="allowDuplicateNeighbours">Whether or not adjacent values can be duplicate.</param>
        public void Add(T value, bool allowDuplicateNeighbours = false)
        {
            // prevent adding duplicate history
            if (!allowDuplicateNeighbours && history.Count > 0 && history.First.Value.Equals(value))
            {
                return;
            }

            history.EnforceSizeLimit(Limit - 1);
            history.AddFirst(value);

            currentNode = history.First;
            navigationStarted = false;
        }

        /// <summary>
        /// Remove node at specified index.
        /// </summary>
        /// <param name="index">Index of node to remove.</param>
        public void Remove(int index)
        {
            history.RemoveAt(index);

            currentNode = history.First;
            navigationStarted = false;
        }

        /// <summary>
        /// Clears all value history.
        /// </summary>
        public void Clear()
        {
            history.Clear();
            currentNode = null;
            navigationStarted = false;
        }


        /// <summary>
        /// Returns a <see cref="List<T>"/> of the current history.
        /// </summary>
        /// <returns><see cref="List<T>"/> of the current history.</returns>
        public List<T> GetHistory()
        {
            List<T> result = new List<T>();

            foreach (T value in this)
            {
                result.Add(value);
            }

            result.Reverse();

            return result;
        }

        /// <summary>
        /// Returns an <see cref="IEnumerator<T>"/> of the current history.
        /// </summary>
        /// <returns><see cref="IEnumerator<T>"/> of the current history.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return history.GetEnumerator();
        }

        /// <summary>
        /// Returns an <see cref="IEnumerator"/> of the current history.
        /// </summary>
        /// <returns><see cref="IEnumerator"/> of the current history.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
