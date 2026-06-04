// SPDX-License-Identifier: MPL-2.0
/*
 * Copyright (c) 2024-2026 Saltbox Games Cooperative
 *
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Threading.Tasks;

namespace SaltboxGames.Core.Services
{
    /// <summary>
    /// Represents a managed service with an asynchronous initialization and shutdown lifecycle.
    /// </summary>
    public interface IService
    {
        /// <summary>
        /// Initializes the service and resolves any required dependencies from the provided service initializer.
        /// </summary>
        /// <param name="services">The service initializer available to the service during initialization.</param>
        /// <returns>A task that completes when initialization has finished.</returns>
        public Task InitializeAsync(IServiceInitializer services);
        
        /// <summary>
        /// Starts the service after it has been initialized.
        /// </summary>
        public void StartService();
        
        /// <summary>
        /// Stops the service before shutdown.
        /// </summary>
        public void StopService();
        
        /// <summary>
        /// Releases service resources during shutdown.
        /// </summary>
        /// <returns>A task that completes when shutdown has finished.</returns>
        public Task ShutdownAsync();
    }
}
