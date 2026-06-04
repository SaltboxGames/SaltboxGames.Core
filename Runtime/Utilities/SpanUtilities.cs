/*
 * Copyright (c) 2024-2026 Saltbox Games Cooperative
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Runtime.CompilerServices;

namespace SaltboxGames.Core.Utilities
{
    /// <summary>
    /// Provides helper methods for span and reference operations.
    /// </summary>
    public class SpanUtilities
    {
        /// <summary>
        /// Swaps two values by reference.
        /// </summary>
        /// <typeparam name="T">The value type.</typeparam>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap<T>(ref T a, ref T b)
        {
            (a, b) = (b, a);
        }
        
        /// <summary>
        /// Swaps two elements in a span by index.
        /// </summary>
        /// <typeparam name="T">The element type of the span.</typeparam>
        /// <param name="array">The span containing the elements to swap.</param>
        /// <param name="a">The index of the first element.</param>
        /// <param name="b">The index of the second element.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap<T>(Span<T> array, int a, int b)
        {
            Swap(ref array[a], ref array[b]);
        }
    }
}
