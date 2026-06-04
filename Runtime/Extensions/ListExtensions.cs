/*
 * Copyright (c) 2024-2026 Saltbox Games Cooperative
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;

#if !ENABLE_IL2CPP && (UNITY_6000_0_OR_NEWER || UNITY_EDITOR)
using SaltboxGames.Core.Utilities;
using UnityEngine.Assertions;
#elif NET8_0_OR_GREATER
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#endif

namespace SaltboxGames.Core.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="List{T}"/>.
    /// </summary>
    public static class ListExtensions
    {
        /// <summary>
        /// Replaces the element at <paramref name="index"/> with the last element,
        /// and removes the last element. O(1) but does not preserve order.
        /// </summary>
        /// <typeparam name="T">The element type of the list.</typeparam>
        /// <param name="list">The list to modify.</param>
        /// <param name="index">The index of the element to remove.</param>
        public static void RemoveAtSwapBack<T>(this List<T> list, int index)
        {
            int lastIndex = list.Count - 1;
            if (index != lastIndex)
            {
                list[index] = list[lastIndex];
            }

            list.RemoveAt(lastIndex);
        }
        
        /// <summary>
        /// Replaces the item with the last element,
        /// and removes the last element. O(1) but does not preserve order.
        /// </summary>
        /// <typeparam name="T">The element type of the list.</typeparam>
        /// <param name="list">The list to modify.</param>
        /// <param name="item">The item to remove.</param>
        public static void RemoveSwapBack<T>(this List<T> list, T item)
        {
            int index = list.IndexOf(item);
            int lastIndex = list.Count - 1;
            if (index != lastIndex)
            {
                list[index] = list[lastIndex];
            }

            list.RemoveAt(lastIndex);
        }
        
#if !ENABLE_IL2CPP && (UNITY_6000_0_OR_NEWER || UNITY_EDITOR)
        private static class ListReflectionCache<T>
        {
            internal static Func<List<T>, T[]> GetItems;
        }
        
        /// <summary>
        /// Returns a <see cref="Span{T}"/> representing the active portion of the list, without allocations.
        /// This is equivalent to <c>CollectionsMarshal.AsSpan</c>, but uses cached compiled reflection to access the backing array.
        /// </summary>
        /// <typeparam name="T">The element type of the list.</typeparam>
        /// <param name="list">The list to expose as a span.</param>
        /// <returns>A span over the elements in the list (up to <c>list.Count</c>).</returns>
        /// <remarks>This overload is unavailable on IL2CPP targets.</remarks>
        public static Span<T> AsSpan<T>(this List<T> list)
        {
            ListReflectionCache<T>.GetItems ??= Reflection.GetFieldGetter<List<T>, T[]>("_items");
            Assert.IsNotNull(ListReflectionCache<T>.GetItems);
            
            T[] items = ListReflectionCache<T>.GetItems(list);
            return new Span<T>(items, 0, list.Count);
        }
#elif NET8_0_OR_GREATER
        /// <summary>
        /// Returns a <see cref="Span{T}"/> over the list using the standard .NET <c>CollectionsMarshal.AsSpan</c> API.
        /// </summary>
        /// <typeparam name="T">The element type of the list.</typeparam>
        /// <param name="list">The list to expose as a span.</param>
        /// <returns>A span over the elements in the list.</returns>
        /// <remarks>This overload is used for standalone .NET builds.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<T> AsSpan<T>(this List<T> list)
        {
            return CollectionsMarshal.AsSpan(list);
        }
#endif
        
    }
}
