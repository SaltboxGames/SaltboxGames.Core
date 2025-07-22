/*
 * Copyright (c) 2024 SaltboxGames, Jonathan Gardner
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */


using System.Collections.Generic;

namespace SaltboxGames.Core.Collections
{
    public class QueuePool<T>
    {
        public static QueuePool<T> Shared = new QueuePool<T>();
        
        private Stack<Queue<T>> pool = new Stack<Queue<T>>();
        public Queue<T> Rent()
        {
            if (pool.Count > 0)
            {
                return pool.Pop();
            }
            return new Queue<T>();
        }

        public void Return(Queue<T> list)
        {
            list.Clear();
            pool.Push(list);
        }
    }
}
