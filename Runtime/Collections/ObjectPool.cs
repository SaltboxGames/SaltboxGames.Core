/*
 * Copyright (c) 2024 SaltboxGames, Jonathan Gardner
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */


using System;
using System.Collections.Generic;

namespace SaltboxGames.Core.Collections
{
    public class ObjectPool<T> where T : new()
    {
        public static ObjectPool<T> Shared = new ObjectPool<T>();
        
        [ThreadStatic]
        private readonly Stack<T> _pool;

        public ObjectPool(int initialCapacity = 0)
        {
            _pool = new Stack<T>(initialCapacity);
            for (int i = 0; i < initialCapacity; i++)
            {
                _pool.Push(new T());
            }
        }

        public T Rent()
        {
            if (_pool.Count > 0)
            {
                return _pool.Pop();
            }
            return new T();
        }

        public void Return(T item)
        {
            _pool.Push(item);
        }
    }
}
