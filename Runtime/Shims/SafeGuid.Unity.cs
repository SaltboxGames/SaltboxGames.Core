// SPDX-License-Identifier: MPL-2.0
/*
 * Copyright (c) 2026 Saltbox Games Cooperative
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

#if UNITY_2019_4_OR_NEWER

using System.Runtime.InteropServices;
using UnityEngine;

namespace SaltboxGames.Core.Shims
{
    /// <summary>
    /// Adds Unity runtime serialization support to <see cref="SafeGuid"/>.
    /// </summary>
    public partial struct SafeGuid
    {
        [SerializeField]
        [FieldOffset(0)]
        private int segment1;

        [SerializeField]
        [FieldOffset(4)]
        private int segment2;

        [SerializeField]
        [FieldOffset(8)]
        private int segment3;

        [SerializeField]
        [FieldOffset(12)]
        private int segment4;
    }
}

#endif
