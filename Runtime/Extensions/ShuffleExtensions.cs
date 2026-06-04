/*
 * Copyright (c) 2024 SaltboxGames
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SaltboxGames.Core.Utilities;

namespace SaltboxGames.Core.Extensions
{
    /// <summary>
    /// Provides deterministic in-place shuffle extension methods.
    /// </summary>
    public static class ShuffleExtensions
    {
        /// <summary>
        /// Shuffles a span in place using a Fisher-Yates shuffle.
        /// </summary>
        /// <typeparam name="T">The element type of the span.</typeparam>
        /// <param name="span">The span to shuffle.</param>
        public static void Shuffle<T>(this Span<T> span)
        {
            int n = span.Length;
            while (n > 1)
            {
                int k = DeterministicRandom.NextInt(n--);
                SpanUtilities.Swap(ref span[n], ref span[k]);
            }
        }

#if !ENABLE_IL2CPP && (UNITY_6000_0_OR_NEWER || UNITY_EDITOR)
        /// <summary>
        /// Shuffles a list in place using a Fisher-Yates shuffle.
        /// </summary>
        /// <typeparam name="T">The element type of the list.</typeparam>
        /// <param name="list">The list to shuffle.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Shuffle<T>(this List<T> list)
        {
            Shuffle(list.AsSpan());
        }
#else
        /// <summary>
        /// Shuffles a list in place using a Fisher-Yates shuffle.
        /// </summary>
        /// <typeparam name="T">The element type of the list.</typeparam>
        /// <param name="list">The list to shuffle.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Shuffle<T>(this List<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                int k = DeterministicRandom.NextInt(n--);
                (list[n], list[k]) = (list[k], list[n]);
            }
        }
#endif    
    }
}
