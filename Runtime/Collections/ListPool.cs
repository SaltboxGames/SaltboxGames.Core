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
    public class ListPool<T>
    {
        public static ListPool<T> Shared = new ListPool<T>();
        
        private Stack<List<T>> pool = new Stack<List<T>>();
        public List<T> Rent()
        {
            if (pool.Count > 0)
            {
                return pool.Pop();
            }
            return new List<T>();
        }

        public void Return(List<T> list)
        {
            list.Clear();
            pool.Push(list);
        }
    }
}
