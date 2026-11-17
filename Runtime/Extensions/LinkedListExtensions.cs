// SPDX-License-Identifier: MPL-2.0
/*
 * Copyright (c) 2024-2026 Saltbox Games Cooperative
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;

namespace SaltboxGames.Core.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="LinkedList{T}"/>.
    /// </summary>
    public static class LinkedListExtensions
    {
        /// <summary>
        /// Retrieve a node's value at the specified index.
        /// </summary>
        /// <typeparam name="T">Element type of the linked list.</typeparam>
        /// <param name="linkedList">The linked list to modify.</param>
        /// <param name="index">Index of node to retrieve value from.</param>
        /// <returns>Value of the node at specified index.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown when the specified index is less than 0 or greater than the linked list's count.</exception>
        public static T GetAt<T>(this LinkedList<T> linkedList, int index)
        {
            if (index < 0 || linkedList.Count <= index)
            {
                throw new IndexOutOfRangeException();
            }
            else if (linkedList.Count <= 0)
            {
                return default;
            }

            return linkedList.GetNodeAt(index).Value;
        }

        /// <summary>
        /// Retrieve a node at the specified index.
        /// </summary>
        /// <typeparam name="T">Element type of the linked list.</typeparam>
        /// <param name="linkedList">The linked list to modify.</param>
        /// <param name="index">Index of node to retrieve.</param>
        /// <returns>Node at the specified index.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown when the specified index is less than 0 or greater than the linked list's count.</exception>
        public static LinkedListNode<T> GetNodeAt<T>(this LinkedList<T> linkedList, int index)
        {
            if (index < 0 || index >= linkedList.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            LinkedListNode<T> node;

            if (index < linkedList.Count / 2)
            {
                node = linkedList.First;

                for (int i = 0; i < index; i++)
                {
                    node = node!.Next;
                }
            }
            else
            {
                node = linkedList.Last;

                for (int i = linkedList.Count - 1; i > index; i--)
                {
                    node = node!.Previous;
                }
            }

            return node;
        }

        /// <summary>
        /// Removes a node at the specified index.
        /// </summary>
        /// <typeparam name="T">Element type of the linked list.</typeparam>
        /// <param name="linkedList">The linked list to modify.</param>
        /// <param name="index">Index of node to remove.</param>
        public static void RemoveAt<T>(this LinkedList<T> linkedList, int index)
        {
            linkedList.Remove(linkedList.GetNodeAt(index));
        }

        /// <summary>
        /// Removes nodes at the end of the linked list until it meets the specified size limit.
        /// </summary>
        /// <typeparam name="T">Element type of the linked list.</typeparam>
        /// <param name="linkedList">The linked list to modify.</param>
        /// <param name="sizeMaximum">Maximum size limit to enforce.</param>
        public static void EnforceSizeLimit<T>(this LinkedList<T> linkedList, int sizeMaximum)
        {
            if (linkedList.Count > sizeMaximum)
            {
                while (linkedList.Count > sizeMaximum)
                {
                    linkedList.RemoveLast();
                }
            }
        }
    }
}
