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
        // So we can avoid copies and allocations
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
#if SIMD_ENABLED
            //This float conversion is only ok because we're not doing math.
            ref System.Numerics.Vector4 selfVec = ref Unsafe.As<SafeGuid, System.Numerics.Vector4>(ref Unsafe.AsRef(in a));
            ref System.Numerics.Vector4 otherVec = ref Unsafe.As<SafeGuid, System.Numerics.Vector4>(ref Unsafe.AsRef(in b));
            return System.Numerics.Vector4.Equals(selfVec, otherVec);
#else
            return a.segment1 == b.segment1 && 
                   a.segment2 == b.segment2 && 
                   a.segment3 == b.segment3 && 
                   a.segment4 == b.segment4;
#endif
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in SafeGuid a, in SafeGuid b)
        {
            return !(a == b);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(SafeGuid other)
        {
            return this == other;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            return obj is SafeGuid other && 
                   Equals(other);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return HashCode.Combine(segment1, segment2, segment3, segment4);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            // This matches the Unity Editor string Formatting;
            return internalValue.ToString("N");
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(string format, IFormatProvider provider)
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
    }
}