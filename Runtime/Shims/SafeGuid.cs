// SPDX-License-Identifier: MPL-2.0
/*
 * Copyright (c) 2024-2026 Saltbox Games Cooperative
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#if MEMORY_PACK
using MemoryPack;
#endif

#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_2019_4_OR_NEWER
using UnityEngine;
#endif

namespace SaltboxGames.Core.Shims
{
    /// <summary>
    /// Represents a Unity runtime, Unity editor, and MemoryPack-friendly GUID value.
    /// </summary>
#if MEMORY_PACK
    [MemoryPackable]
#endif
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public partial struct SafeGuid : IEquatable<SafeGuid>, IFormattable
    {
        [FieldOffset(0)]
        private Guid internalValue;
        
#if UNITY_2019_4_OR_NEWER
        [SerializeField]
#endif
        [FieldOffset(0)]
        private Int32 segment1;

#if UNITY_2019_4_OR_NEWER
        [SerializeField]
#endif
        [FieldOffset(4)]
        private Int32 segment2;

#if UNITY_2019_4_OR_NEWER
        [SerializeField]
#endif
        [FieldOffset(8)]
        private Int32 segment3;

#if UNITY_2019_4_OR_NEWER
        [SerializeField]
#endif
        [FieldOffset(12)]
        private Int32 segment4;

        /// <summary>
        /// Converts a <see cref="SafeGuid"/> to a <see cref="Guid"/>.
        /// </summary>
        /// <param name="other">The value to convert.</param>
        /// <returns>The equivalent <see cref="Guid"/> value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Guid(in SafeGuid other)
        {
            return Unsafe.As<SafeGuid, Guid>(ref Unsafe.AsRef(in other));
        }

        /// <summary>
        /// Converts a <see cref="Guid"/> to a <see cref="SafeGuid"/>.
        /// </summary>
        /// <param name="other">The value to convert.</param>
        /// <returns>The equivalent <see cref="SafeGuid"/> value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator SafeGuid(in Guid other)
        {
            return Unsafe.As<Guid, SafeGuid>(ref Unsafe.AsRef(in other));
        }
        
#if UNITY_EDITOR
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
#endif
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static nuint Diff(in SafeGuid a, in SafeGuid b)
        {
            //Branch-less long compare
            ref byte aPtr = ref Unsafe.As<SafeGuid, byte>(ref Unsafe.AsRef(in a));
            ref byte bPtr = ref Unsafe.As<SafeGuid, byte>(ref Unsafe.AsRef(in b));
            
            ulong aLo = Unsafe.ReadUnaligned<ulong>(ref aPtr);
            ulong aHi = Unsafe.ReadUnaligned<ulong>(ref Unsafe.Add(ref aPtr, 8));
            
            ulong bLo = Unsafe.ReadUnaligned<ulong>(ref bPtr);
            ulong bHi = Unsafe.ReadUnaligned<ulong>(ref Unsafe.Add(ref bPtr, 8));

            ulong diff = (aLo ^ bLo) | (aHi ^ bHi); 
            if (IntPtr.Size == 4) // <-- Compile time constant; folded away by JIT/IL2CPP
            {
                // slightly more performant on ARM32 systems;
                return (nuint)(diff | diff >> 32);
            }
            
            return (nuint)diff;
        }
        
        /// <summary>
        /// Determines whether two <see cref="SafeGuid"/> values are equal.
        /// </summary>
        /// <param name="a">The first value to compare.</param>
        /// <param name="b">The second value to compare.</param>
        /// <returns><see langword="true"/> when the values are equal; otherwise <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in SafeGuid a, in SafeGuid b)
        {
            return Diff(a, b) == 0;
        }
        
        /// <summary>
        /// Determines whether two <see cref="SafeGuid"/> values are not equal.
        /// </summary>
        /// <param name="a">The first value to compare.</param>
        /// <param name="b">The second value to compare.</param>
        /// <returns><see langword="true"/> when the values are not equal; otherwise <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in SafeGuid a, in SafeGuid b)
        {
            return Diff(a, b) != 0;
        }
        
        /// <summary>
        /// Determines whether this value equals another <see cref="SafeGuid"/>.
        /// </summary>
        /// <param name="other">The value to compare with this instance.</param>
        /// <returns><see langword="true"/> when the values are equal; otherwise <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(SafeGuid other)
        {
            return Diff(this, other) == 0;
        }
        
        /// <summary>
        /// Determines whether this value equals another object.
        /// </summary>
        /// <param name="obj">The object to compare with this instance.</param>
        /// <returns><see langword="true"/> when <paramref name="obj"/> is an equal <see cref="SafeGuid"/>; otherwise <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object obj)
        {
            return obj is SafeGuid other && Diff(this, other) == 0;
        }

        /// <summary>
        /// Gets a hash code for this value.
        /// </summary>
        /// <returns>A hash code for this value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode()
        {
            const ulong hashMultiplier = 0x9E3779B97F4A7C15ul;
            const ulong splitMix = 0x94D049BB133111EBul;
            
            ref byte ptr = ref Unsafe.As<SafeGuid, byte>(ref Unsafe.AsRef(in this));
            
            ulong lo = Unsafe.ReadUnaligned<ulong>(ref ptr);
            ulong hi = Unsafe.ReadUnaligned<ulong>(ref Unsafe.Add(ref ptr, 8));
            
            unchecked
            {
                // Branch-less 128->64->32 mix 
                ulong x = lo ^ (hi * hashMultiplier);
                x ^= x >> 32;
                x *= hashMultiplier;
                x ^= x >> 29;
                x *= splitMix;
                x ^= x >> 32;
                return (int)x;
            }
        }

        /// <summary>
        /// Formats this value as a 32-digit string without hyphens.
        /// </summary>
        /// <returns>The formatted GUID string.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override string ToString()
        {
            // This matches the Unity Editor string Formatting;
            return internalValue.ToString("N");
        }
        
        /// <summary>
        /// Formats this value using the specified GUID format.
        /// </summary>
        /// <param name="format">A standard GUID format string.</param>
        /// <param name="provider">An object that supplies culture-specific formatting information.</param>
        /// <returns>The formatted GUID string.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly string ToString(string format, IFormatProvider provider)
        {
            return internalValue.ToString(format, provider);
        }

        /// <summary>
        /// Creates a new <see cref="SafeGuid"/> value.
        /// </summary>
        /// <returns>A new GUID value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SafeGuid NewGuid()
        {
            return Guid.NewGuid();
        }
        
        /// <summary>
        /// Parses a GUID string into a <see cref="SafeGuid"/>.
        /// </summary>
        /// <param name="input">The GUID string to parse.</param>
        /// <returns>The parsed GUID value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SafeGuid Parse(ReadOnlySpan<char> input)
        {
            return Guid.Parse(input);
        }
        
        /// <summary>
        /// Attempts to parse a GUID string into a <see cref="SafeGuid"/>.
        /// </summary>
        /// <param name="input">The GUID string to parse.</param>
        /// <param name="result">The parsed value when parsing succeeds; otherwise the default value.</param>
        /// <returns><see langword="true"/> when parsing succeeds; otherwise <see langword="false"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryParse(ReadOnlySpan<char> input, out SafeGuid result)
        {
            if (Guid.TryParse(input, out Guid guid))
            {
                result = guid;
                return true;
            }
            result = default;
            return false;
        }
        
        /// <summary>
        /// Decomposes this value into four serialized integer segments.
        /// </summary>
        /// <param name="s1">The first segment.</param>
        /// <param name="s2">The second segment.</param>
        /// <param name="s3">The third segment.</param>
        /// <param name="s4">The fourth segment.</param>
        public readonly void Decompose(out int s1, out int s2, out int s3, out int s4)
        {
            s1 = segment1; 
            s2 = segment2; 
            s3 = segment3; 
            s4 = segment4;
        }

        /// <summary>
        /// Creates a <see cref="SafeGuid"/> from four serialized integer segments.
        /// </summary>
        /// <param name="s1">The first segment.</param>
        /// <param name="s2">The second segment.</param>
        /// <param name="s3">The third segment.</param>
        /// <param name="s4">The fourth segment.</param>
        /// <returns>A GUID value composed from the provided segments.</returns>
        public static SafeGuid FromSegments(int s1, int s2, int s3, int s4)
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
