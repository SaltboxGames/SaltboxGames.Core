/*
 * Copyright (c) 2024 SaltboxGames, Jonathan Gardner
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
    public static class ShuffleExtensions
    {
        /// <summary>
        /// Simple in place fisher-yates shuffle
        /// </summary>
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
        /// Simple in place fisher-yates shuffle
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Shuffle<T>(this List<T> list)
        {
            Shuffle(list.AsSpan());
        }
#else
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