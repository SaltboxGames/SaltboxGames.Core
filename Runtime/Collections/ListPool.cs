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
    /// Provides a simple reusable pool for <see cref="List{T}"/> instances.
    /// </summary>
    /// <typeparam name="T">The element type of pooled lists.</typeparam>
    public class ListPool<T>
    {
        /// <summary>
        /// Gets the shared list pool for this element type.
        /// </summary>
        public static ListPool<T> Shared = new ListPool<T>();
        
        private Stack<List<T>> pool = new Stack<List<T>>();

        /// <summary>
        /// Rents a list from the pool or creates a new one.
        /// </summary>
        /// <returns>A list instance.</returns>
        public List<T> Rent()
        {
            if (pool.Count > 0)
            {
                return pool.Pop();
            }
            return new List<T>();
        }

        /// <summary>
        /// Clears a list and returns it to the pool.
        /// </summary>
        /// <param name="list">The list to return.</param>
        public void Return(List<T> list)
        {
            list.Clear();
            pool.Push(list);
        }
    }
}
