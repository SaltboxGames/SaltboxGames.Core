// SPDX-License-Identifier: MPL-2.0
/*
 * Copyright (c) 2026 Saltbox Games Cooperative
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

#if MEMORY_PACK

using MemoryPack;

namespace SaltboxGames.Core.Shims
{
    /// <summary>
    /// Adds MemoryPack serialization support to <see cref="SafeGuid"/>.
    /// </summary>
    [MemoryPackable]
    public partial struct SafeGuid { }
}

#endif
