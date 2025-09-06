/*
 * Copyright (c) 2024 SaltboxGames, Jonathan Gardner
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
    /// Unity Runtime, Editor and MemoryPack friendly Guid
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

        //We know the structs are the same size; 16 Bytes.
        // So we can avoid a copy and alloc
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Guid(in SafeGuid other)
        {
            return Unsafe.As<SafeGuid, Guid>(ref Unsafe.AsRef(in other));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator SafeGuid(in Guid other)
        {
            return Unsafe.As<Guid, SafeGuid>(ref Unsafe.AsRef(in other));
        }
        
#if UNITY_EDITOR
        //Unity's Editor GUID uses a different string formatter
        // this prioritizes the .ToString() being the same instead of the actual byte value
        // This is better for interacting with Addressables.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator SafeGuid(in GUID other)
        {
            Guid.TryParse(other.ToString(), out Guid parsedValue);
            return parsedValue;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator GUID(in SafeGuid other)
        {
            GUID.TryParse(other.internalValue.ToString("N"), out GUID parsedValue);
            return parsedValue;
        }
#endif
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in SafeGuid a, in SafeGuid b)
        {
            //Branch-less long compare
            ref byte aPtr = ref Unsafe.As<SafeGuid, byte>(ref Unsafe.AsRef(in a));
            ref byte bPtr = ref Unsafe.As<SafeGuid, byte>(ref Unsafe.AsRef(in b));
            
            ulong aLo = Unsafe.ReadUnaligned<ulong>(ref aPtr);
            ulong aHi = Unsafe.ReadUnaligned<ulong>(ref Unsafe.Add(ref aPtr, 8));
            
            ulong bLo = Unsafe.ReadUnaligned<ulong>(ref bPtr);
            ulong bHi = Unsafe.ReadUnaligned<ulong>(ref Unsafe.Add(ref bPtr, 8));

            ulong diff = (aLo ^ bLo) | (aHi ^ bHi);
            return diff == 0;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in SafeGuid a, in SafeGuid b)
        {
            return !(a == b);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(SafeGuid other)
        {
            return this == other;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override bool Equals(object obj)
        {
            return obj is SafeGuid other && 
                   Equals(other);
        }

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override string ToString()
        {
            // This matches the Unity Editor string Formatting;
            return internalValue.ToString("N");
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly string ToString(string format, IFormatProvider provider)
        {
            return internalValue.ToString(format, provider);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SafeGuid NewGuid()
        {
            return Guid.NewGuid();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SafeGuid Parse(ReadOnlySpan<char> input)
        {
            return Guid.Parse(input);
        }
        
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
        
        public readonly void Decompose(out int s1, out int s2, out int s3, out int s4)
        {
            s1 = segment1; 
            s2 = segment2; 
            s3 = segment3; 
            s4 = segment4;
        }

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