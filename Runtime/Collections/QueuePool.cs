/*
 * Copyright (c) 2024 SaltboxGames
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */


using System.Collections.Generic;

namespace SaltboxGames.Core.Collections
{
    /// <summary>
    /// Provides a simple reusable pool for <see cref="Queue{T}"/> instances.
    /// </summary>
    /// <typeparam name="T">The element type of pooled queues.</typeparam>
    public class QueuePool<T>
    {
        /// <summary>
        /// Gets the shared queue pool for this element type.
        /// </summary>
        public static QueuePool<T> Shared = new QueuePool<T>();
        
        private Stack<Queue<T>> pool = new Stack<Queue<T>>();

        /// <summary>
        /// Rents a queue from the pool or creates a new one.
        /// </summary>
        /// <returns>A queue instance.</returns>
        public Queue<T> Rent()
        {
            if (pool.Count > 0)
            {
                return pool.Pop();
            }
            return new Queue<T>();
        }

        /// <summary>
        /// Clears a queue and returns it to the pool.
        /// </summary>
        /// <param name="list">The queue to return.</param>
        public void Return(Queue<T> list)
        {
            list.Clear();
            pool.Push(list);
        }
    }
}
