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

namespace SaltboxGames.Core.Collections
{
    /// <summary>
    /// Provides a simple reusable pool for objects with public parameterless constructors.
    /// </summary>
    /// <typeparam name="T">The pooled object type.</typeparam>
    public class ObjectPool<T> where T : new()
    {
        /// <summary>
        /// Gets the shared object pool for this type.
        /// </summary>
        public static ObjectPool<T> Shared = new ObjectPool<T>();
        
        private readonly Stack<T> _pool;

        /// <summary>
        /// Creates a new object pool with optional preallocated items.
        /// </summary>
        /// <param name="initialCapacity">The number of items to create immediately.</param>
        public ObjectPool(int initialCapacity = 0)
        {
            _pool = new Stack<T>(initialCapacity);
            for (int i = 0; i < initialCapacity; i++)
            {
                _pool.Push(new T());
            }
        }

        /// <summary>
        /// Rents an object from the pool or creates a new one.
        /// </summary>
        /// <returns>An object instance.</returns>
        public T Rent()
        {
            if (_pool.Count > 0)
            {
                return _pool.Pop();
            }
            return new T();
        }

        /// <summary>
        /// Returns an object to the pool.
        /// </summary>
        /// <param name="item">The object to return.</param>
        /// <remarks>The object state is not reset by the pool.</remarks>
        public void Return(T item)
        {
            _pool.Push(item);
        }
    }
}
