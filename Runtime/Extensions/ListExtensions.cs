/*
 * Copyright (c) 2024 SaltboxGames, Jonathan Gardner
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using SaltboxGames.Core.Utilities;

#if UNITY_2021_1_OR_NEWER
using UnityEngine.Assertions;

#else
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#endif

namespace SaltboxGames.Core.Extensions
{
    public static class ListExtensions
    {
#if UNITY_2021_1_OR_NEWER
        private static class ListReflectionCache<T>
        {
            public static Func<List<T>, T[]> GetItems;
        }
        
        /// <summary>
        /// Returns a <see cref="Span{T}"/> representing the active portion of the list, without allocations.
        /// This is equivalent to <c>CollectionsMarshal.AsSpan</c>, but uses cached compiled reflection to access the backing array.
        /// </summary>
        /// <typeparam name="T">The element type of the list.</typeparam>
        /// <param name="list">The list to expose as a span.</param>
        /// <returns>A span over the elements in the list (up to <c>list.Count</c>).</returns>
        public static Span<T> AsSpan<T>(this List<T> list)
        {
            ListReflectionCache<T>.GetItems ??= Reflection.GetFieldGetter<List<T>, T[]>("_items");

            Assert.IsNotNull(ListReflectionCache<T>.GetItems);
            
            T[] items = ListReflectionCache<T>.GetItems(list);
            return new Span<T>(items, 0, list.Count);
        }
#else

        /// <summary>
        /// Returns a <see cref="Span{T}"/> over the list using the standard <c>CollectionsMarshal.AsSpan</c> API.
        /// </summary>
        /// <typeparam name="T">The element type of the list.</typeparam>
        /// <param name="list">The list to expose as a span.</param>
        /// <returns>A span over the elements in the list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(this List<T> list)
        {
            return CollectionsMarshal.AsSpan(list);
        }
#endif
        
    }
}
