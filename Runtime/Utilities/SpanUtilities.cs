using System;
using System.Runtime.CompilerServices;

namespace SaltboxGames.Core.Utilities
{
    public class SpanUtilities
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap<T>(ref T a, ref T b)
        {
            (a, b) = (b, a);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Swap<T>(Span<T> array, int a, int b)
        {
            Swap(ref array[a], ref array[b]);
        }
    }
}
