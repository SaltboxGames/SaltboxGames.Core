// SPDX-License-Identifier: MPL-2.0
/*
 * Copyright (c) 2024-2026 Saltbox Games Cooperative
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

namespace SaltboxGames.Core.Services
{
    /// <summary>
    /// Describes the lifecycle state of a service scope or service entry.
    /// </summary>
    public enum ServiceScopeState
    {
        /// <summary>
        /// The scope or service is registered but has not started initialization.
        /// </summary>
        Registered,
        
        /// <summary>
        /// The scope or service is currently initializing.
        /// </summary>
        Initializing,
        
        /// <summary>
        /// The scope or service has initialized but has not started.
        /// </summary>
        Initialized,
        
        /// <summary>
        /// The scope or service is started.
        /// </summary>
        Started,
        
        /// <summary>
        /// The scope or service is currently stopping.
        /// </summary>
        Stopping,
        
        /// <summary>
        /// The scope or service has stopped but has not shut down.
        /// </summary>
        Stopped,
        
        /// <summary>
        /// The scope or service is currently shutting down.
        /// </summary>
        ShuttingDown,
        
        /// <summary>
        /// The scope or service has shut down and can no longer resolve services.
        /// </summary>
        Shutdown
    }
}
