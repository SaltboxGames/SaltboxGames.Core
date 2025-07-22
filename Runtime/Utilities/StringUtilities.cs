/*
 * Copyright (c) 2024 SaltboxGames, Jonathan Gardner
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */


using System;
using System.Collections.Generic;

#if !UNITY_2019_1_OR_NEWER
using System.Runtime.InteropServices;
#endif

namespace SaltboxGames.Core.Utilities
{
    public static class StringUtilities
    {
#if !UNITY_2019_1_OR_NEWER
        public static string LongestCommonPrefix(List<string> strings)
        {
            Span<string> span = CollectionsMarshal.AsSpan(strings);
            return LongestCommonPrefix(span);
        }
#else
        public static string LongestCommonPrefix(List<string> strings)
        {
            if (strings.Count == 0)
            {
                return "";
            }
            string prefix = strings[0];

            for (int index = 1; index < strings.Count; index++)
            {
                string str = strings[index];
                
                int i = 0;
                while (i < prefix.Length && i < str.Length && prefix[i] == str[i])
                {
                    i++;
                }
                prefix = prefix.Substring(0, i);
                if (prefix == "")
                {
                    break;
                }
            }

            return prefix;
        }
#endif

        public static string LongestCommonPrefix(ReadOnlySpan<string> strings)
        {
            if (strings.Length == 0)
            {
                return "";
            }
            string prefix = strings[0];

            for (int index = 1; index < strings.Length; index++)
            {
                string str = strings[index];
                
                int i = 0;
                while (i < prefix.Length && i < str.Length && prefix[i] == str[i])
                {
                    i++;
                }
                prefix = prefix.Substring(0, i);
                if (prefix == "")
                {
                    break;
                }
            }

            return prefix;
        }
    }
}
