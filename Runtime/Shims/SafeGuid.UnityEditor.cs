// SPDX-License-Identifier: MPL-2.0
/*
 * Copyright (c) 2026 Saltbox Games Cooperative
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

#if UNITY_EDITOR

using System;
using System.Runtime.CompilerServices;
using UnityEditor;

namespace SaltboxGames.Core.Shims
{
    /// <summary>
    /// Adds Unity editor GUID conversion support to <see cref="SafeGuid"/>.
    /// </summary>
    public partial struct SafeGuid
    {
        /// <summary>
        /// Converts a Unity editor GUID to a <see cref="SafeGuid"/> using Unity's string representation.
        /// </summary>
        /// <param name="other">The Unity editor GUID to convert.</param>
        /// <returns>The equivalent <see cref="SafeGuid"/> value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator SafeGuid(in GUID other)
        {
            Guid.TryParse(other.ToString(), out Guid parsedValue);
            return parsedValue;
        }

        /// <summary>
        /// Converts a <see cref="SafeGuid"/> to a Unity editor GUID using Unity's string representation.
        /// </summary>
        /// <param name="other">The value to convert.</param>
        /// <returns>The equivalent Unity editor GUID value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator GUID(in SafeGuid other)
        {
            GUID.TryParse(other.internalValue.ToString("N"), out GUID parsedValue);
            return parsedValue;
        }
        
        /// <summary>
        /// Decomposes this value into four serialized integer segments for Unity package bridges.
        /// </summary>
        /// <param name="s1">The first segment.</param>
        /// <param name="s2">The second segment.</param>
        /// <param name="s3">The third segment.</param>
        /// <param name="s4">The fourth segment.</param>
        internal readonly void Decompose(out int s1, out int s2, out int s3, out int s4)
        {
            s1 = segment1;
            s2 = segment2;
            s3 = segment3;
            s4 = segment4;
        }

        /// <summary>
        /// Creates a <see cref="SafeGuid"/> from four serialized integer segments for Unity package bridges.
        /// </summary>
        /// <param name="s1">The first segment.</param>
        /// <param name="s2">The second segment.</param>
        /// <param name="s3">The third segment.</param>
        /// <param name="s4">The fourth segment.</param>
        /// <returns>A GUID value composed from the provided segments.</returns>
        internal static SafeGuid FromSegments(int s1, int s2, int s3, int s4)
        {
            Unsafe.SkipInit(out SafeGuid guid);
            guid.segment1 = s1;
            guid.segment2 = s2;
            guid.segment3 = s3;
            guid.segment4 = s4;
            return guid;
        }
    }
}

#endif
